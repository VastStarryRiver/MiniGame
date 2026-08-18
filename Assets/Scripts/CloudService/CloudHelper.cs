using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
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
        private class GenerateTokenResponse
        {
            public string accessToken;
            public long expiresAt;
        }

        private class RankSnapshot
        {
            public string saveId;
            public List<PlayerSaveData> entries;
        }

        private const string RankSnapshotUserId = "sys";
        private const int LeaderboardCapacity = 100;

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
        /// 微信小游戏登录，用 code 换取 openid 并换取云存档令牌
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

                    return await GenerateToken(client, "wx-" + result.openid);
                }
            }
            catch (Exception error)
            {
                Debug.LogError($"WechatLogin 异常: {error.Message}");

                return null;
            }
        }

        /// <summary>
        /// 抖音小游戏登录，用 code 换取 openid 并换取云存档令牌
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

                    return await GenerateToken(client, "dy-" + result.openid);
                }
            }
            catch (Exception error)
            {
                Debug.LogError($"DouyinLogin 异常: {error.Message}");

                return null;
            }
        }

        /// <summary>
        /// 读取排行榜快照，依 rankKey 数值降序取前 maxCount 名
        /// </summary>
        [CloudFunc]
        public async Task<List<PlayerSaveData>> GetAllCloudData(string gameId, string rankKey, int maxCount, string platform)
        {
            // 禁止客户端直接指定命名空间，避免共享 UOS App 时跨游戏拉取
            if (!TryGetGameSecrets(gameId, out _))
            {
                return null;
            }

            if (!IsValidPlatform(platform))
            {
                Debug.LogError("GetAllCloudData: platform 无效");

                return new List<PlayerSaveData>();
            }

            if (string.IsNullOrEmpty(rankKey))
            {
                Debug.LogError("GetAllCloudData: rankKey 不能为空");

                return null;
            }

            if (maxCount <= 0)
            {
                return new List<PlayerSaveData>();
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    ApplyBasicAuth(client);
                    RankSnapshot snapshot = await LoadRankSnapshot(client, gameId, platform);
                    List<PlayerSaveData> results = snapshot.entries;

                    if (results.Count > maxCount)
                    {
                        results.RemoveRange(maxCount, results.Count - maxCount);
                    }

                    return results;
                }
            }
            catch (Exception error)
            {
                Debug.LogError($"GetAllCloudData 异常: {error.Message}");

                return null;
            }
        }

        /// <summary>
        /// 上报排行分数，增量维护 Top100 快照，返回本次是否上榜或更新
        /// </summary>
        [CloudFunc]
        public async Task<bool> ReportRankScore(string gameId, string userId, string rankKey, double score, string platform, string nickName, string avatarUrl)
        {
            if (!TryGetGameSecrets(gameId, out _))
            {
                return false;
            }

            if (!IsValidPlatform(platform))
            {
                Debug.LogError("ReportRankScore: platform 无效");

                return false;
            }

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(rankKey) || double.IsNaN(score) || double.IsInfinity(score))
            {
                Debug.LogError("ReportRankScore: userId、rankKey 或 score 无效");

                return false;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    ApplyBasicAuth(client);
                    RankSnapshot snapshot = await LoadRankSnapshot(client, gameId, platform);
                    List<PlayerSaveData> entries = snapshot.entries;
                    SortRankEntries(entries, rankKey);
                    int existingIndex = FindRankEntryIndex(entries, userId);

                    if (existingIndex >= 0)
                    {
                        if (entries[existingIndex].data == null)
                        {
                            entries[existingIndex].data = new Dictionary<string, string>();
                        }

                        entries[existingIndex].data[rankKey] = score.ToString(CultureInfo.InvariantCulture);
                        ApplyProfileData(entries[existingIndex].data, nickName, avatarUrl);
                    }
                    else
                    {
                        if (entries.Count >= LeaderboardCapacity)
                        {
                            double lastScore = GetRankScore(entries[entries.Count - 1].data, rankKey);

                            if (score <= lastScore)
                            {
                                return false;
                            }

                            entries.RemoveAt(entries.Count - 1);
                        }
                        else if (score <= 0)
                        {
                            return false; // 榜不满 100 时，rankKey 数据需大于 0 才上榜
                        }

                        Dictionary<string, string> data = new Dictionary<string, string>
                        {
                            { rankKey, score.ToString(CultureInfo.InvariantCulture) }
                        };
                        ApplyProfileData(data, nickName, avatarUrl);
                        entries.Add(new PlayerSaveData
                        {
                            userId = userId,
                            data = data
                        });
                    }

                    SortRankEntries(entries, rankKey);

                    if (entries.Count > LeaderboardCapacity)
                    {
                        entries.RemoveRange(LeaderboardCapacity, entries.Count - LeaderboardCapacity);
                    }

                    await SaveRankSnapshot(client, gameId, platform, snapshot.saveId, entries);

                    return true;
                }
            }
            catch (Exception error)
            {
                Debug.LogError($"ReportRankScore 异常: {error.Message}");

                throw;
            }
        }



        /// <summary>
        /// 读取排行榜快照，不存在时返回空列表
        /// </summary>
        private async Task<RankSnapshot> LoadRankSnapshot(HttpClient client, string gameId, string platform)
        {
            RankSnapshot snapshot = new RankSnapshot
            {
                saveId = null,
                entries = new List<PlayerSaveData>()
            };
            string namespaces = GetRankNamespace(gameId, platform);
            string listUrl = $"https://save.unity.cn/v1/saves?namespaces={Uri.EscapeDataString(namespaces)}&userId={Uri.EscapeDataString(RankSnapshotUserId)}&start=0&count=1&skipTotal=true";
            HttpResponseMessage listResponse = await client.GetAsync(listUrl);

            if (!listResponse.IsSuccessStatusCode)
            {
                string errBody = await listResponse.Content.ReadAsStringAsync();
                Debug.LogError($"LoadRankSnapshot: 列表请求失败 status={(int)listResponse.StatusCode}, body={errBody}");
                listResponse.EnsureSuccessStatusCode();
            }

            string listBody = await listResponse.Content.ReadAsStringAsync();
            ListSavesResponse listResult = JsonConvert.DeserializeObject<ListSavesResponse>(listBody);
            SaveInfo info = listResult?.saves != null && listResult.saves.Count > 0 ? listResult.saves[0] : null;

            if (info == null || string.IsNullOrEmpty(info.saveId))
            {
                return snapshot;
            }

            snapshot.saveId = info.saveId;
            string getUrl = $"https://save.unity.cn/v1/saves/{Uri.EscapeDataString(info.saveId)}?includeDownloadURL=true";
            HttpResponseMessage getResponse = await client.GetAsync(getUrl);

            if (!getResponse.IsSuccessStatusCode)
            {
                Debug.LogError($"LoadRankSnapshot: 获取存档失败 saveId={info.saveId}, status={(int)getResponse.StatusCode}");
                getResponse.EnsureSuccessStatusCode();
            }

            string getBody = await getResponse.Content.ReadAsStringAsync();
            GetSaveResponse getResult = JsonConvert.DeserializeObject<GetSaveResponse>(getBody);
            string fileUrl = getResult?.save?.file?.fileURL;

            if (string.IsNullOrEmpty(fileUrl))
            {
                FailRankWrite($"LoadRankSnapshot: 存档缺少下载地址 saveId={info.saveId}");

                return snapshot;
            }

            HttpResponseMessage fileResponse = await client.GetAsync(fileUrl);

            if (!fileResponse.IsSuccessStatusCode)
            {
                Debug.LogError($"LoadRankSnapshot: 下载存档内容失败 saveId={info.saveId}, status={(int)fileResponse.StatusCode}");
                fileResponse.EnsureSuccessStatusCode();
            }

            string fileText = await fileResponse.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(fileText))
            {
                return snapshot;
            }

            snapshot.entries = JsonConvert.DeserializeObject<List<PlayerSaveData>>(fileText) ?? new List<PlayerSaveData>();

            return snapshot;
        }

        /// <summary>
        /// 将排行榜快照写回独立 namespace
        /// </summary>
        private async Task SaveRankSnapshot(HttpClient client, string gameId, string platform, string saveId, List<PlayerSaveData> entries)
        {
            UploadTokenRequest tokenRequest = new UploadTokenRequest
            {
                userId = RankSnapshotUserId,
                saveId = saveId,
                fileUploadRequest = new FileUploadSpec
                {
                    format = "",
                    originalName = "rank.json"
                }
            };
            string tokenJson = JsonConvert.SerializeObject(tokenRequest, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            HttpContent tokenContent = new StringContent(tokenJson, Encoding.UTF8, "application/json");
            HttpResponseMessage tokenResponse = await client.PostAsync("https://save.unity.cn/v1/saves/upload-token", tokenContent);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                string errBody = await tokenResponse.Content.ReadAsStringAsync();
                Debug.LogError($"SaveRankSnapshot: 获取上传令牌失败 status={(int)tokenResponse.StatusCode}, body={errBody}");
                tokenResponse.EnsureSuccessStatusCode();
            }

            string tokenBody = await tokenResponse.Content.ReadAsStringAsync();
            UploadTokenResponse tokenResult = JsonConvert.DeserializeObject<UploadTokenResponse>(tokenBody);

            if (tokenResult == null || tokenResult.fileUploadToken == null || string.IsNullOrEmpty(tokenResult.fileUploadToken.objectId))
            {
                FailRankWrite("SaveRankSnapshot: 上传令牌返回数据无效");

                return;
            }

            string resolvedSaveId = !string.IsNullOrEmpty(tokenResult.saveId) ? tokenResult.saveId : saveId;

            if (string.IsNullOrEmpty(resolvedSaveId))
            {
                FailRankWrite("SaveRankSnapshot: 上传令牌未返回 saveId");

                return;
            }

            byte[] fileBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(entries));
            await UploadObjectToCos(tokenResult.fileUploadToken, fileBytes);

            string snapshotName = "排行榜";

            if (platform == "wx")
            {
                snapshotName = "微信排行榜";
            }
            else if (platform == "dy")
            {
                snapshotName = "抖音排行榜";
            }

            CreateSaveRequest createRequest = new CreateSaveRequest
            {
                userId = RankSnapshotUserId,
                saveId = resolvedSaveId,
                name = snapshotName,
                saveNamespace = GetRankNamespace(gameId, platform),
                progressType = "LINEAR",
                fileUploadRequest = new FileUploadConfirmation
                {
                    clearExisting = false,
                    objectId = tokenResult.fileUploadToken.objectId
                }
            };
            string createJson = JsonConvert.SerializeObject(createRequest);
            HttpContent createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            HttpResponseMessage createResponse = await client.PostAsync("https://save.unity.cn/v1/saves", createContent);

            if (!createResponse.IsSuccessStatusCode)
            {
                string errBody = await createResponse.Content.ReadAsStringAsync();
                Debug.LogError($"SaveRankSnapshot: 创建或更新存档失败 status={(int)createResponse.StatusCode}, body={errBody}");
                createResponse.EnsureSuccessStatusCode();
            }
        }

        /// <summary>
        /// 使用临时密钥将快照文件上传到 COS
        /// </summary>
        private static async Task UploadObjectToCos(UploadToken token, byte[] fileBytes)
        {
            if (string.IsNullOrEmpty(token.tmpSecretId) || string.IsNullOrEmpty(token.tmpSecretKey) || string.IsNullOrEmpty(token.token)
                || string.IsNullOrEmpty(token.objectName))
            {
                FailRankWrite("UploadObjectToCos: 上传令牌字段不完整");

                return;
            }

            string objectPath = BuildCosObjectPath(token.objectDir, token.objectName);
            string host = "";
            string url = "";

            if (!string.IsNullOrEmpty(token.bucketUrl))
            {
                Uri bucketUri = new Uri(token.bucketUrl);
                host = bucketUri.Host;
                url = token.bucketUrl.TrimEnd('/') + objectPath;
            }
            else if (!string.IsNullOrEmpty(token.bucketName) && !string.IsNullOrEmpty(token.region))
            {
                host = $"{token.bucketName}.cos.{token.region}.myqcloud.com";
                url = $"https://{host}{objectPath}";
            }
            else
            {
                FailRankWrite("UploadObjectToCos: 缺少 bucketUrl 或 bucketName/region");

                return;
            }

            long startTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string keyTime = $"{startTime};{startTime + 600}";
            string signKey = HmacSha1Hex(token.tmpSecretKey, keyTime);
            string httpHeaders = $"host={Uri.EscapeDataString(host)}&x-cos-security-token={Uri.EscapeDataString(token.token)}";
            string httpString = $"put\n{objectPath}\n\n{httpHeaders}\n";
            string stringToSign = $"sha1\n{keyTime}\n{Sha1Hex(httpString)}\n";
            string signature = HmacSha1Hex(signKey, stringToSign);
            string authorization = $"q-sign-algorithm=sha1&q-ak={token.tmpSecretId}&q-sign-time={keyTime}&q-key-time={keyTime}&q-header-list=host;x-cos-security-token&q-url-param-list=&q-signature={signature}";

            using (HttpClient cosClient = new HttpClient())
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url))
            {
                request.Headers.TryAddWithoutValidation("Authorization", authorization);
                request.Headers.TryAddWithoutValidation("x-cos-security-token", token.token);
                request.Content = new ByteArrayContent(fileBytes);
                HttpResponseMessage response = await cosClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string errBody = await response.Content.ReadAsStringAsync();
                    Debug.LogError($"UploadObjectToCos: 上传失败 status={(int)response.StatusCode}, body={errBody}");
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        /// <summary>
        /// 排行榜写路径失败时中止调用，避免客户端把失败当成已处理
        /// </summary>
        private static void FailRankWrite(string message)
        {
            Debug.LogError(message);

            using (HttpResponseMessage failed = new HttpResponseMessage(HttpStatusCode.BadGateway))
            {
                failed.EnsureSuccessStatusCode();
            }
        }

        /// <summary>
        /// 按 rankKey 数值降序排列快照条目
        /// </summary>
        private static void SortRankEntries(List<PlayerSaveData> entries, string rankKey)
        {
            entries.Sort((left, right) =>
            {
                double leftScore = GetRankScore(left?.data, rankKey);
                double rightScore = GetRankScore(right?.data, rankKey);

                return rightScore.CompareTo(leftScore);
            });
        }

        /// <summary>
        /// 查找指定用户在快照中的下标，未找到返回 -1
        /// </summary>
        private static int FindRankEntryIndex(List<PlayerSaveData> entries, string userId)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && entries[i].userId == userId)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 非空昵称头像写入排行榜条目，空值保留旧资料
        /// </summary>
        private static void ApplyProfileData(Dictionary<string, string> data, string nickName, string avatarUrl)
        {
            if (data == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(nickName))
            {
                data[CloudDataKeys.ProfileNickName] = nickName;
            }

            if (!string.IsNullOrEmpty(avatarUrl))
            {
                data[CloudDataKeys.ProfileAvatarUrl] = avatarUrl;
            }
        }

        /// <summary>
        /// 校验平台标识是否为 wx 或 dy
        /// </summary>
        private static bool IsValidPlatform(string platform)
        {
            return platform == "wx" || platform == "dy";
        }

        /// <summary>
        /// 拼接排行榜快照 namespace
        /// </summary>
        private static string GetRankNamespace(string gameId, string platform)
        {
            return $"kv_{gameId}_rank_{platform}";
        }

        /// <summary>
        /// 拼接 COS 对象路径并对每一段做 URL 编码
        /// </summary>
        private static string BuildCosObjectPath(string objectDir, string objectName)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('/');

            if (!string.IsNullOrEmpty(objectDir))
            {
                string[] dirParts = objectDir.Split('/');

                for (int i = 0; i < dirParts.Length; i++)
                {
                    if (string.IsNullOrEmpty(dirParts[i]))
                    {
                        continue;
                    }

                    builder.Append(Uri.EscapeDataString(dirParts[i]));
                    builder.Append('/');
                }
            }

            builder.Append(Uri.EscapeDataString(objectName));

            return builder.ToString();
        }

        /// <summary>
        /// 计算 UTF-8 文本的 SHA1 十六进制摘要
        /// </summary>
        private static string Sha1Hex(string text)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                return BytesToHex(sha1.ComputeHash(Encoding.UTF8.GetBytes(text)));
            }
        }

        /// <summary>
        /// 计算 HMAC-SHA1 十六进制摘要
        /// </summary>
        private static string HmacSha1Hex(string key, string text)
        {
            using (HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(key)))
            {
                return BytesToHex(hmac.ComputeHash(Encoding.UTF8.GetBytes(text)));
            }
        }

        /// <summary>
        /// 将字节数组转为小写十六进制
        /// </summary>
        private static string BytesToHex(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }

            return builder.ToString();
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
        /// 校验游戏标识并获取对应密钥配置
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
        /// 用用户 ID 换取云存档令牌
        /// </summary>
        private async Task<PlatformLoginResult> GenerateToken(HttpClient client, string userId)
        {
            ApplyBasicAuth(client);

            string requestStr = JsonConvert.SerializeObject(new { userID = userId });
            HttpContent content = new StringContent(requestStr, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("https://p.unity.cn/v1/login/token", content);

            if (!response.IsSuccessStatusCode)
            {
                string errBody = await response.Content.ReadAsStringAsync();
                Debug.LogError($"GenerateToken 失败 status={(int)response.StatusCode}, body={errBody}");

                return null;
            }

            string body = await response.Content.ReadAsStringAsync();
            GenerateTokenResponse tokenResult = JsonConvert.DeserializeObject<GenerateTokenResponse>(body);

            if (tokenResult == null || string.IsNullOrEmpty(tokenResult.accessToken))
            {
                Debug.LogError("GenerateToken 返回数据无效");

                return null;
            }

            return new PlatformLoginResult
            {
                accessToken = tokenResult.accessToken,
                expiresAt = tokenResult.expiresAt,
                userID = userId
            };
        }

        /// <summary>
        /// 为 HttpClient 设置 UOS Basic 认证头
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