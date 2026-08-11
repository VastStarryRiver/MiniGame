using CloudService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.UOS.Auth;
using Unity.UOS.CloudSave;
using Unity.UOS.CloudSave.Model.Files;



namespace Invariable
{
    public class CloudManager : Singleton<CloudManager>
    {
        private const string CloudSaveGameId = "MiniGame"; // 每个游戏项目必须改为唯一值，并与 CloudHelper 内配置的 GameId 保持一致
        private const int CloudGetAllMaxCount = 100;
        private const float UploadDebounceSeconds = 2f;
        private static readonly string CloudSaveNamespace = $"minigame_kv_{CloudSaveGameId}";
        private Dictionary<string, string> m_cloudDataCache = null;
        private SemaphoreSlim m_cloudUploadLock = null;
        private static CloudHelper m_cloudHelper = null;
        private bool m_cloudDataDirty = false;

        /// <summary>
        /// 上传任务排队串行执行，同一时间只会执行一个上传任务
        /// </summary>
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
        public static CloudHelper CloudHelper
        {
            get
            {
                m_cloudHelper ??= new CloudHelper();

                return m_cloudHelper;
            }
        }



        /// <summary>
        /// 初始化云存档数据
        /// </summary>
        public void InitCloudData(Action callBack = null)
        {
#if UNITY_EDITOR
            callBack?.Invoke();
#else
            _ = InitCloudDataAsync(callBack);
#endif
        }

        /// <summary>
        /// 异步初始化云存档并登录平台
        /// </summary>
        private async Task InitCloudDataAsync(Action callBack)
        {
            try
            {
                await CloudSaveSDK.InitializeAsync();

                string code = await SdkManager.Instance.PlatformLogin();

                if (string.IsNullOrEmpty(code))
                {
                    GameLog.Error("平台登录未获取到 code");

                    return;
                }

                PlatformLoginResult loginResult = null;

#if MINIGAME_SUBPLATFORM_WEIXIN
                loginResult = await CloudHelper.WechatLogin(CloudSaveGameId, code);

#elif MINIGAME_SUBPLATFORM_DOUYIN
                loginResult = await CloudHelper.DouyinLogin(CloudSaveGameId, code);
#endif

                if (loginResult == null || string.IsNullOrEmpty(loginResult.personaAccessToken))
                {
                    GameLog.Error("云函数登录失败");

                    return;
                }

                AuthTokenManager.SaveToken(new TokenInfo
                {
                    AccessToken = loginResult.personaAccessToken,
                    RefreshToken = loginResult.personaRefreshToken,
                    UserId = loginResult.userID,
                    PersonaId = loginResult.personaID
                });

                SaveItem saveItem = await CloudSaveSDK.Instance.Files.GetLinearAsync(CloudSaveNamespace);

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
            catch (Exception error)
            {
                GameLog.Error($"初始化云数据失败: {error}");
                m_cloudDataCache = null;
            }
            finally
            {
                callBack?.Invoke();
            }
        }

        /// <summary>
        /// 按排名字段获取前 N 名玩家云数据
        /// </summary>
        public void GetAllCloudData(string rankKey, Action<List<PlayerCloudData>> callBack)
        {
#if UNITY_EDITOR
            callBack?.Invoke(new List<PlayerCloudData>());
#else
            _ = GetAllCloudDataAsync(rankKey, callBack);
#endif
        }

        /// <summary>
        /// 异步按排名字段拉取前 N 名玩家云数据
        /// </summary>
        private async Task GetAllCloudDataAsync(string rankKey, Action<List<PlayerCloudData>> callBack)
        {
            var result = new List<PlayerCloudData>();

            try
            {
                if (m_cloudDataCache == null)
                {
                    return;
                }

                List<PlayerSaveData> saveList = await CloudHelper.GetAllCloudData(CloudSaveGameId, rankKey, CloudGetAllMaxCount);

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
            catch (Exception error)
            {
                GameLog.Error($"获取全部云数据失败: {error}");
                result = new List<PlayerCloudData>();
            }
            finally
            {
                callBack?.Invoke(result);
            }
        }

        /// <summary>
        /// 上传本地云缓存到远程存档
        /// </summary>
        private async Task UploadCloudData()
        {
            await CloudUploadLock.WaitAsync();

            try
            {
                string json = JsonConvert.SerializeObject(m_cloudDataCache);
                byte[] fileBytes = Encoding.UTF8.GetBytes(json);
                await CloudSaveSDK.Instance.Files.SaveLinearAsync(CloudSaveNamespace, new UpdateOptions
                {
                    Name = "minigame_kv_save",
                    File = new FileOptions
                    {
                        UpdateFileWay = UpdateFileWay.ByFileBytes,
                        FileBytes = fileBytes
                    }
                });
            }
            catch (Exception error)
            {
                GameLog.Error($"上传云数据失败: {error}");
            }
            finally
            {
                CloudUploadLock.Release();
            }
        }

        /// <summary>
        /// 写入云缓存并触发防抖上传
        /// </summary>
        internal void SetCloudCache(string key, string data)
        {
            if (m_cloudDataCache == null)
            {
                return;
            }

            m_cloudDataCache[key] = data;
            m_cloudDataDirty = true;
            ScheduleDebouncedUpload();
        }

        /// <summary>
        /// 读取云缓存值
        /// </summary>
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

        /// <summary>
        /// 立即上传脏数据（取消进行中的防抖）
        /// </summary>
        public void FlushCloudData()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.CancelInvokeByKey(InvariableConst.Timer_CloudManager_UploadDebounce);
            }

            _ = FlushCloudDataAsync();
        }

        /// <summary>
        /// 安排防抖上传
        /// </summary>
        private void ScheduleDebouncedUpload()
        {
            if (!GameManager.HasInstance)
            {
                _ = FlushCloudDataAsync();

                return;
            }

            GameManager.Instance.CancelInvokeByKey(InvariableConst.Timer_CloudManager_UploadDebounce);
            GameManager.Instance.DelayCallSeconds(InvariableConst.Timer_CloudManager_UploadDebounce, () =>
            {
                _ = FlushCloudDataAsync();
            }, UploadDebounceSeconds);
        }

        /// <summary>
        /// 异步上传脏数据；上传期间再次写入会重新标记 dirty
        /// </summary>
        private async Task FlushCloudDataAsync()
        {
            if (!m_cloudDataDirty || m_cloudDataCache == null)
            {
                return;
            }

            m_cloudDataDirty = false;
            await UploadCloudData();

            if (m_cloudDataDirty)
            {
                ScheduleDebouncedUpload();
            }
        }
    }
}