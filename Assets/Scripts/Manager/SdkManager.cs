using System;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json;
using WeChatWASM;



public class SdkManager : Singleton<SdkManager>
{
#if UNITY_EDITOR
    Rect lastSafeArea;

#elif UNITY_WEBGL
    SafeArea lastSafeArea;
#endif



    public void InitMiniGameSDK(Action callBack = null)
    {
#if UNITY_EDITOR
        callBack?.Invoke();

#elif UNITY_WEBGL
        WX.InitSDK((code) =>
        {
            WXUpdateManager wXUpdateManager = WX.GetUpdateManager();

            wXUpdateManager.OnUpdateReady((result) =>
            {
                WX.cloud.Init(new ICloudConfig() { env = "" });
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
                    WX.cloud.Init(new ICloudConfig() { env = "" });
                    callBack?.Invoke();
                }
            });
        });
#endif
    }

    public void SetLocalData(string key, string data)
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        WX.StorageSetStringSync(key, data);
#else
        PlayerPrefs.SetString(key, data);
#endif
    }

    public string GetLocalData(string key, string defaultValue = "")
    {
        string data = "";

#if !UNITY_EDITOR && UNITY_WEBGL
        data = WX.StorageGetStringSync(key, defaultValue);
#else
        data = PlayerPrefs.GetString(key, defaultValue);
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

#elif UNITY_WEBGL
        SafeArea safeArea = WX.GetWindowInfo().safeArea;
        return lastSafeArea.left != safeArea.left || lastSafeArea.right != safeArea.right || lastSafeArea.top != safeArea.top || lastSafeArea.bottom != safeArea.bottom || lastSafeArea.width != safeArea.width || lastSafeArea.height != safeArea.height;

#else
        return false;
#endif
    }

    public void SetSafeArea()
    {
#if UNITY_EDITOR
        lastSafeArea = Screen.safeArea;

#elif UNITY_WEBGL
        lastSafeArea = WX.GetWindowInfo().safeArea;
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

#elif UNITY_WEBGL
        SafeArea safeArea = WX.GetWindowInfo().safeArea;//原点在左上角
        double pixelRatio = WX.GetWindowInfo().pixelRatio;//获取设备像素比

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

    public void CallCloudFunction(string functionName, object data, Action<string> onSuccess = null, Action onError = null)
    {
#if UNITY_EDITOR
        return;

#elif UNITY_WEBGL
        WX.cloud.CallFunction(new CallFunctionParam
        {
            name = functionName,
            data = data,
            success = (res) => {
                string json = res.result;
                onSuccess?.Invoke(json);
            },
            fail = (err) => {
                onError?.Invoke();
            }
        });
#endif
    }
}