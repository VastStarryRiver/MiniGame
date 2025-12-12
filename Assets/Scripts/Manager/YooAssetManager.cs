using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using YooAsset;
using UnityEngine.Networking;
using System.Text;
using System.IO;



public class YooAssetManager : Singleton<YooAssetManager>
{
    private ResourcePackage m_package;
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
            return "MyPackage";
        }
    }

    private Dictionary<string, AssetHandle> m_assetHandles;
    private Dictionary<string, SceneHandle> m_sceneHandles;



    public void SetWebInfo()
    {
        BinAsset data = Resources.Load<BinAsset>("LocalAssets/WebData");
        string[] webData = ConfigUtils.ReadSafeFile<string>(data.bytes).Split('\n');
        ConfigUtils.SetWebData(webData);
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <param name="address"></param>
    /// <param name="callBack"></param>
    public void AsyncLoadAsset<T>(string address, Action<T> callBack) where T : UnityEngine.Object
    {
        m_assetHandles ??= new Dictionary<string, AssetHandle>();

        if (m_assetHandles.ContainsKey(address))
        {
            callBack((T)m_assetHandles[address].AssetObject);
        }
        else
        {
            AssetHandle handle = Package.LoadAssetAsync<T>(address);

            handle.Completed += (operation) => {
                if (operation.Status == EOperationStatus.Succeed)
                {
                    m_assetHandles[address] = operation;
                    callBack((T)operation.AssetObject);
                }
                else
                {
                    Debug.LogError($"异步加载资源失败！address:{address}");
                }
            };
        }
    }

    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="address"></param>
    /// <param name="loadSceneMode"></param>
    /// <param name="callBack"></param>
    public void AsyncLoadScene(string address, LoadSceneMode loadSceneMode, Action<Scene> callBack)
    {
        m_sceneHandles ??= new Dictionary<string, SceneHandle>();

        if (m_sceneHandles.ContainsKey(address))
        {
            SceneHandle handle = m_sceneHandles[address];
            callBack(handle.SceneObject);
        }
        else
        {
            SceneHandle handle = Package.LoadSceneAsync(address, loadSceneMode);

            handle.Completed += (operation) => {
                if (operation.Status == EOperationStatus.Succeed)
                {
                    m_sceneHandles[address] = operation;
                    callBack(operation.SceneObject);
                }
                else
                {
                    Debug.LogError($"异步加载场景失败！address:{address}");
                }
            };
        }
    }

    /// <summary>
    /// 卸载资源
    /// </summary>
    public void UnLoadAsset()
    {
        if (m_assetHandles != null && m_assetHandles.Count > 0)
        {
            foreach (var item in m_assetHandles)
            {
                item.Value.Release();
            }

            m_assetHandles.Clear();
        }
    }

    /// <summary>
    /// 卸载场景
    /// </summary>
    public void UnLoadScene(string address)
    {
        UnLoadAsset();

        if (m_sceneHandles == null || m_sceneHandles.Count <= 0 || !m_sceneHandles.ContainsKey(address))
        {
            return;
        }

        var handle1 = SceneManager.UnloadSceneAsync(m_sceneHandles[address].SceneObject);

        handle1.completed += (operation) =>
        {
            if (!operation.isDone)
            {
                return;
            }

            var handle2 = m_sceneHandles[address].UnloadAsync();

            handle2.Completed += (operation) =>
            {
                if (operation.Status != EOperationStatus.Succeed)
                {
                    return;
                }

                m_sceneHandles[address].Release();
                m_sceneHandles.Remove(address);
            };
        };
    }
}