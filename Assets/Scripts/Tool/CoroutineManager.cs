using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;



public class CoroutineManager : MonoBehaviour
{
    private struct callBackData
    {
        public float currTime;
        public float needTime;
        public Action callBack;
    }

    public static CoroutineManager Instance = null;
    private Dictionary<string, callBackData> callBackInvoke = null;
    private Dictionary<string, callBackData> callBackRepeating = null;
    private List<string> callBackKeys = null;



    private void Awake()
    {
        Instance = this;
        callBackInvoke = new Dictionary<string, callBackData>();
        callBackRepeating = new Dictionary<string, callBackData>();
        callBackKeys = new List<string>();
    }

    private void Update()
    {
        for (int i = 0; i < callBackKeys.Count; i++)
        {
            if (callBackInvoke.ContainsKey(callBackKeys[i]))
            {
                float currTime = callBackInvoke[callBackKeys[i]].currTime + Time.deltaTime;

                if(currTime >= callBackInvoke[callBackKeys[i]].needTime)
                {
                    callBackInvoke[callBackKeys[i]].callBack?.Invoke();
                    callBackInvoke.Remove(callBackKeys[i]);
                }
                else
                {
                    callBackInvoke[callBackKeys[i]] = new callBackData
                    {
                        currTime = currTime,
                        needTime = callBackInvoke[callBackKeys[i]].needTime,
                        callBack = callBackInvoke[callBackKeys[i]].callBack,
                    };
                }
            }
            else if(callBackRepeating.ContainsKey(callBackKeys[i]))
            {
                float currTime = callBackRepeating[callBackKeys[i]].currTime + Time.deltaTime;

                if (currTime >= callBackRepeating[callBackKeys[i]].needTime)
                {
                    callBackRepeating[callBackKeys[i]].callBack?.Invoke();

                    callBackRepeating[callBackKeys[i]] = new callBackData
                    {
                        currTime = 0,
                        needTime = callBackRepeating[callBackKeys[i]].needTime,
                        callBack = callBackRepeating[callBackKeys[i]].callBack,
                    };
                }
                else
                {
                    callBackRepeating[callBackKeys[i]] = new callBackData
                    {
                        currTime = currTime,
                        needTime = callBackRepeating[callBackKeys[i]].needTime,
                        callBack = callBackRepeating[callBackKeys[i]].callBack,
                    };
                }
            }
        }
    }

    public void InvokePlayAnimation(UnityEngine.Object obj, string childPath = "", string animName = "", WrapMode wrapMode = WrapMode.Once, Action callBack = null)
    {
        StartCoroutine(PlayAnimation(obj, childPath, animName, wrapMode, callBack));
    }

    private IEnumerator PlayAnimation(UnityEngine.Object obj, string childPath = "", string animName = "", WrapMode wrapMode = WrapMode.Once, Action callBack = null)
    {
        if (string.IsNullOrEmpty(animName))
        {
            yield break;
        }

        Transform trans = ConvenientUtility.GetTransform(obj);

        if (trans == null)
        {
            yield break;
        }

        if (!string.IsNullOrEmpty(childPath))
        {
            trans = trans.Find(childPath);
        }

        if (trans == null)
        {
            yield break;
        }

        Animation animation = trans.GetComponent<Animation>();

        if (animation == null)
        {
            yield break;
        }

        animation.wrapMode = wrapMode;

        animation.Play(animName);

        if (wrapMode == WrapMode.Once)
        {
            yield return new WaitWhile(() => animation.isPlaying);

            if (callBack != null)
            {
                callBack.Invoke();
            }
        }
    }

    public void InvokeRestartGame()
    {
        StartCoroutine(RestartGame());
    }

    private IEnumerator RestartGame()
    {
        List<int> list = new List<int>();

        foreach (var item in UIManager.Instance.m_panelList)
        {
            list.Add(item.Key);
        }

        if (list.Count > 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                ConvenientUtility.CloseUIPrefabPanel(UIManager.Instance.m_panelList[list[i]]);
            }
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            Debug.Log("加载进度: " + (progress * 100) + "%");
            yield return null;
        }
    }

    public void AddDelayInvoke(string key, Action callBack, float needTime)
    {
        if(callBackKeys.Contains(key))
        {
            return;
        }

        callBackKeys.Add(key);

        callBackInvoke.Add(key, new callBackData
        {
            currTime = 0,
            needTime = needTime,
            callBack = callBack,
        });
    }

    public void AddInvokeRepeating(string key, Action callBack, float needTime)
    {
        if (callBackKeys.Contains(key))
        {
            return;
        }

        callBackKeys.Add(key);

        callBackRepeating.Add(key, new callBackData
        {
            currTime = 0,
            needTime = needTime,
            callBack = callBack,
        });
    }

    public void CancelInvokeByKey(string key)
    {
        if (callBackKeys.Contains(key))
        {
            callBackKeys.Remove(key);
        }

        if (callBackInvoke.ContainsKey(key))
        {
            callBackInvoke.Remove(key);
        }

        if (callBackRepeating.ContainsKey(key))
        {
            callBackRepeating.Remove(key);
        }
    }
}