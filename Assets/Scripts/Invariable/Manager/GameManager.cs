using System;
using System.Collections.Generic;
using UnityEngine;



namespace Invariable
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager m_instance = null;

        private Dictionary<string, List<Delegate>> m_event = null;
        private Dictionary<string, TimerNode> m_timerMap = null;
        private List<TimerNode> m_secondHeap = null;
        private List<TimerNode> m_frameHeap = null;

        /// <summary>
        /// 实例是否存在（判空检查用，不触发错误日志）
        /// </summary>
        public static bool HasInstance
        {
            get
            {
                return m_instance != null;
            }
        }

        public static GameManager Instance
        {
            get
            {
                if (!HasInstance)
                {
                    GameLog.Error("GameManager实例对象为空");
                }

                return m_instance;
            }
        }

        private class TimerNode
        {
            public string Key;
            public Action CallBack;
            public bool IsRepeating;
            public bool IsFrameBased;
            public float IntervalSeconds;
            public int IntervalFrames;
            public float NextDueTime;
            public int NextDueFrame;
            public bool IsAlive;
        }



        private void Awake()
        {
            m_event = new Dictionary<string, List<Delegate>>();
            m_timerMap = new Dictionary<string, TimerNode>();
            m_secondHeap = new List<TimerNode>();
            m_frameHeap = new List<TimerNode>();
            m_instance = this;
            RepeatingCallSeconds(InvariableConst.Timer_Config_TickEvict, ConfigManagerCore.TickEvict, ConfigFormat.EvictScanIntervalSeconds);
        }

        private void Update()
        {
            TickSecondTimers();
            TickFrameTimers();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                CloudManager.Instance.FlushCloudData();
            }
        }

        private void OnApplicationQuit()
        {
            CloudManager.Instance.FlushCloudData();
        }

        private void OnDestroy()
        {
            CloudManager.Instance.FlushCloudData();

            if (m_timerMap != null && m_timerMap.Count > 0)
            {
                List<string> timerKeys = new List<string>(m_timerMap.Keys);

                for (int i = 0; i < timerKeys.Count; i++)
                {
                    CancelInvokeByKey(timerKeys[i]);
                }
            }

            m_secondHeap?.Clear();
            m_frameHeap?.Clear();

            if (m_instance == this)
            {
                m_instance = null;
            }
        }



        /// <summary>
        /// 添加泛型事件监听
        /// </summary>
        public void AddEventListener<T>(string key, Action<T> callBack)
        {
            if (!m_event.ContainsKey(key))
            {
                m_event.Add(key, new List<Delegate>());
            }

            if (!m_event[key].Contains(callBack))
            {
                m_event[key].Add(callBack);
            }
        }

        /// <summary>
        /// 移除泛型事件监听
        /// </summary>
        public void RemoveEventListener<T>(string key, Action<T> callBack)
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

        /// <summary>
        /// 触发泛型事件回调
        /// </summary>
        public void InvokeEventCallBack<T>(string key, T arg)
        {
            if (!m_event.TryGetValue(key, out List<Delegate> list) || list.Count <= 0)
            {
                return;
            }

            List<Delegate> snapshot = PoolUtils.Get<List<Delegate>>();

            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    snapshot.Add(list[i]);
                }

                for (int i = 0; i < snapshot.Count; i++)
                {
                    try
                    {
                        ((Action<T>)snapshot[i]).Invoke(arg);
                    }
                    catch (Exception error)
                    {
                        GameLog.Error($"Event callback error [{key}]: {error}");
                    }
                }
            }
            finally
            {
                PoolUtils.Release(snapshot);
            }
        }

        /// <summary>
        /// 延迟指定帧后执行一次回调
        /// </summary>
        public void DelayCallFrames(string key, Action callBack, int frame)
        {
            if (HasInvokeKey(key) || callBack == null)
            {
                return;
            }

            if (frame < 0)
            {
                frame = 0;
            }

            TimerNode node = new TimerNode
            {
                Key = key,
                CallBack = callBack,
                IsRepeating = false,
                IsFrameBased = true,
                IntervalFrames = frame,
                NextDueFrame = Time.frameCount + frame,
                IsAlive = true,
            };

            m_timerMap.Add(key, node);
            HeapPushFrame(node);
        }

        /// <summary>
        /// 延迟指定秒后执行一次回调
        /// </summary>
        public void DelayCallSeconds(string key, Action callBack, float time)
        {
            if (HasInvokeKey(key) || callBack == null)
            {
                return;
            }

            if (time < 0f)
            {
                time = 0f;
            }

            TimerNode node = new TimerNode
            {
                Key = key,
                CallBack = callBack,
                IsRepeating = false,
                IsFrameBased = false,
                IntervalSeconds = time,
                NextDueTime = Time.time + time,
                IsAlive = true,
            };

            m_timerMap.Add(key, node);
            HeapPushSecond(node);
        }

        /// <summary>
        /// 按帧间隔循环执行回调
        /// </summary>
        /// <param name="immediately">true 时注册后立即执行一次，再按间隔循环</param>
        public void RepeatingCallFrames(string key, Action callBack, int frame = 1, bool immediately = true)
        {
            if (HasInvokeKey(key) || callBack == null)
            {
                return;
            }

            if (frame < 1)
            {
                frame = 1;
            }

            TimerNode node = new TimerNode
            {
                Key = key,
                CallBack = callBack,
                IsRepeating = true,
                IsFrameBased = true,
                IntervalFrames = frame,
                IsAlive = true,
            };

            if (immediately)
            {
                callBack.Invoke();

                if (!node.IsAlive)
                {
                    return;
                }
            }

            node.NextDueFrame = Time.frameCount + frame;
            m_timerMap.Add(key, node);
            HeapPushFrame(node);
        }

        /// <summary>
        /// 按秒间隔循环执行回调
        /// </summary>
        /// <param name="immediately">true 时注册后立即执行一次，再按间隔循环</param>
        public void RepeatingCallSeconds(string key, Action callBack, float time = 1f, bool immediately = true)
        {
            if (HasInvokeKey(key) || callBack == null)
            {
                return;
            }

            if (time < 0f)
            {
                time = 0f;
            }

            TimerNode node = new TimerNode
            {
                Key = key,
                CallBack = callBack,
                IsRepeating = true,
                IsFrameBased = false,
                IntervalSeconds = time,
                IsAlive = true,
            };

            if (immediately)
            {
                callBack.Invoke();

                if (!node.IsAlive)
                {
                    return;
                }
            }

            node.NextDueTime = Time.time + time;
            m_timerMap.Add(key, node);
            HeapPushSecond(node);
        }

        /// <summary>
        /// 按键取消延迟或循环调用
        /// </summary>
        public void CancelInvokeByKey(string key)
        {
            if (m_timerMap.TryGetValue(key, out TimerNode node))
            {
                node.IsAlive = false;
                m_timerMap.Remove(key);
                GameLog.Info(key + "取消调用");
            }
        }

        /// <summary>
        /// 判断延迟/循环调用键是否仍存在
        /// </summary>
        private bool HasInvokeKey(string key)
        {
            return m_timerMap.ContainsKey(key);
        }

        /// <summary>
        /// 驱动秒级计时器堆
        /// </summary>
        private void TickSecondTimers()
        {
            float now = Time.time;

            while (m_secondHeap.Count > 0)
            {
                TimerNode node = m_secondHeap[0];

                if (!node.IsAlive)
                {
                    HeapPopSecond();

                    continue;
                }

                if (node.NextDueTime > now)
                {
                    break;
                }

                HeapPopSecond();

                if (!node.IsRepeating)
                {
                    m_timerMap.Remove(node.Key);
                }

                node.CallBack.Invoke();

                if (!node.IsAlive || !node.IsRepeating || !m_timerMap.ContainsKey(node.Key))
                {
                    continue;
                }

                node.NextDueTime = Time.time + node.IntervalSeconds;
                HeapPushSecond(node);
            }
        }

        /// <summary>
        /// 驱动帧级计时器堆
        /// </summary>
        private void TickFrameTimers()
        {
            int frame = Time.frameCount;

            while (m_frameHeap.Count > 0)
            {
                TimerNode node = m_frameHeap[0];

                if (!node.IsAlive)
                {
                    HeapPopFrame();

                    continue;
                }

                if (node.NextDueFrame > frame)
                {
                    break;
                }

                HeapPopFrame();

                if (!node.IsRepeating)
                {
                    m_timerMap.Remove(node.Key);
                }

                node.CallBack.Invoke();

                if (!node.IsAlive || !node.IsRepeating || !m_timerMap.ContainsKey(node.Key))
                {
                    continue;
                }

                node.NextDueFrame = Time.frameCount + node.IntervalFrames;
                HeapPushFrame(node);
            }
        }

        /// <summary>
        /// 将秒级计时节点压入最小堆
        /// </summary>
        private void HeapPushSecond(TimerNode node)
        {
            m_secondHeap.Add(node);
            int index = m_secondHeap.Count - 1;

            while (index > 0)
            {
                int parent = (index - 1) / 2;

                if (m_secondHeap[parent].NextDueTime <= m_secondHeap[index].NextDueTime)
                {
                    break;
                }

                TimerNode temp = m_secondHeap[parent];
                m_secondHeap[parent] = m_secondHeap[index];
                m_secondHeap[index] = temp;
                index = parent;
            }
        }

        /// <summary>
        /// 弹出秒级计时最小堆堆顶
        /// </summary>
        private TimerNode HeapPopSecond()
        {
            TimerNode root = m_secondHeap[0];
            int last = m_secondHeap.Count - 1;
            m_secondHeap[0] = m_secondHeap[last];
            m_secondHeap.RemoveAt(last);

            if (m_secondHeap.Count <= 0)
            {
                return root;
            }

            int index = 0;

            while (true)
            {
                int left = index * 2 + 1;
                int right = left + 1;
                int smallest = index;

                if (left < m_secondHeap.Count && m_secondHeap[left].NextDueTime < m_secondHeap[smallest].NextDueTime)
                {
                    smallest = left;
                }

                if (right < m_secondHeap.Count && m_secondHeap[right].NextDueTime < m_secondHeap[smallest].NextDueTime)
                {
                    smallest = right;
                }

                if (smallest == index)
                {
                    break;
                }

                TimerNode temp = m_secondHeap[index];
                m_secondHeap[index] = m_secondHeap[smallest];
                m_secondHeap[smallest] = temp;
                index = smallest;
            }

            return root;
        }

        /// <summary>
        /// 将帧级计时节点压入最小堆
        /// </summary>
        private void HeapPushFrame(TimerNode node)
        {
            m_frameHeap.Add(node);
            int index = m_frameHeap.Count - 1;

            while (index > 0)
            {
                int parent = (index - 1) / 2;

                if (m_frameHeap[parent].NextDueFrame <= m_frameHeap[index].NextDueFrame)
                {
                    break;
                }

                TimerNode temp = m_frameHeap[parent];
                m_frameHeap[parent] = m_frameHeap[index];
                m_frameHeap[index] = temp;
                index = parent;
            }
        }

        /// <summary>
        /// 弹出帧级计时最小堆堆顶
        /// </summary>
        private TimerNode HeapPopFrame()
        {
            TimerNode root = m_frameHeap[0];
            int last = m_frameHeap.Count - 1;
            m_frameHeap[0] = m_frameHeap[last];
            m_frameHeap.RemoveAt(last);

            if (m_frameHeap.Count <= 0)
            {
                return root;
            }

            int index = 0;

            while (true)
            {
                int left = index * 2 + 1;
                int right = left + 1;
                int smallest = index;

                if (left < m_frameHeap.Count && m_frameHeap[left].NextDueFrame < m_frameHeap[smallest].NextDueFrame)
                {
                    smallest = left;
                }

                if (right < m_frameHeap.Count && m_frameHeap[right].NextDueFrame < m_frameHeap[smallest].NextDueFrame)
                {
                    smallest = right;
                }

                if (smallest == index)
                {
                    break;
                }

                TimerNode temp = m_frameHeap[index];
                m_frameHeap[index] = m_frameHeap[smallest];
                m_frameHeap[smallest] = temp;
                index = smallest;
            }

            return root;
        }
    }
}