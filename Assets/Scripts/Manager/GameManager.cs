using System;
using System.Collections.Generic;
using UnityEngine;



public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private Dictionary<string, List<Action<object>>> m_event;



    private void Awake()
    {
        Instance = this;
    }



    public void AddEventListener(string key, Action<object> callBack)
    {
        m_event ??= new Dictionary<string, List<Action<object>>>();

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
}