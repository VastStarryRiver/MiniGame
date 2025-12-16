using System;
using System.Collections.Generic;
using UnityEngine;



public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private struct callBackData
    {
        public float currTime;
        public float needTime;
        public Action callBack;
    }

    private Dictionary<string, List<Action<object>>> m_event = null;
    private Dictionary<string, callBackData> m_callBackInvoke = null;
    private Dictionary<string, callBackData> m_callBackRepeating = null;
    private List<string> m_callBackKeys = null;



    private void Awake()
    {
        m_event = new Dictionary<string, List<Action<object>>>();
        m_callBackInvoke = new Dictionary<string, callBackData>();
        m_callBackRepeating = new Dictionary<string, callBackData>();
        m_callBackKeys = new List<string>();
        Instance = this;
    }

    private void Update()
    {
        for (int i = 0; i < m_callBackKeys.Count; i++)
        {
            if (m_callBackInvoke.ContainsKey(m_callBackKeys[i]))
            {
                float currTime = m_callBackInvoke[m_callBackKeys[i]].currTime + Time.deltaTime;

                if (currTime >= m_callBackInvoke[m_callBackKeys[i]].needTime)
                {
                    m_callBackInvoke[m_callBackKeys[i]].callBack?.Invoke();
                    m_callBackInvoke.Remove(m_callBackKeys[i]);
                }
                else
                {
                    m_callBackInvoke[m_callBackKeys[i]] = new callBackData
                    {
                        currTime = currTime,
                        needTime = m_callBackInvoke[m_callBackKeys[i]].needTime,
                        callBack = m_callBackInvoke[m_callBackKeys[i]].callBack,
                    };
                }
            }
            else if (m_callBackRepeating.ContainsKey(m_callBackKeys[i]))
            {
                float currTime = m_callBackRepeating[m_callBackKeys[i]].currTime + Time.deltaTime;

                if (currTime >= m_callBackRepeating[m_callBackKeys[i]].needTime)
                {
                    m_callBackRepeating[m_callBackKeys[i]].callBack?.Invoke();

                    m_callBackRepeating[m_callBackKeys[i]] = new callBackData
                    {
                        currTime = 0,
                        needTime = m_callBackRepeating[m_callBackKeys[i]].needTime,
                        callBack = m_callBackRepeating[m_callBackKeys[i]].callBack,
                    };
                }
                else
                {
                    m_callBackRepeating[m_callBackKeys[i]] = new callBackData
                    {
                        currTime = currTime,
                        needTime = m_callBackRepeating[m_callBackKeys[i]].needTime,
                        callBack = m_callBackRepeating[m_callBackKeys[i]].callBack,
                    };
                }
            }
        }
    }



    public void AddEventListener(string key, Action<object> callBack)
    {
        if (!m_event.ContainsKey(key))
        {
            m_event.Add(key, new List<Action<object>>());
        }

        if (!m_event[key].Contains(callBack))
        {
            m_event[key].Add(callBack);
        }
    }

    public void RemoveEventListener(string key, Action<object> callBack)
    {
        if (m_event == null || !m_event.ContainsKey(key) || !m_event[key].Contains(callBack))
        {
            return;
        }

        m_event[key].Remove(callBack);

        if (m_event[key].Count <= 0)
        {
            m_event.Remove(key);
        }
    }

    public void InvokeEventCallBack(string key, object arg = null)
    {
        if (m_event == null || !m_event.ContainsKey(key) || m_event[key].Count <= 0)
        {
            return;
        }

        int count = m_event[key].Count;

        for (int i = 0; i < count; i++)
        {
            m_event[key][i].Invoke(arg);
        }
    }

    public void AddDelayInvoke(string key, Action callBack, float needTime)
    {
        if (m_callBackKeys.Contains(key))
        {
            return;
        }

        m_callBackKeys.Add(key);

        m_callBackInvoke.Add(key, new callBackData
        {
            currTime = 0,
            needTime = needTime,
            callBack = callBack,
        });
    }

    public void AddInvokeRepeating(string key, Action callBack, float needTime)
    {
        if (m_callBackKeys.Contains(key))
        {
            return;
        }

        m_callBackKeys.Add(key);

        m_callBackRepeating.Add(key, new callBackData
        {
            currTime = 0,
            needTime = needTime,
            callBack = callBack,
        });
    }

    public void CancelInvokeByKey(string key)
    {
        if (m_callBackKeys.Contains(key))
        {
            m_callBackKeys.Remove(key);
        }

        if (m_callBackInvoke.ContainsKey(key))
        {
            m_callBackInvoke.Remove(key);
        }

        if (m_callBackRepeating.ContainsKey(key))
        {
            m_callBackRepeating.Remove(key);
        }
    }
}