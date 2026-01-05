using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

#if !UNITY_EDITOR && MINIGAME_SUBPLATFORM_WEIXIN
using WeChatWASM;

#elif !UNITY_EDITOR && MINIGAME_SUBPLATFORM_DOUYIN
using TTSDK;
using static TTSDK.TTKeyboard;
#endif



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
            WXUpdateManager wXUpdateManager = WX.GetUpdateManager();

            wXUpdateManager.OnUpdateReady((result) =>
            {
                callBack?.Invoke();
            });

            wXUpdateManager.OnCheckForUpdate((result) =>
            {
                if (result.hasUpdate)
                {
                    wXUpdateManager.ApplyUpdate();
                }
                else
                {
                    callBack?.Invoke();
                }
            });
        });

#elif MINIGAME_SUBPLATFORM_DOUYIN
        TT.InitSDK((code, env) =>
        {
            TTUpdateManager tTUpdateManager = TT.GetUpdateManager();

            tTUpdateManager.OnCheckForUpdate((result) =>
            {
                if (result.HasUpdate)
                {
                    ApplyUpdateParams applyUpdateParams = new ApplyUpdateParams();

                    applyUpdateParams.Complete = () =>
                    {
                        callBack?.Invoke();
                    };

                    tTUpdateManager.ApplyUpdate(applyUpdateParams);
                }
                else
                {
                    callBack?.Invoke();
                }
            });
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

    public void GetSafeAnchor(out Vector2 anchorMin, out Vector2 anchorMax)
    {
        anchorMin = Vector2.zero;
        anchorMax = Vector2.zero;

        int height = 0;
        int width = 0;

#if UNITY_EDITOR
        Rect safeArea = Screen.safeArea; // 原点在左下角

        height = Screen.height;
        width = Screen.width;

        anchorMin = safeArea.position;
        anchorMax = safeArea.position + safeArea.size;

#elif MINIGAME_SUBPLATFORM_WEIXIN
        var windowInfo = WX.GetWindowInfo();
        SafeArea safeArea = windowInfo.safeArea; // 原点在左上角
        double pixelRatio = windowInfo.pixelRatio; // 获取设备像素比

        float left = (float)(safeArea.left * pixelRatio);
        float right = (float)(safeArea.right * pixelRatio);
        float top = (float)(safeArea.top * pixelRatio);
        float bottom = (float)(safeArea.bottom * pixelRatio);

        height = (int)(windowInfo.screenHeight * pixelRatio);
        width = (int)(windowInfo.screenWidth * pixelRatio);

        anchorMin = new Vector2(left, height - bottom);
        anchorMax = new Vector2(right, height - top);

#elif MINIGAME_SUBPLATFORM_DOUYIN
        var systemInfo = TT.GetSystemInfo();
        SafeArea safeArea = systemInfo.safeArea; // 原点在左上角
        double pixelRatio = systemInfo.pixelRatio; // 获取设备像素比

        float left = (float)(safeArea.left * pixelRatio);
        float right = (float)(safeArea.right * pixelRatio);
        float top = (float)(safeArea.top * pixelRatio);
        float bottom = (float)(safeArea.bottom * pixelRatio);

        height = (int)(systemInfo.screenHeight * pixelRatio);
        width = (int)(systemInfo.screenWidth * pixelRatio);

        anchorMin = new Vector2(left, height - bottom);
        anchorMax = new Vector2(right, height - top);
#endif

        anchorMin.x /= width;
        anchorMin.y /= height;
        anchorMax.x /= width;
        anchorMax.y /= height;
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

    private void DeviceOrientationChange(ScreenAdapter screenAdapter = null)
    {
        GetSafeAnchor(out Vector2 anchorMin, out Vector2 anchorMax);

        if (screenAdapter != null)
        {
            RectTransform panel = screenAdapter.GetComponent<RectTransform>();
            panel.anchorMin = anchorMin;
            panel.anchorMax = anchorMax;
        }
        else
        {
            for (int i = 0; i < m_screenAdapters.Count; i++)
            {
                RectTransform panel = m_screenAdapters[i].GetComponent<RectTransform>();
                panel.anchorMin = anchorMin;
                panel.anchorMax = anchorMax;
            }
        }
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
#endif
}