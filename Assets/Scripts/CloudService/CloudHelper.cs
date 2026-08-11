using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.UOS.Func.Stateless.Core.Attributes;
using UnityEngine;



namespace CloudService
{
    // 注意事项
    //
    // 1. 云函数类所在脚本文件的名称必须与类名相同
    //
    // 2. 所有的类必须放置于命名空间内，且所用到的代码文件必须放到同一目录中
    //
    // 3. 使用 [CloudService] 标记有远程调用函数的类，使用 [CloudFunc] 标记需要远程调用的函数
    //
    // 4. 请在云函数类构造函数中初始化云函数，不要创建并调用其他带有参数的构造函数
    //
    // 5. 切换到远程模式后云函数类只会保留带有 [CloudFunc] 的方法，其他字段将会被隐藏
    //
    // 6. 使用 [CloudFunc] 标记的函数必须符合 public async Task<返回数据类型> 函数名称(输出参数) { 函数体 } 这样的格式
    //
    // 7. 编写代码中只能使用 UnityEngine 命名空间下的 Debug.Log，Debug.LogWarning，Debug.LogError 函数，不能使用其他函数
    //
    // 8. 打包之前先切换远程调用模式
    //

    [CloudService]
    public class CloudHelper
    {
        // GameId 必须与客户端 CloudManager.CloudSaveGameId 一致，并填入微信/抖音 AppID/AppSecret
        private static readonly GameSecrets Secrets = new GameSecrets
        {
            GameId = "MiniGame",
            WechatAppId = "",
            WechatAppSecret = "",
            DouyinAppId = "",
            DouyinAppSecret = ""
        };



        public CloudHelper()
        {
        }



        /// <summary>
        /// 微信小游戏登录，用 code 换取 openid 并完成外部登录。
        /// </summary>
        [CloudFunc]
        public async Task<PlatformLoginResult> WechatLogin(string gameId, string code)
        {
            if (!TryGetGameSecrets(gameId, out GameSecrets secrets))
            {
                return null;
            }

            if (string.IsNullOrEmpty(secrets.WechatAppId) || string.IsNullOrEmpty(secrets.WechatAppSecret))
            {
                Debug.LogError($"WechatLogin: 游戏 {gameId} 未配置微信 AppID/AppSecret");

                return null;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"https://api.weixin.qq.com/sns/jscode2session?appid={secrets.WechatAppId}&secret={secrets.WechatAppSecret}&js_code={Uri.EscapeDataString(code)}&grant_type=authorization_code";
                    HttpResponseMessage response = await client.GetAsync(url);
                    string body = await response.Content.ReadAsStringAsync();
                    WeixinLoginResult result = JsonConvert.DeserializeObject<WeixinLoginResult>(body);

                    if (result == null || result.errcode != 0 || string.IsNullOrEmpty(result.openid))
                    {
                        Debug.LogError($"WechatLogin: jscode2session 失败 errcode={result?.errcode}, errmsg={result?.errmsg}");

                        return null;
                    }

                    return await ExternalLogin(client, "wx-" + result.openid);
                }
            }
            catch (Exception error)
            {
                Debug.LogError($"WechatLogin 异常: {error.Message}");

                return null;
            }
        }

        /// <summary>
        /// 抖音小游戏登录，用 code 换取 openid 并完成外部登录。
        /// </summary>
        [CloudFunc]
        public async Task<PlatformLoginResult> DouyinLogin(string gameId, string code)
        {
            if (!TryGetGameSecrets(gameId, out GameSecrets secrets))
            {
                return null;
            }

            if (string.IsNullOrEmpty(secrets.DouyinAppId) || string.IsNullOrEmpty(secrets.DouyinAppSecret))
            {
                Debug.LogError($"DouyinLogin: 游戏 {gameId} 未配置抖音 AppID/AppSecret");

                return null;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"https://minigame.zijieapi.com/mgplatform/api/apps/jscode2session?appid={secrets.DouyinAppId}&secret={secrets.DouyinAppSecret}&code={Uri.EscapeDataString(code)}";
                    HttpResponseMessage response = await client.GetAsync(url);
                    string body = await response.Content.ReadAsStringAsync();
                    DouyinLoginResult result = JsonConvert.DeserializeObject<DouyinLoginResult>(body);

                    if (result == null || result.error != 0 || string.IsNullOrEmpty(result.openid))
                    {
                        string errText = !string.IsNullOrEmpty(result?.errmsg) ? result.errmsg : result?.message;
                        Debug.LogError($"DouyinLogin: jscode2session 失败 error={result?.error}, msg={errText}");

                        return null;
                    }

                    return await ExternalLogin(client, "dy-" + result.openid);
                }
            }
            catch (Exception error)
            {
                Debug.LogError($"DouyinLogin 异常: {error.Message}");

                return null;
            }
        }

        /// <summary>
        /// 按游戏标识拉取云存档，依 rankKey 数值降序取前 maxCount 名
        /// </summary>
        [CloudFunc]
        public async Task<List<PlayerSaveData>> GetAllCloudData(string gameId, string rankKey, int maxCount)
        {
            // 禁止客户端直接指定命名空间，避免共享 UOS App 时跨游戏拉取
            if (!TryGetGameSecrets(gameId, out _))
            {
                return null;
            }

            if (string.IsNullOrEmpty(rankKey))
            {
                Debug.LogError("GetAllCloudData: rankKey 不能为空");

                return null;
            }

            string namespaces = $"minigame_kv_{gameId}";

            if (maxCount <= 0)
            {
                return new List<PlayerSaveData>();
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    ApplyBasicAuth(client);

                    List<SaveInfo> saveInfos = new List<SaveInfo>();
                    int start = 0;
                    const int PageSize = 50;
                    bool hasMore = true;

                    while (hasMore)
                    {
                        string listUrl = $"https://save.unity.cn/v1/saves?namespaces={Uri.EscapeDataString(namespaces)}&start={start}&count={PageSize}&skipTotal=true";
                        HttpResponseMessage listResponse = await client.GetAsync(listUrl);

                        if (!listResponse.IsSuccessStatusCode)
                        {
                            string errBody = await listResponse.Content.ReadAsStringAsync();
                            Debug.LogError($"GetAllCloudData: 列表请求失败 status={(int)listResponse.StatusCode}, body={errBody}");

                            return null;
                        }

                        string listBody = await listResponse.Content.ReadAsStringAsync();
                        ListSavesResponse listResult = JsonConvert.DeserializeObject<ListSavesResponse>(listBody);

                        if (listResult?.saves == null || listResult.saves.Count == 0)
                        {
                            break;
                        }

                        saveInfos.AddRange(listResult.saves);
                        start += listResult.saves.Count;
                        hasMore = listResult.saves.Count >= PageSize;
                    }

                    const int FetchConcurrency = 10;
                    List<Task<PlayerSaveData>> fetchTasks = new List<Task<PlayerSaveData>>();

                    using (SemaphoreSlim semaphore = new SemaphoreSlim(FetchConcurrency, FetchConcurrency))
                    {
                        for (int i = 0; i < saveInfos.Count; i++)
                        {
                            SaveInfo info = saveInfos[i];

                            if (info == null || string.IsNullOrEmpty(info.saveId))
                            {
                                continue;
                            }

                            fetchTasks.Add(FetchSaveDataAsync(client, info, semaphore));
                        }

                        PlayerSaveData[] fetched = await Task.WhenAll(fetchTasks);
                        List<PlayerSaveData> results = new List<PlayerSaveData>();

                        for (int i = 0; i < fetched.Length; i++)
                        {
                            if (fetched[i] != null)
                            {
                                results.Add(fetched[i]);
                            }
                        }

                        results.Sort((left, right) =>
                        {
                            double leftScore = GetRankScore(left.data, rankKey);
                            double rightScore = GetRankScore(right.data, rankKey);

                            return rightScore.CompareTo(leftScore);
                        });

                        if (results.Count > maxCount)
                        {
                            results.RemoveRange(maxCount, results.Count - maxCount);
                        }

                        return results;
                    }
                }
            }
            catch (Exception error)
            {
                Debug.LogError($"GetAllCloudData 异常: {error.Message}");

                return null;
            }
        }



        /// <summary>
        /// 并发拉取单条存档详情与内容
        /// </summary>
        private async Task<PlayerSaveData> FetchSaveDataAsync(HttpClient client, SaveInfo info, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();

            try
            {
                string getUrl = $"https://save.unity.cn/v1/saves/{Uri.EscapeDataString(info.saveId)}?includeDownloadURL=true";
                HttpResponseMessage getResponse = await client.GetAsync(getUrl);

                if (!getResponse.IsSuccessStatusCode)
                {
                    Debug.LogError($"GetAllCloudData: 获取存档失败 saveId={info.saveId}, status={(int)getResponse.StatusCode}");

                    return null;
                }

                string getBody = await getResponse.Content.ReadAsStringAsync();
                GetSaveResponse getResult = JsonConvert.DeserializeObject<GetSaveResponse>(getBody);
                string fileUrl = getResult?.save?.file?.fileURL;

                if (string.IsNullOrEmpty(fileUrl))
                {
                    return null;
                }

                HttpResponseMessage fileResponse = await client.GetAsync(fileUrl);

                if (!fileResponse.IsSuccessStatusCode)
                {
                    Debug.LogError($"GetAllCloudData: 下载存档内容失败 saveId={info.saveId}, status={(int)fileResponse.StatusCode}");

                    return null;
                }

                string fileText = await fileResponse.Content.ReadAsStringAsync();
                Dictionary<string, string> data;

                if (string.IsNullOrEmpty(fileText))
                {
                    data = new Dictionary<string, string>();
                }
                else
                {
                    data = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileText) ?? new Dictionary<string, string>();
                }

                return new PlayerSaveData
                {
                    userId = !string.IsNullOrEmpty(getResult.save.userId) ? getResult.save.userId : info.userId,
                    data = data
                };
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// 解析排名字段数值，缺失或无法解析时返回负无穷以便降序排到末尾
        /// </summary>
        private static double GetRankScore(Dictionary<string, string> data, string rankKey)
        {
            if (data == null || !data.TryGetValue(rankKey, out string raw) || string.IsNullOrEmpty(raw))
            {
                return double.NegativeInfinity;
            }

            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double score))
            {
                return score;
            }

            return double.NegativeInfinity;
        }

        /// <summary>
        /// 校验游戏标识并获取对应密钥配置。
        /// </summary>
        private bool TryGetGameSecrets(string gameId, out GameSecrets secrets)
        {
            if (string.IsNullOrEmpty(gameId) || gameId != Secrets.GameId)
            {
                Debug.LogError($"游戏标识不匹配: gameId={gameId}, expected={Secrets.GameId}");
                secrets = null;

                return false;
            }

            secrets = Secrets;

            return true;
        }

        /// <summary>
        /// 使用外部用户 ID 调用 UOS 外部登录接口。
        /// </summary>
        private async Task<PlatformLoginResult> ExternalLogin(HttpClient client, string externalUserID)
        {
            ApplyBasicAuth(client);

            ExternalLoginRequest request = new ExternalLoginRequest { externalUserID = externalUserID };
            string requestStr = JsonConvert.SerializeObject(request);
            HttpContent content = new StringContent(requestStr, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("https://p.unity.cn/v1/login/external", content);

            if (!response.IsSuccessStatusCode)
            {
                string errBody = await response.Content.ReadAsStringAsync();
                Debug.LogError($"ExternalLogin 失败 status={(int)response.StatusCode}, body={errBody}");

                return null;
            }

            string body = await response.Content.ReadAsStringAsync();
            ExternalLoginResponse loginResult = JsonConvert.DeserializeObject<ExternalLoginResponse>(body);

            if (loginResult == null || string.IsNullOrEmpty(loginResult.personaAccessToken) || loginResult.persona == null)
            {
                Debug.LogError("ExternalLogin 返回数据无效");

                return null;
            }

            return new PlatformLoginResult
            {
                personaAccessToken = loginResult.personaAccessToken,
                personaRefreshToken = loginResult.personaRefreshToken,
                userID = loginResult.persona.userID,
                personaID = loginResult.persona.personaID
            };
        }

        /// <summary>
        /// 为 HttpClient 设置 UOS Basic 认证头。
        /// </summary>
        private void ApplyBasicAuth(HttpClient client)
        {
            string appId = Environment.GetEnvironmentVariable("UOS_APP_ID");
            string appServiceSecret = Environment.GetEnvironmentVariable("UOS_APP_SERVICE_SECRET");
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{appId}:{appServiceSecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }
    }
}