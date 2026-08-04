using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.UOS.Auth;
using Unity.UOS.CloudSave;
using Unity.UOS.CloudSave.Model.Files;
using FileOptions = Unity.UOS.CloudSave.Model.Files.FileOptions;
using Newtonsoft.Json;
using CloudService;
using UnityEngine;



namespace Invariable
{
    public class CloudManager : Singleton<CloudManager>
    {
        private const string CLOUD_SAVE_GAME_ID = "MiniGame"; // 每个游戏项目必须改为唯一值，并与 CloudHelper.Secrets.GameId 保持一致
        private static readonly string CLOUD_SAVE_NAMESPACE = $"minigame_kv_{CLOUD_SAVE_GAME_ID}";
        private const int CLOUD_GET_ALL_MAX_COUNT = 200;
        private Dictionary<string, string> m_cloudDataCache = null;

        /// <summary>
        /// 上传任务排队串行执行，同一时间只会执行一个上传任务
        /// </summary>
        private SemaphoreSlim m_cloudUploadLock = null;
        private SemaphoreSlim CloudUploadLock
        {
            get
            {
                m_cloudUploadLock ??= new SemaphoreSlim(1, 1);
                return m_cloudUploadLock;
            }
        }

        /// <summary>
        /// UOS云函数类
        /// </summary>
        private static CloudHelper m_cloudHelper = null;
        public static CloudHelper CloudHelper
        {
            get
            {
                m_cloudHelper ??= new CloudHelper();
                return m_cloudHelper;
            }
        }



        public void InitCloudData(Action callBack = null)
        {
#if UNITY_EDITOR
            callBack?.Invoke();
#else
            _ = InitCloudDataAsync(callBack);
#endif
        }

        private async Task InitCloudDataAsync(Action callBack)
        {
            try
            {
                await CloudSaveSDK.InitializeAsync();

                string code = await SdkManager.Instance.PlatformLogin();
                if (string.IsNullOrEmpty(code))
                {
                    Debug.LogError("平台登录未获取到 code");
                }

                PlatformLoginResult loginResult = null;

#if MINIGAME_SUBPLATFORM_WEIXIN
                loginResult = await CloudHelper.WechatLogin(CLOUD_SAVE_GAME_ID, code);

#elif MINIGAME_SUBPLATFORM_DOUYIN
                loginResult = await CloudHelper.DouyinLogin(CLOUD_SAVE_GAME_ID, code);
#endif

                if (loginResult == null || string.IsNullOrEmpty(loginResult.personaAccessToken))
                {
                    Debug.LogError("云函数登录失败");
                }

                AuthTokenManager.SaveToken(new TokenInfo
                {
                    AccessToken = loginResult.personaAccessToken,
                    RefreshToken = loginResult.personaRefreshToken,
                    UserId = loginResult.userID,
                    PersonaId = loginResult.personaID
                });

                SaveItem saveItem = await CloudSaveSDK.Instance.Files.GetLinearAsync(CLOUD_SAVE_NAMESPACE);
                if (saveItem != null && !string.IsNullOrEmpty(saveItem.SaveId))
                {
                    byte[] fileBytes = await CloudSaveSDK.Instance.Files.LoadBytesAsync(saveItem.SaveId);
                    string json = Encoding.UTF8.GetString(fileBytes);

                    if (string.IsNullOrEmpty(json))
                    {
                        m_cloudDataCache = new Dictionary<string, string>();
                    }
                    else
                    {
                        m_cloudDataCache = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                    }
                }
                else
                {
                    m_cloudDataCache = new Dictionary<string, string>();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"初始化云数据失败: {e}");
                m_cloudDataCache = null;
            }
            finally
            {
                callBack?.Invoke();
            }
        }

        public void GetAllCloudData(Action<List<PlayerCloudData>> callBack)
        {
#if UNITY_EDITOR
            callBack?.Invoke(new List<PlayerCloudData>());
#else
            _ = GetAllCloudDataAsync(callBack);
#endif
        }

        private async Task GetAllCloudDataAsync(Action<List<PlayerCloudData>> callBack)
        {
            var result = new List<PlayerCloudData>();

            try
            {
                if (m_cloudDataCache == null)
                {
                    return;
                }

                List<PlayerSaveData> saveList = await CloudHelper.GetAllCloudData(CLOUD_SAVE_GAME_ID, CLOUD_GET_ALL_MAX_COUNT);
                if (saveList != null)
                {
                    for (int i = 0; i < saveList.Count; i++)
                    {
                        PlayerSaveData save = saveList[i];
                        if (save == null)
                        {
                            continue;
                        }

                        result.Add(new PlayerCloudData
                        {
                            UserId = save.userId,
                            Data = save.data ?? new Dictionary<string, string>()
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"获取全部云数据失败: {e}");
                result = new List<PlayerCloudData>();
            }
            finally
            {
                callBack?.Invoke(result);
            }
        }

        private async Task UploadCloudData()
        {
            await CloudUploadLock.WaitAsync();

            try
            {
                string json = JsonConvert.SerializeObject(m_cloudDataCache);
                byte[] fileBytes = Encoding.UTF8.GetBytes(json);
                await CloudSaveSDK.Instance.Files.SaveLinearAsync(CLOUD_SAVE_NAMESPACE, new UpdateOptions
                {
                    Name = "minigame_kv_save",
                    File = new FileOptions
                    {
                        UpdateFileWay = UpdateFileWay.ByFileBytes,
                        FileBytes = fileBytes
                    }
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"上传云数据失败: {e}");
            }
            finally
            {
                CloudUploadLock.Release();
            }
        }

        internal void SetCloudCache(string key, string data)
        {
            if (m_cloudDataCache == null)
            {
                return;
            }

            m_cloudDataCache[key] = data;
            _ = UploadCloudData();
        }

        internal string GetCloudCache(string key, string defaultValue = "")
        {
            if (m_cloudDataCache == null)
            {
                return defaultValue;
            }

            if (m_cloudDataCache.TryGetValue(key, out string data) && !string.IsNullOrEmpty(data))
            {
                return data;
            }

            return defaultValue;
        }
    }
}