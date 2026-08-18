using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;



namespace Invariable
{
    public class UIButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public bool m_isNotChangeScale = false;
        public float m_changeScale = 1.1f;
        public UnityEvent m_clickEvent = new UnityEvent();
        public UnityEvent m_doubleClickEvent = new UnityEvent();
        public UnityEvent m_downEvent = new UnityEvent();
        public UnityEvent m_upEvent = new UnityEvent();
        public UnityEvent m_longPressEvent = new UnityEvent();

        private static readonly List<UIButton> ActiveButtons = new List<UIButton>();

        private static UIButtonDriver m_driver = null;

        private bool m_hasClickListener;
        private bool m_hasDoubleClickListener;
        private bool m_hasLongPressListener;
        private int m_clickTimes;
        private bool m_isCancelClick;
        private float m_startPressTime;
        private float m_endPressTime;
        private float m_startClickTime;
        private float m_endClickTime;
        private PointerEventData m_eventData = null;
        private RectTransform m_trans = null;
        private bool m_isActiveTracked;



        private void Awake()
        {
            m_trans = gameObject.GetComponent<RectTransform>();
        }

        private void OnDestroy()
        {
            UnregisterActive();
            m_eventData = null;
            m_startPressTime = 0;
            m_endPressTime = 0;
            m_startClickTime = 0;
            m_endClickTime = 0;
            m_clickTimes = 0;
        }



        public void OnPointerClick(PointerEventData eventData)
        {
            if (m_isCancelClick)
            {
                m_isCancelClick = false;
                m_eventData = null;
            }
            else
            {
                if (m_hasDoubleClickListener || m_doubleClickEvent.GetPersistentEventCount() > 0)
                {
                    m_clickTimes++;
                    m_eventData = eventData;
                    RegisterActive();
                }
                else if (m_hasClickListener || m_clickEvent.GetPersistentEventCount() > 0)
                {
                    m_clickEvent.Invoke();
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!m_isNotChangeScale)
            {
                m_trans.localScale = new Vector3(m_changeScale, m_changeScale, m_changeScale);
            }

            if (m_hasLongPressListener || m_longPressEvent.GetPersistentEventCount() > 0)
            {
                m_startPressTime = Time.time;
                RegisterActive();
            }

            m_downEvent.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!m_isNotChangeScale)
            {
                m_trans.localScale = new Vector3(1, 1, 1);
            }

            if (m_hasLongPressListener || m_longPressEvent.GetPersistentEventCount() > 0)
            {
                m_startPressTime = 0;
                m_endPressTime = 0;
            }

            RefreshActiveState();
            m_upEvent.Invoke();
        }



        /// <summary>
        /// 添加单击回调
        /// </summary>
        public void AddClickListener(Action callBack)
        {
            ReleaseClickListener();

            if (callBack == null)
            {
                return;
            }

            m_hasClickListener = true;
            m_clickEvent.AddListener(callBack.Invoke);
        }

        /// <summary>
        /// 移除单击回调
        /// </summary>
        public void ReleaseClickListener()
        {
            m_clickEvent.RemoveAllListeners();
            m_hasClickListener = false;
        }

        /// <summary>
        /// 添加双击回调
        /// </summary>
        public void AddDoubleClickListener(Action callBack)
        {
            ReleaseDoubleClickListener();

            if (callBack == null)
            {
                return;
            }

            m_hasDoubleClickListener = true;
            m_doubleClickEvent.AddListener(callBack.Invoke);
        }

        /// <summary>
        /// 移除双击回调
        /// </summary>
        public void ReleaseDoubleClickListener()
        {
            m_doubleClickEvent.RemoveAllListeners();
            m_hasDoubleClickListener = false;
            RefreshActiveState();
        }

        /// <summary>
        /// 添加按下回调
        /// </summary>
        public void AddDownListener(Action callBack)
        {
            ReleaseDownListener();

            if (callBack == null)
            {
                return;
            }

            m_downEvent.AddListener(callBack.Invoke);
        }

        /// <summary>
        /// 移除按下回调
        /// </summary>
        public void ReleaseDownListener()
        {
            m_downEvent.RemoveAllListeners();
        }

        /// <summary>
        /// 添加抬起回调
        /// </summary>
        public void AddUpListener(Action callBack)
        {
            ReleaseUpListener();

            if (callBack == null)
            {
                return;
            }

            m_upEvent.AddListener(callBack.Invoke);
        }

        /// <summary>
        /// 移除抬起回调
        /// </summary>
        public void ReleaseUpListener()
        {
            m_upEvent.RemoveAllListeners();
        }

        /// <summary>
        /// 添加长按回调
        /// </summary>
        public void AddLongPressListener(Action callBack)
        {
            ReleaseLongPressListener();

            if (callBack == null)
            {
                return;
            }

            m_hasLongPressListener = true;
            m_longPressEvent.AddListener(callBack.Invoke);
        }

        /// <summary>
        /// 移除长按回调
        /// </summary>
        public void ReleaseLongPressListener()
        {
            m_longPressEvent.RemoveAllListeners();
            m_hasLongPressListener = false;
            RefreshActiveState();
        }



        /// <summary>
        /// 驱动活跃按钮的长按/双击判定
        /// </summary>
        internal static void TickActiveButtons()
        {
            for (int i = ActiveButtons.Count - 1; i >= 0; i--)
            {
                UIButton button = ActiveButtons[i];

                if (button == null)
                {
                    ActiveButtons.RemoveAt(i);

                    continue;
                }

                button.Tick();
            }
        }

        /// <summary>
        /// 单帧推进长按与双击判定
        /// </summary>
        private void Tick()
        {
            CallLongPressListener();
            CallDoubleClickListener();
            RefreshActiveState();
        }

        /// <summary>
        /// 检测并触发双击
        /// </summary>
        private void CallDoubleClickListener()
        {
            if (m_eventData != null)
            {
                if (m_startClickTime == 0)
                {
                    m_startClickTime = Time.time;
                }

                m_endClickTime = Time.time;

                if (m_endClickTime - m_startClickTime >= 0.15f)
                {
                    if (m_clickTimes == 1)
                    {
                        m_clickTimes = 0;
                        m_clickEvent.Invoke();
                    }
                    else if (m_clickTimes >= 2)
                    {
                        m_clickTimes = 0;
                        m_doubleClickEvent.Invoke();
                    }

                    m_eventData = null;
                    m_startClickTime = 0;
                    m_endClickTime = 0;
                }
            }
        }

        /// <summary>
        /// 检测并触发长按
        /// </summary>
        private void CallLongPressListener()
        {
            if (m_startPressTime != 0)
            {
                if (m_isCancelClick)
                {
                    m_startPressTime = 0;
                    m_endPressTime = 0;

                    m_isCancelClick = false;
                }
                else
                {
                    m_endPressTime = Time.time;

                    if (m_endPressTime - m_startPressTime >= 0.2f)
                    {
                        m_startPressTime = 0;
                        m_endPressTime = 0;

                        m_isCancelClick = true;

                        m_longPressEvent.Invoke();
                    }
                }
            }
        }

        /// <summary>
        /// 将按钮加入活跃驱动列表
        /// </summary>
        private void RegisterActive()
        {
            EnsureDriver();

            if (m_isActiveTracked)
            {
                return;
            }

            ActiveButtons.Add(this);
            m_isActiveTracked = true;
        }

        /// <summary>
        /// 将按钮移出活跃驱动列表
        /// </summary>
        private void UnregisterActive()
        {
            if (!m_isActiveTracked)
            {
                return;
            }

            ActiveButtons.Remove(this);
            m_isActiveTracked = false;
        }

        /// <summary>
        /// 按当前状态刷新是否需要被驱动
        /// </summary>
        private void RefreshActiveState()
        {
            bool needTrack = m_startPressTime != 0 || m_eventData != null;

            if (needTrack)
            {
                RegisterActive();
            }
            else
            {
                UnregisterActive();
            }
        }

        /// <summary>
        /// 确保全局按钮驱动器存在
        /// </summary>
        private static void EnsureDriver()
        {
            if (m_driver != null)
            {
                return;
            }

            GameObject driverObject = new GameObject("UIButtonDriver");
            DontDestroyOnLoad(driverObject);
            m_driver = driverObject.AddComponent<UIButtonDriver>();
        }

        private class UIButtonDriver : MonoBehaviour
        {
            private void Update()
            {
                TickActiveButtons();
            }
        }
    }
}