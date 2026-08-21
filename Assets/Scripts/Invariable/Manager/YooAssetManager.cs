using HybridCLR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;



namespace Invariable
{
    public class YooAssetManager : Singleton<YooAssetManager>
    {
        private const float EvictIdleSeconds = 180f;
        private const float EvictScanIntervalSeconds = 30f;
        private ResourcePackage m_package = null;
        private Dictionary<string, AssetHandle> m_assetHandles = null;
        private Dictionary<string, List<Action<object>>> m_pendingCallbacks = null;
        private Dictionary<string, float> m_lastAccessTimes = null;
        private Dictionary<string, SceneHandle> m_sceneHandles = null;
        private Assembly m_hotUpdateAssembly = null;
        private bool m_isEvictTimerStarted = false;

        public ResourcePackage Package
        {
            get
            {
                if (m_package == null)
                {
                    m_package = YooAssets.TryGetPackage(PackageName);

                    if (m_package == null)
                    {
                        m_package = YooAssets.CreatePackage(PackageName);
                    }

                    YooAssets.SetDefaultPackage(m_package);
                }

                return m_package;
            }
        }

        public string PackageName
        {
            get
            {
                return InvariableConst.YooAssetPackageName;
            }
        }

        public Assembly HotUpdateAssembly
        {
            get
            {
                return m_hotUpdateAssembly;
            }
        }



        /// <summary>
        /// 预加载Dll
        /// </summary>
        /// <param name="callBack">预加载完成回调</param>
        public void PreLoadDll(Action<Assembly> callBack)
        {
            if (m_hotUpdateAssembly != null)
            {
                callBack?.Invoke(m_hotUpdateAssembly);

                return;
            }

#if UNITY_EDITOR
            Assembly hotUpdateAss = AppDomain.CurrentDomain.GetAssemblies().First((a) => a.GetName().Name == "HotUpdate");
            m_hotUpdateAssembly = hotUpdateAss;
            callBack?.Invoke(hotUpdateAss);

            return;
#endif

            LoadMetadataForAOTAssemblies("MiniGame", callBack);
        }

        /// <summary>
        /// 补充元数据（AOT DLL 并行加载，全部完成后加载 HotUpdate）
        /// </summary>
        /// <param name="platform">平台标识</param>
        /// <param name="callBack">加载完成回调</param>
        private void LoadMetadataForAOTAssemblies(string platform, Action<Assembly> callBack)
        {
            string[] aotDllList = InvariableConst.AotDllNames;
            int remaining = aotDllList.Length;

            for (int i = 0; i < aotDllList.Length; i++)
            {
                string aotDllName = aotDllList[i];

                AsyncLoadAsset<BinAsset>($"{platform}_{aotDllName}.dll", (data) =>
                {
                    if (data == null)
                    {
                        GameLog.Error($"AOT DLL 加载失败: {platform}_{aotDllName}.dll，请核对 InvariableConst.AotDllNames 手工清单是否漏配");

                        return;
                    }

                    byte[] bytes = ConfigUtils.ReadSafeFile<byte[]>(data.m_bytes);
                    RuntimeApi.LoadMetadataForAOTAssembly(bytes, HomologousImageMode.SuperSet);

                    remaining--;

                    if (remaining > 0)
                    {
                        return;
                    }

                    AsyncLoadAsset<BinAsset>($"{platform}_HotUpdate.dll", (hotUpdateData) =>
                    {
                        if (hotUpdateData == null)
                        {
                            GameLog.Error($"HotUpdate DLL 加载失败: {platform}_HotUpdate.dll");

                            return;
                        }

                        byte[] hotUpdateBytes = ConfigUtils.ReadSafeFile<byte[]>(hotUpdateData.m_bytes);
                        Assembly hotUpdateAss = Assembly.Load(hotUpdateBytes);
                        m_hotUpdateAssembly = hotUpdateAss;
                        callBack?.Invoke(hotUpdateAss);
                    });
                });
            }
        }

        /// <summary>
        /// 异步加载资源（同地址在途去重，完成后通知全部回调）
        /// </summary>
        /// <param name="address">资源地址</param>
        /// <param name="callBack">加载完成回调</param>
        public void AsyncLoadAsset<T>(string address, Action<T> callBack) where T : UnityEngine.Object
        {
            m_assetHandles ??= new Dictionary<string, AssetHandle>();
            m_pendingCallbacks ??= new Dictionary<string, List<Action<object>>>();
            m_lastAccessTimes ??= new Dictionary<string, float>();
            EnsureEvictTimer();

            if (m_assetHandles.TryGetValue(address, out AssetHandle cachedHandle))
            {
                Touch(address);
                callBack?.Invoke((T)cachedHandle.AssetObject);

                return;
            }

            if (m_pendingCallbacks.TryGetValue(address, out List<Action<object>> pending))
            {
                pending.Add((asset) => callBack?.Invoke((T)asset));

                return;
            }

            List<Action<object>> callbacks = new List<Action<object>>
            {
                (asset) => callBack?.Invoke((T)asset)
            };
            m_pendingCallbacks.Add(address, callbacks);

            AssetHandle handle = Package.LoadAssetAsync<T>(address);

            handle.Completed += (operation) =>
            {
                m_pendingCallbacks.Remove(address);

                if (operation.Status == EOperationStatus.Succeed)
                {
                    m_assetHandles[address] = operation;
                    Touch(address);

                    for (int i = 0; i < callbacks.Count; i++)
                    {
                        callbacks[i]?.Invoke(operation.AssetObject);
                    }
                }
                else
                {
                    GameLog.Error($"异步加载资源失败！address:{address}");

                    for (int i = 0; i < callbacks.Count; i++)
                    {
                        callbacks[i]?.Invoke(null);
                    }
                }
            };
        }

        /// <summary>
        /// 异步加载场景，Single 模式先卸载其他场景并在完成后清空 GameObject 池
        /// </summary>
        /// <param name="address">场景地址</param>
        /// <param name="loadSceneMode">场景加载模式</param>
        /// <param name="callBack">加载完成回调</param>
        public void AsyncLoadScene(string address, LoadSceneMode loadSceneMode, Action<Scene> callBack)
        {
            m_sceneHandles ??= new Dictionary<string, SceneHandle>();

            bool isSingle = loadSceneMode == LoadSceneMode.Single;

            Action loadScene = () =>
            {
                if (m_sceneHandles.TryGetValue(address, out SceneHandle cachedHandle))
                {
                    Scene scene = cachedHandle.SceneObject;

                    if (scene.isLoaded)
                    {
                        if (isSingle)
                        {
                            PoolUtils.ClearAllGameObjectPools();
                        }

                        callBack?.Invoke(scene);

                        return;
                    }

                    cachedHandle.Release();
                    m_sceneHandles.Remove(address);
                }

                SceneHandle handle = Package.LoadSceneAsync(address, loadSceneMode);

                handle.Completed += (operation) =>
                {
                    if (operation.Status != EOperationStatus.Succeed)
                    {
                        GameLog.Error($"异步加载场景失败！address:{address}");
                        operation.Release();
                        callBack?.Invoke(default);

                        return;
                    }

                    m_sceneHandles[address] = operation;

                    if (isSingle)
                    {
                        PoolUtils.ClearAllGameObjectPools();
                    }

                    callBack?.Invoke(operation.SceneObject);
                };
            };

            if (!isSingle)
            {
                loadScene();

                return;
            }

            List<string> otherAddresses = new List<string>();

            foreach (KeyValuePair<string, SceneHandle> item in m_sceneHandles)
            {
                if (item.Key != address)
                {
                    otherAddresses.Add(item.Key);
                }
            }

            if (otherAddresses.Count <= 0)
            {
                loadScene();

                return;
            }

            int pendingUnloadCount = otherAddresses.Count;

            for (int i = 0; i < otherAddresses.Count; i++)
            {
                UnLoadScene(otherAddresses[i], () =>
                {
                    pendingUnloadCount--;

                    if (pendingUnloadCount <= 0)
                    {
                        loadScene();
                    }
                });
            }
        }

        /// <summary>
        /// 按地址精细释放已缓存资源
        /// </summary>
        /// <param name="address">资源地址</param>
        public void ReleaseAsset(string address)
        {
            if (m_assetHandles == null || !m_assetHandles.TryGetValue(address, out AssetHandle handle))
            {
                return;
            }

            handle.Release();
            m_assetHandles.Remove(address);
            m_lastAccessTimes?.Remove(address);
            Utils.ClearSpriteCache(address);
            Package.TryUnloadUnusedAsset(address);
        }

        /// <summary>
        /// 卸载全部已缓存资源
        /// </summary>
        public void UnLoadAsset()
        {
            if (m_assetHandles != null && m_assetHandles.Count > 0)
            {
                foreach (KeyValuePair<string, AssetHandle> item in m_assetHandles)
                {
                    item.Value.Release();
                }

                m_assetHandles.Clear();
            }

            m_lastAccessTimes?.Clear();
            Utils.ClearSpriteCache();
        }

        /// <summary>
        /// 卸载未使用资源（需业务在切场景等时机手动调用）
        /// </summary>
        /// <param name="callBack">卸载完成回调</param>
        public void UnloadUnusedAssets(Action callBack = null)
        {
            UnloadUnusedAssetsOperation operation = Package.UnloadUnusedAssetsAsync();

            operation.Completed += (_) =>
            {
                callBack?.Invoke();
            };
        }

        /// <summary>
        /// 卸载场景（仅释放场景句柄，不连带释放全部资源）
        /// </summary>
        /// <param name="address">场景地址</param>
        /// <param name="callBack">卸载完成回调，成功失败均触发</param>
        public void UnLoadScene(string address, Action callBack = null)
        {
            if (m_sceneHandles == null || m_sceneHandles.Count <= 0 || !m_sceneHandles.ContainsKey(address))
            {
                callBack?.Invoke();

                return;
            }

            SceneHandle sceneHandle = m_sceneHandles[address];
            Action finishUnload = () =>
            {
                sceneHandle.Release();
                m_sceneHandles.Remove(address);
                callBack?.Invoke();
            };
            AsyncOperation handle1 = SceneManager.UnloadSceneAsync(sceneHandle.SceneObject);

            if (handle1 == null)
            {
                UnloadSceneOperation handle2 = sceneHandle.UnloadAsync();

                handle2.Completed += (_) =>
                {
                    finishUnload();
                };

                return;
            }

            handle1.completed += (operation) =>
            {
                if (!operation.isDone)
                {
                    finishUnload();

                    return;
                }

                UnloadSceneOperation handle2 = sceneHandle.UnloadAsync();

                handle2.Completed += (_) =>
                {
                    finishUnload();
                };
            };
        }

        /// <summary>
        /// 刷新资源最近访问时间
        /// </summary>
        private void Touch(string address)
        {
            m_lastAccessTimes ??= new Dictionary<string, float>();
            m_lastAccessTimes[address] = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// 注册闲置句柄清扫计时器
        /// </summary>
        private void EnsureEvictTimer()
        {
            if (m_isEvictTimerStarted || !GameManager.HasInstance)
            {
                return;
            }

            m_isEvictTimerStarted = true;
            GameManager.Instance.RepeatingCallSeconds(InvariableConst.Timer_YooAsset_TickEvict, TickEvict, EvictScanIntervalSeconds, false);
        }

        /// <summary>
        /// 逐出闲置超过阈值且不在白名单内的资源句柄
        /// </summary>
        private void TickEvict()
        {
            if (m_assetHandles == null || m_assetHandles.Count <= 0 || m_lastAccessTimes == null)
            {
                return;
            }

            float now = Time.realtimeSinceStartup;
            List<string> expireAddresses = new List<string>();

            foreach (KeyValuePair<string, AssetHandle> item in m_assetHandles)
            {
                string address = item.Key;

                if (IsEvictExempt(address))
                {
                    continue;
                }

                if (!m_lastAccessTimes.TryGetValue(address, out float lastAccessTime))
                {
                    continue;
                }

                if ((now - lastAccessTime) >= EvictIdleSeconds)
                {
                    expireAddresses.Add(address);
                }
            }

            for (int i = 0; i < expireAddresses.Count; i++)
            {
                ReleaseAsset(expireAddresses[i]);
            }
        }

        /// <summary>
        /// 判断地址是否免于闲置释放
        /// </summary>
        private static bool IsEvictExempt(string address)
        {
            return address.StartsWith("Audios_", StringComparison.Ordinal)
                || address.StartsWith("Config_", StringComparison.Ordinal)
                || address.StartsWith("MiniGame_", StringComparison.Ordinal);
        }
    }
}