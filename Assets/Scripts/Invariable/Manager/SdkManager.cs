using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Reflection;

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
        private Dictionary<string, WXCustomAd> m_bannerAd = null;
        private Dictionary<string, WXRewardedVideoAd> m_rewardedVideoAd = null;

#elif !UNITY_EDITOR && MINIGAME_SUBPLATFORM_DOUYIN
        private Dictionary<string, TTBannerAd> m_bannerAd = null;
        private Dictionary<string, TTRewardedVideoAd> m_rewardedVideoAd = null;
#endif

        private TMP_InputField m_inputField = null;
        private bool m_isKeyboardShowing = false;
        private List<ScreenAdapter> m_screenAdapters = null;



        public void InitSDK(Action callBack = null)
        {
#if UNITY_EDITOR
            callBack?.Invoke();

#elif MINIGAME_SUBPLATFORM_WEIXIN
            WX.InitSDK((code) =>
            {
                AddOnShowListener();
                AddGameUpdateListener();
                m_bannerAd ??= new Dictionary<string, WXCustomAd>();
                m_rewardedVideoAd ??= new Dictionary<string, WXRewardedVideoAd>();
                callBack?.Invoke();
            });

#elif MINIGAME_SUBPLATFORM_DOUYIN
            TT.InitSDK((code, env) =>
            {
                AddOnShowListener();
                AddGameUpdateListener();
                m_bannerAd ??= new Dictionary<string, TTBannerAd>();
                m_rewardedVideoAd ??= new Dictionary<string, TTRewardedVideoAd>();
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

#if !UNITY_EDITOR && MINIGAME_SUBPLATFORM_WEIXIN


#elif !UNITY_EDITOR && MINIGAME_SUBPLATFORM_DOUYIN
        public void ShowSidebar()
        {
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

                    }, null, null);
                }
            }, null, null);
        }
#endif
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
            ConfigUtils.GetConfigData("BannerAd", (config) =>
            {
                int index = 1;

                if (config.Count >= 2)
                {
                    index = UnityEngine.Random.Range(1, config.Count + 1);
                }

                string adUnitId = config[index.ToString()]["AdUnitId"];

                if (adUnitId == "0")
                {
                    return;
                }

#if UNITY_EDITOR

#elif MINIGAME_SUBPLATFORM_WEIXIN
                if (!m_bannerAd.ContainsKey(adUnitId))
                {
                    WXCustomAd bannerAd = WX.CreateCustomAd(new WXCreateCustomAdParam()
                    {
                        adUnitId = adUnitId,

                        style = new CustomStyle
                        {
                            left = left, // 左边距（像素）
                            top = top,  // 上边距（像素），0表示顶部，如果是底部，可以设置为 Screen.height - 广告高度
                            width = width, // 广告宽度，单位像素，广告会自动等比缩放
                        }
                    });

                    bannerAd.OnLoad((res) => {
                        if (m_bannerAd.ContainsKey(adUnitId))
                        {
                            return;
                        }

                        m_bannerAd.Add(adUnitId, bannerAd);

                        bannerAd.Show();
                    });

                    bannerAd.OnError((res) => {
                        Debug.LogError($"Banner广告加载失败: {res.errMsg}");
                    });
                }
                else
                {
                    m_bannerAd[adUnitId].Show();
                }

#elif MINIGAME_SUBPLATFORM_DOUYIN
                if (!m_bannerAd.ContainsKey(adUnitId))
                {
                    TTBannerAd bannerAd = TT.CreateBannerAd(new CreateBannerAdParam()
                    {
                        BannerAdId = adUnitId,

                        Style = new TTBannerStyle
                        {
                            left = left, // 左边距（像素）
                            top = top,  // 上边距（像素），0表示顶部，如果是底部，可以设置为 Screen.height - 广告高度
                            width = width, // 广告宽度，单位像素，广告会自动等比缩放
                        }
                    });

                    bannerAd.OnLoad += () => {
                        if (m_bannerAd.ContainsKey(adUnitId))
                        {
                            return;
                        }

                        m_bannerAd.Add(adUnitId, bannerAd);

                        bannerAd.Show();
                    };

                    bannerAd.OnError += (code, message) => {
                        Debug.LogError($"Banner广告加载失败: {message}");
                    };
                }
                else
                {
                    m_bannerAd[adUnitId].Show();
                }
#endif
            });
        }

        public void HideBannerAd(string adUnitId = "")
        {
#if UNITY_EDITOR

#else
            if (string.IsNullOrEmpty(adUnitId))
            {
                foreach (var item in m_bannerAd)
                {
                    item.Value.Hide();
                }
            }
            else if (m_bannerAd.ContainsKey(adUnitId))
            {
                m_bannerAd[adUnitId].Hide();
            }
#endif
        }

        public void ShowRewardedVideoAd(Action callBack = null)
        {
            ConfigUtils.GetConfigData("RewardedVideoAd", (config) =>
            {
                int index = 1;

                if (config.Count >= 2)
                {
                    index = UnityEngine.Random.Range(1, config.Count + 1);
                }

                string adUnitId = config[index.ToString()]["AdUnitId"];

                if (adUnitId == "0")
                {
                    return;
                }

#if UNITY_EDITOR

#elif MINIGAME_SUBPLATFORM_WEIXIN
                if (!m_rewardedVideoAd.ContainsKey(adUnitId))
                {
                    WXRewardedVideoAd rewardedVideoAd = WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam
                    {
                        adUnitId = adUnitId
                    });

                    rewardedVideoAd.OnLoad((res) => {
                        if (!m_rewardedVideoAd.ContainsKey(adUnitId))
                        {
                            m_rewardedVideoAd.Add(adUnitId, rewardedVideoAd);
                        }

                        rewardedVideoAd.Show();
                    });

                    rewardedVideoAd.OnError((res) => {
                        Debug.LogError($"激励视频广告错误: {res.errMsg}");
                    });

                    rewardedVideoAd.OnClose((res) => {
                        if (res != null && res.isEnded)
                        {
                            callBack?.Invoke();
                        }
                    });

                    rewardedVideoAd.Load();
                }
                else
                {
                    m_rewardedVideoAd[adUnitId].Load();
                }

#elif MINIGAME_SUBPLATFORM_DOUYIN
                if (!m_rewardedVideoAd.ContainsKey(adUnitId))
                {
                    TTRewardedVideoAd rewardedVideoAd = TT.CreateRewardedVideoAd(new CreateRewardedVideoAdParam
                    {
                        AdUnitId = adUnitId
                    });

                    rewardedVideoAd.OnLoad += () => {
                        if (!m_rewardedVideoAd.ContainsKey(adUnitId))
                        {
                            m_rewardedVideoAd.Add(adUnitId, rewardedVideoAd);
                        }

                        rewardedVideoAd.Show();
                    };

                    rewardedVideoAd.OnError += (code, message) => {
                        Debug.LogError($"激励视频广告错误: {message}");
                    };

                    rewardedVideoAd.OnClose += (isEnded, count) => {
                        if (isEnded)
                        {
                            callBack?.Invoke();
                        }
                    };

                    rewardedVideoAd.Load();
                }
                else
                {
                    m_rewardedVideoAd[adUnitId].Load();
                }
#endif
            });
        }
        #endregion
    }
}