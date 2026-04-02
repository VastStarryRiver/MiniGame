using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

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

        /// <summary>
        /// 获取安全区域，固定写死
        /// </summary>
        /// <param name="offsetMin">相对于左下角的偏移量</param>
        /// <param name="offsetMax">相对于右上角的偏移量</param>
        public void GetSafeAnchor(out Vector2 offsetMin, out Vector2 offsetMax)
        {
            bool isChange = true;

#if UNITY_EDITOR
            isChange = false;

#elif MINIGAME_SUBPLATFORM_WEIXIN
            var systemInfo = WX.GetDeviceInfo();

            if (systemInfo.platform == "windows" || systemInfo.platform == "mac" || systemInfo.platform == "ohos_pc")
            {
                isChange = false;
            }

#elif MINIGAME_SUBPLATFORM_DOUYIN
            var systemInfo = TT.GetSystemInfo();

            if (systemInfo.platform == "windows" || systemInfo.platform == "mac" || systemInfo.platform == "ohos_pc")
            {
                isChange = false;
            }
#endif

            if (isChange)
            {
                offsetMin = new Vector2(30, 130); // Left = 30, Bottom = 130
                offsetMax = new Vector2(-30, -90); // Right = 30, Top = 90
            }
            else
            {
                offsetMin = Vector2.zero;
                offsetMax = Vector2.zero;
            }
        }

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

        private void DeviceOrientationChange(OnDeviceOrientationChangeListenerResult result)
        {
            DeviceOrientationChange();
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

        private void DeviceOrientationChange(OnDeviceOrientationChangeResult result)
        {
            DeviceOrientationChange();
        }

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
    }
}