using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;



public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private Dictionary<string, List<Action<object>>> m_event = null;
    private Dictionary<string, CancellationTokenSource> m_cancellationTokenSources = null;



    private void Awake()
    {
        m_event = new Dictionary<string, List<Action<object>>>();
        m_cancellationTokenSources = new Dictionary<string, CancellationTokenSource>();
        Instance = this;
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
        if (!m_event.ContainsKey(key) || !m_event[key].Contains(callBack))
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
        if (!m_event.ContainsKey(key) || m_event[key].Count <= 0)
        {
            return;
        }

        int count = m_event[key].Count;

        for (int i = 0; i < count; i++)
        {
            m_event[key][i].Invoke(arg);
        }
    }

    public async void DelayCallFrames(string key, Action callBack, int frame)
    {
        if (m_cancellationTokenSources.ContainsKey(key))
        {
            return;
        }

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        m_cancellationTokenSources.Add(key, cancellationTokenSource);

        await UniTask.DelayFrame(frame, cancellationToken: m_cancellationTokenSources[key].Token);

        if (cancellationTokenSource.IsCancellationRequested)
        {
            return;
        }

        callBack.Invoke();
    }

    public async void DelayCallSeconds(string key, Action callBack, float time)
    {
        if (m_cancellationTokenSources.ContainsKey(key))
        {
            return;
        }

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        m_cancellationTokenSources.Add(key, cancellationTokenSource);

        await UniTask.Delay(TimeSpan.FromSeconds(time), cancellationToken: m_cancellationTokenSources[key].Token);

        if (cancellationTokenSource.IsCancellationRequested)
        {
            return;
        }

        callBack.Invoke();
    }

    public async void RepeatingCallFrames(string key, Action callBack, int frame = 1)
    {
        if (m_cancellationTokenSources.ContainsKey(key))
        {
            return;
        }

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        m_cancellationTokenSources.Add(key, cancellationTokenSource);

        var list = UniTaskAsyncEnumerable.EveryUpdate().WithCancellation(m_cancellationTokenSources[key].Token);
        int frames = frame;

        await foreach (AsyncUnit _ in list)
        {
            frames++;
            if (frames >= frame)
            {
                frames = 0;
                callBack.Invoke();
            }
        }
    }

    public async void RepeatingCallSeconds(string key, Action callBack, float time = 1)
    {
        if (m_cancellationTokenSources.ContainsKey(key))
        {
            return;
        }

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        m_cancellationTokenSources.Add(key, cancellationTokenSource);

        var list = UniTaskAsyncEnumerable.EveryUpdate().WithCancellation(m_cancellationTokenSources[key].Token);
        float times = time;

        await foreach (AsyncUnit _ in list)
        {
            times += Time.deltaTime;
            if (times >= time)
            {
                times = 0;
                callBack.Invoke();
            }
        }
    }

    public void CancelInvokeByKey(string key)
    {
        if (!m_cancellationTokenSources.ContainsKey(key))
        {
            return;
        }

        m_cancellationTokenSources[key].Cancel();
        m_cancellationTokenSources[key].Dispose();
        m_cancellationTokenSources.Remove(key);
        Debug.Log(key + "取消调用");
    }
}