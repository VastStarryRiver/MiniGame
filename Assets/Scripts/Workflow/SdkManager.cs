using System;
using UnityEngine;

#if !UNITY_EDITOR && MINIGAME_SUBPLATFORM_WEIXIN
using WeChatWASM;

#elif !UNITY_EDITOR && MINIGAME_SUBPLATFORM_DOUYIN
using TTSDK;
#endif



public class SdkManager : Singleton<SdkManager>
{
#if UNITY_EDITOR
    Rect lastSafeArea;

#elif MINIGAME_SUBPLATFORM_WEIXIN
    SafeArea lastSafeArea;

#elif MINIGAME_SUBPLATFORM_DOUYIN
    SafeArea lastSafeArea;
#endif



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

    public bool JudgeSafeArea()
    {
#if UNITY_EDITOR
        Rect safeArea = Screen.safeArea;
        return lastSafeArea != safeArea;

#elif MINIGAME_SUBPLATFORM_WEIXIN
        SafeArea safeArea = WX.GetWindowInfo().safeArea;
        return lastSafeArea.left != safeArea.left || lastSafeArea.right != safeArea.right || lastSafeArea.top != safeArea.top || lastSafeArea.bottom != safeArea.bottom || lastSafeArea.width != safeArea.width || lastSafeArea.height != safeArea.height;

#elif MINIGAME_SUBPLATFORM_DOUYIN
        SafeArea safeArea = TT.GetSystemInfo().safeArea;
        return lastSafeArea.left != safeArea.left || lastSafeArea.right != safeArea.right || lastSafeArea.top != safeArea.top || lastSafeArea.bottom != safeArea.bottom || lastSafeArea.width != safeArea.width || lastSafeArea.height != safeArea.height;

#else
        return false;
#endif
    }

    public void SetSafeArea()
    {
#if UNITY_EDITOR
        lastSafeArea = Screen.safeArea;

#elif MINIGAME_SUBPLATFORM_WEIXIN
        lastSafeArea = WX.GetWindowInfo().safeArea;

#elif MINIGAME_SUBPLATFORM_DOUYIN
        lastSafeArea = TT.GetSystemInfo().safeArea;
#endif
    }

    public void GetSafeAnchor(out Vector2 anchorMin, out Vector2 anchorMax)
    {
        anchorMin = Vector2.zero;
        anchorMax = Vector2.zero;

#if UNITY_EDITOR
        Rect safeArea = Screen.safeArea;//原点在左下角

        anchorMin = safeArea.position;
        anchorMax = safeArea.position + safeArea.size;

#elif MINIGAME_SUBPLATFORM_WEIXIN
        SafeArea safeArea = WX.GetWindowInfo().safeArea;//原点在左上角
        double pixelRatio = WX.GetWindowInfo().pixelRatio;//获取设备像素比

        float left = (float)(safeArea.left * pixelRatio);
        float right = (float)(safeArea.right * pixelRatio);
        float top = (float)(safeArea.top * pixelRatio);
        float bottom = (float)(safeArea.bottom * pixelRatio);

        anchorMin = new Vector2(left, Screen.height - bottom);
        anchorMax = new Vector2(right, Screen.height - top);

#elif MINIGAME_SUBPLATFORM_DOUYIN
        SafeArea safeArea = TT.GetSystemInfo().safeArea;//原点在左上角
        double pixelRatio = TT.GetSystemInfo().pixelRatio;//获取设备像素比

        float left = (float)(safeArea.left * pixelRatio);
        float right = (float)(safeArea.right * pixelRatio);
        float top = (float)(safeArea.top * pixelRatio);
        float bottom = (float)(safeArea.bottom * pixelRatio);

        anchorMin = new Vector2(left, Screen.height - bottom);
        anchorMax = new Vector2(right, Screen.height - top);
#endif

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
    }
}