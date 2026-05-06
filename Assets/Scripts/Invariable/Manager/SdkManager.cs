using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Reflection;
using YooAsset;

#if !UNITY_EDITOR && MINIGAME_SUBPLATFORM_WEIXIN
using WeChatWASM;

#elif !UNITY_EDITOR && MINIGAME_SUBPLATFORM_DOUYIN
using TTSDK;
using static TTSDK.TTKeyboard;
using TTSDK.UNBridgeLib.LitJson;
#endif



namespace Invariable
{
    public class SdkManager : Singleton<SdkManager>
    {
#if !UNITY_EDITOR && MINIGAME_SUBPLATFORM_WEIXIN
        private WXCustomAd m_bannerAd = null;
        private WXRewardedVideoAd m_rewardedVideoAd = null;
        private WXGameClubButton wXGameClubButton = null;

#elif !UNITY_EDITOR && MINIGAME_SUBPLATFORM_DOUYIN
        private TTBannerAd m_bannerAd = null;
        private TTRewardedVideoAd m_rewardedVideoAd = null;
#endif

        private TMP_InputField m_inputField = null;
        private bool m_isKeyboardShowing = false;
        private List<ScreenAdapter> m_screenAdapters = null;
        private bool m_isShowBannerAd = false;
        private bool m_isShowRewardedVideoAd = false;
        private Action<bool> m_rewardedVideoAdCallBack = null;



        public void InitSDK(Action callBack = null)
        {
#if UNITY_EDITOR
            callBack?.Invoke();

#elif MINIGAME_SUBPLATFORM_WEIXIN
            WX.InitSDK((code) =>
            {
                AddOnShowListener();
                AddGameUpdateListener();
                callBack?.Invoke();
            });

#elif MINIGAME_SUBPLATFORM_DOUYIN
            TT.InitSDK((code, env) =>
            {
                AddOnShowListener();
                AddGameUpdateListener();
                callBack?.Invoke();
            });
#endif
        }

        #region 游戏生命周期事件监听
        private void AddOnShowListener()
        {
#if !UNITY_EDITOR && MINIGAME_SUBPLATFORM_WEIXIN


#elif !UNITY_EDITOR && MINIGAME_SUBPLATFORM_DOUYIN
            TT.GetAppLifeCycle().OnShow += (data) =>
            {
                if (!data.ContainsKey("launchFrom") || !data.ContainsKey("location"))
                {
                    return;
                }

                string launchFrom = data["launchFrom"].ToString();
                string location = data["location"].ToString();

                if (launchFrom == "homepage" && location == "sidebar_card")
                {
                    Debug.Log("侧边栏复访");
                }
            };
#endif
        }

        private void AddGameUpdateListener()
        {
#if !UNITY_EDITOR && MINIGAME_SUBPLATFORM_WEIXIN
            WXUpdateManager wXUpdateManager = WX.GetUpdateManager();

            wXUpdateManager.OnCheckForUpdate((result) =>
            {
                if (result.hasUpdate)
                {
                    Debug.Log("有新版本发布了！");
                }
            });

            wXUpdateManager.OnUpdateReady((result) =>
            {
                Debug.Log("重启游戏应用新版本！");
                wXUpdateManager.ApplyUpdate();
            });

#elif !UNITY_EDITOR && MINIGAME_SUBPLATFORM_DOUYIN
            TTUpdateManager tTUpdateManager = TT.GetUpdateManager();

            tTUpdateManager.OnCheckForUpdate((result) =>
            {
                if (result.HasUpdate)
                {
                    Debug.Log("有新版本发布了！");
                }
            });

            tTUpdateManager.OnUpdateReady(() =>
            {
                Debug.Log("重启游戏应用新版本！");
                tTUpdateManager.ApplyUpdate(new ApplyUpdateParams());
            });
#endif
        }
        #endregion

        #region 数据存储
        public void SetLocalData(string key, string data)
        {
#if UNITY_EDITOR
            PlayerPrefs.SetString(key, data);

#elif MINIGAME_SUBPLATFORM_WEIXIN
            WX.StorageSetStringSync(key, data);

#elif MINIGAME_SUBPLATFORM_DOUYIN
            TT.Save<string>(data, key);
#endif
        }

        public string GetLocalData(string key, string defaultValue = "")
        {
            string data = "";

#if UNITY_EDITOR
            data = PlayerPrefs.GetString(key, defaultValue);

#elif MINIGAME_SUBPLATFORM_WEIXIN
            data = WX.StorageGetStringSync(key, defaultValue);

#elif MINIGAME_SUBPLATFORM_DOUYIN
            data = TT.LoadSaving<string>(key);
#endif

            if (string.IsNullOrEmpty(data))
            {
                data = defaultValue;
            }

            SetLocalData(key, data);

            return data;
        }
        #endregion

        #region 输入框
        public void ShowKeyboard(TMP_InputField InputField)
        {
            if (m_isKeyboardShowing)
            {
                return;
            }

            m_inputField = InputField;

            m_isKeyboardShowing = true;

#if !UNITY_EDITOR && MINIGAME_SUBPLATFORM_WEIXIN
            WX.ShowKeyboard(new ShowKeyboardOption
            {
                defaultValue = m_inputField.text,
                maxLength = m_inputField.characterLimit > 0 ? m_inputField.characterLimit : 140,
                confirmType = "done"
            });

            WX.OnKeyboardInput(KeyboardInput);
            WX.OnKeyboardConfirm(KeyboardConfirm);
            WX.OnKeyboardComplete(KeyboardComplete);

#elif !UNITY_EDITOR && MINIGAME_SUBPLATFORM_DOUYIN
            TT.ShowKeyboard(new ShowKeyboardOptions
            {
                defaultValue = m_inputField.text,
                maxLength = m_inputField.characterLimit > 0 ? m_inputField.characterLimit : 140,
                confirmType = "done"
            });

            TT.OnKeyboardInput += KeyboardInput;
            TT.OnKeyboardConfirm += KeyboardConfirm;
            TT.OnKeyboardComplete += KeyboardComplete;
#endif
        }

        private void HideKeyboard()
        {
            if (!m_isKeyboardShowing)
            {
                return;
            }

#if !UNITY_EDITOR && MINIGAME_SUBPLATFORM_WEIXIN
            WX.OffKeyboardInput(KeyboardInput);
            WX.OffKeyboardConfirm(KeyboardConfirm);
            WX.OffKeyboardComplete(KeyboardComplete);
            WX.HideKeyboard(new HideKeyboardOption() { success = (data) => { m_isKeyboardShowing = false; } });

#elif !UNITY_EDITOR && MINIGAME_SUBPLATFORM_DOUYIN
            TT.OnKeyboardInput -= KeyboardInput;
            TT.OnKeyboardConfirm -= KeyboardConfirm;
            TT.OnKeyboardComplete -= KeyboardComplete;
            TT.HideKeyboard(() => { m_isKeyboardShowing = false; });
#endif
        }

#if !UNITY_EDITOR && MINIGAME_SUBPLATFORM_WEIXIN
        private void KeyboardInput(OnKeyboardInputListenerResult result)
        {
            m_inputField.text = result.value;
        }

        private void KeyboardConfirm(OnKeyboardInputListenerResult result)
        {
            m_inputField.text = result.value;
            HideKeyboard();
        }

        private void KeyboardComplete(OnKeyboardInputListenerResult result)
        {
            HideKeyboard();
        }

#elif !UNITY_EDITOR && MINIGAME_SUBPLATFORM_DOUYIN
        private void KeyboardInput(string result)
        {
            m_inputField.text = result;
        }

        private void KeyboardConfirm(string result)
        {
            m_inputField.text = result;
            HideKeyboard();
        }

        private void KeyboardComplete(string result)
        {
            HideKeyboard();
        }
#endif
        #endregion

        #region 屏幕适配
        public void AddScreenAdapter(ScreenAdapter screenAdapter)
        {
            m_screenAdapters ??= new List<ScreenAdapter>();

            if (!m_screenAdapters.Contains(screenAdapter))
            {
                m_screenAdapters.Add(screenAdapter);
            }

            if (m_screenAdapters.Count == 1)
            {
#if !UNITY_EDITOR && MINIGAME_SUBPLATFORM_WEIXIN
                WX.OnDeviceOrientationChange(DeviceOrientationChange);

#elif !UNITY_EDITOR && MINIGAME_SUBPLATFORM_DOUYIN
                TT.OnDeviceOrientationChange(DeviceOrientationChange);
#endif
            }

            DeviceOrientationChange(screenAdapter);
        }

        public void RemoveScreenAdapter(ScreenAdapter screenAdapter)
        {
            if (m_screenAdapters.Contains(screenAdapter))
            {
                m_screenAdapters.Remove(screenAdapter);
            }

            if (m_screenAdapters.Count <= 0)
            {
#if !UNITY_EDITOR && MINIGAME_SUBPLATFORM_WEIXIN
                WX.OffDeviceOrientationChange(DeviceOrientationChange);

#elif !UNITY_EDITOR && MINIGAME_SUBPLATFORM_DOUYIN
                TT.OffDeviceOrientationChange(DeviceOrientationChange);
#endif
            }
        }

        public void DeviceOrientationChange(ScreenAdapter screenAdapter = null)
        {
            if (screenAdapter != null)
            {
                GetSafeAnchor(out Vector2 offsetMin, out Vector2 offsetMax);
                RectTransform panel = screenAdapter.GetComponent<RectTransform>();
                panel.offsetMin = offsetMin;
                panel.offsetMax = offsetMax;
            }
            else
            {
                for (int i = 0; i < m_screenAdapters.Count; i++)
                {
                    GetSafeAnchor(out Vector2 offsetMin, out Vector2 offsetMax);
                    RectTransform panel = m_screenAdapters[i].GetComponent<RectTransform>();
                    panel.offsetMin = offsetMin;
                    panel.offsetMax = offsetMax;
                }
            }
        }

        /// <summary>
        /// 获取安全区域，固定写死
        /// </summary>
        /// <param name="offsetMin">相对于左下角的偏移量</param>
        /// <param name="offsetMax">相对于右上角的偏移量</param>
        public void GetSafeAnchor(out Vector2 offsetMin, out Vector2 offsetMax)
        {
            offsetMin = new Vector2(30, 130); // Left = 30, Bottom = 130
            offsetMax = new Vector2(-30, -90); // Right = 30, Top = 90
        }

#if !UNITY_EDITOR && MINIGAME_SUBPLATFORM_WEIXIN
        private void DeviceOrientationChange(OnDeviceOrientationChangeListenerResult result)
        {
            DeviceOrientationChange();
        }

#elif !UNITY_EDITOR && MINIGAME_SUBPLATFORM_DOUYIN
        private void DeviceOrientationChange(OnDeviceOrientationChangeResult result)
        {
            DeviceOrientationChange();
        }
#endif
        #endregion

        #region 广告
        public void ShowBannerAd(int left, int top, int width)
        {
            string adUnitId = "";

#if UNITY_EDITOR

#elif MINIGAME_SUBPLATFORM_WEIXIN
            if (m_bannerAd != null)
            {
                m_bannerAd.Show();
                return;
            }

            var info = WX.GetWindowInfo();
            double pixelRatio = info.pixelRatio;

            m_bannerAd = WX.CreateCustomAd(new WXCreateCustomAdParam()
            {
                adUnitId = adUnitId,

                style = new CustomStyle
                {
                    left = (int)(left / pixelRatio),
                    top = (int)(top / pixelRatio),
                    width = (int)(width / pixelRatio),
                }
            });

            m_isShowBannerAd = true;
            m_bannerAd.OnLoad((res) => {
                if (m_isShowBannerAd)
                {
                    m_isShowBannerAd = false;
                    m_bannerAd.Show();
                }
            });

            m_bannerAd.OnError((res) => {
                Debug.LogError($"Banner广告加载失败: {res.errMsg}");
            });

#elif MINIGAME_SUBPLATFORM_DOUYIN
            if (m_bannerAd != null)
            {
                m_bannerAd.Show();
                return;
            }

            var systemInfo = TT.GetSystemInfo();
            double pixelRatio = systemInfo.pixelRatio;

            m_bannerAd = TT.CreateBannerAd(new CreateBannerAdParam()
            {
                BannerAdId = adUnitId,

                Style = new TTBannerStyle
                {
                    left = (int)(left / pixelRatio),
                    top = (int)(top / pixelRatio),
                    width = (int)(width / pixelRatio),
                }
            });

            m_isShowBannerAd = true;
            m_bannerAd.OnLoad += () => {
                if (m_isShowBannerAd)
                {
                    m_isShowBannerAd = false;
                    m_bannerAd.Show();
                }
            };

            m_bannerAd.OnError += (code, message) => {
                Debug.LogError($"Banner广告加载失败: {message}");
            };
#endif
        }

        public void ShowRewardedVideoAd(Action<bool> callBack = null)
        {
            string adUnitId = "";

            m_rewardedVideoAdCallBack = callBack;

#if UNITY_EDITOR

#elif MINIGAME_SUBPLATFORM_WEIXIN
            if (m_rewardedVideoAd != null)
            {
                m_rewardedVideoAd.Show();
                return;
            }

            m_rewardedVideoAd = WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam
            {
                adUnitId = adUnitId
            });

            m_isShowRewardedVideoAd = true;
            m_rewardedVideoAd.OnLoad((res) => {
                if (m_isShowRewardedVideoAd)
                {
                    m_isShowRewardedVideoAd = false;
                    m_rewardedVideoAd.Show();
                }
            });

            m_rewardedVideoAd.OnError((res) => {
                Debug.LogError($"激励视频广告错误: {res.errMsg}");
            });

            m_rewardedVideoAd.OnClose((res) => {
                m_rewardedVideoAdCallBack?.Invoke(res != null && res.isEnded);
            });

#elif MINIGAME_SUBPLATFORM_DOUYIN
            if (m_rewardedVideoAd != null)
            {
                m_rewardedVideoAd.Show();
                return;
            }

            m_rewardedVideoAd = TT.CreateRewardedVideoAd(new CreateRewardedVideoAdParam
            {
                AdUnitId = adUnitId
            });

            m_isShowRewardedVideoAd = true;
            m_rewardedVideoAd.OnLoad += () => {
                if (m_isShowRewardedVideoAd)
                {
                    m_isShowRewardedVideoAd = false;
                    m_rewardedVideoAd.Show();
                }
            };

            m_rewardedVideoAd.OnError += (code, message) => {
                Debug.LogError($"激励视频广告错误: {message}");
            };

            m_rewardedVideoAd.OnClose += (isEnded, count) => {
                m_rewardedVideoAdCallBack?.Invoke(isEnded);
            };
#endif
        }
        #endregion

        #region 侧边栏复访
        public void ShowSidebar()
        {
#if !UNITY_EDITOR && MINIGAME_SUBPLATFORM_DOUYIN
            TT.CheckScene(TTSideBar.SceneEnum.SideBar, (isJump) =>
            {
                if (isJump)
                {
                    var data = new JsonData
                    {
                        ["scene"] = "sidebar",
                    };

                    TT.NavigateToScene(data, () =>
                    {
                        SetLocalData("IsGetReward", "1");
                    }, null, null);
                }
            }, null, null);
#endif
        }
        #endregion

        #region 游戏圈
        public void ShowGameClubButton(Rect rect = default, float fontSize = 0)
        {
#if !UNITY_EDITOR && MINIGAME_SUBPLATFORM_WEIXIN
            if (wXGameClubButton == null)
            {
                var info = WX.GetWindowInfo();
                double pixelRatio = info.pixelRatio;

                wXGameClubButton = WX.CreateGameClubButton(new WXCreateGameClubButtonParam()
                {
                    type = GameClubButtonType.text,
                    text = "",
                    style = new GameClubButtonStyle()
                    {
                        //位置和大小
                        left = (int)(rect.x / pixelRatio),
                        top = (int)(rect.y / pixelRatio),
                        width = (int)(rect.width / pixelRatio),
                        height = (int)(rect.height / pixelRatio),

                        // 背景颜色
                        backgroundColor = "#FFFFFF00",

                        // 文字样式
                        color = "#BBDD88",
                        textAlign = GameClubButtonTextAlign.center,
                        fontSize = (int)(fontSize / pixelRatio),
                        lineHeight = (int)((fontSize + 10) / pixelRatio),

                        // 边框样式
                        borderColor = "#00000000",
                        borderWidth = 1,
                        //borderRadius = 30 // 圆角
                    }
                });
            }
            else
            {
                wXGameClubButton.Show();
            }
#endif
        }

        public void HideGameClubButton()
        {
#if !UNITY_EDITOR && MINIGAME_SUBPLATFORM_WEIXIN
            if (wXGameClubButton == null)
            {
                return;
            }

            wXGameClubButton.Hide();
#endif
        }
        #endregion

        #region 环境
        public bool IsWeChat()
        {
#if !UNITY_EDITOR && MINIGAME_SUBPLATFORM_WEIXIN
            return true;
#else
            return false;
#endif
        }

        public bool IsDouYin()
        {
#if !UNITY_EDITOR && MINIGAME_SUBPLATFORM_DOUYIN
            return true;
#else
            return false;
#endif
        }
        #endregion

        #region YooAsset
        public void InitializeYooAsset(ref WebPlayModeParameters createParameters, RemoteServices remoteServices)
        {
#if !UNITY_EDITOR && MINIGAME_SUBPLATFORM_WEIXIN
            string packageRoot = $"{WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE/yoo";
            createParameters.WebServerFileSystemParameters = WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices);

#elif !UNITY_EDITOR && MINIGAME_SUBPLATFORM_DOUYIN
            string packageRoot = "yoo";
            createParameters.WebServerFileSystemParameters = TiktokFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices);
#endif
        }
        #endregion
    }
}