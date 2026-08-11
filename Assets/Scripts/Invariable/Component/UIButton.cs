using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;



namespace Invariable
{
    public class UIButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public bool m_isNotChangeScale = false;
        public float m_changeScale = 1.1f;

        private static readonly List<UIButton> m_activeButtons = new List<UIButton>();
        private static UIButtonDriver m_driver = null;

        private int m_clickTimes;
        private bool m_isCancelClick;
        private float m_startPressTime;
        private float m_endPressTime;
        private float m_startClickTime;
        private float m_endClickTime;
        private Action m_clickFunc = null;
        private Action m_doubleClickFunc = null;
        private Action m_downFunc = null;
        private Action m_upFunc = null;
        private Action m_longPressFunc = null;
        private PointerEventData m_eventData = null;
        private RectTransform m_trans = null;
        private bool m_isActiveTracked;



        private void Awake()
        {
            m_trans = gameObject.GetComponent<RectTransform>();
        }

        private void OnDisable()
        {
            UnregisterActive();
            m_eventData = null;
            m_startPressTime = 0;
            m_endPressTime = 0;
            m_startClickTime = 0;
            m_endClickTime = 0;
            m_clickTimes = 0;
        }

        private void OnDestroy()
        {
            UnregisterActive();
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
                if (m_doubleClickFunc != null)
                {
                    m_clickTimes++;
                    m_eventData = eventData;
                    RegisterActive();
                }
                else if (m_clickFunc != null)
                {
                    m_clickFunc.Invoke();
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!m_isNotChangeScale)
            {
                m_trans.localScale = new Vector3(m_changeScale, m_changeScale, m_changeScale);
            }

            if (m_longPressFunc != null)
            {
                m_startPressTime = Time.time;
                RegisterActive();
            }

            m_downFunc?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!m_isNotChangeScale)
            {
                m_trans.localScale = new Vector3(1, 1, 1);
            }

            if (m_longPressFunc != null)
            {
                m_startPressTime = 0;
                m_endPressTime = 0;
            }

            RefreshActiveState();
            m_upFunc?.Invoke();
        }



        /// <summary>
        /// 添加单击回调
        /// </summary>
        public void AddClickListener(Action callBack)
        {
            m_clickFunc = callBack;
        }

        /// <summary>
        /// 移除单击回调
        /// </summary>
        public void ReleaseClickListener()
        {
            m_clickFunc = null;
        }

        /// <summary>
        /// 添加双击回调
        /// </summary>
        public void AddDoubleClickListener(Action callBack)
        {
            m_doubleClickFunc = callBack;
        }

        /// <summary>
        /// 移除双击回调
        /// </summary>
        public void ReleaseDoubleClickListener()
        {
            m_doubleClickFunc = null;
            RefreshActiveState();
        }

        /// <summary>
        /// 添加按下回调
        /// </summary>
        public void AddDownListener(Action callBack)
        {
            m_downFunc = callBack;
        }

        /// <summary>
        /// 移除按下回调
        /// </summary>
        public void ReleaseDownListener()
        {
            m_downFunc = null;
        }

        /// <summary>
        /// 添加抬起回调
        /// </summary>
        public void AddUpListener(Action callBack)
        {
            m_upFunc = callBack;
        }

        /// <summary>
        /// 移除抬起回调
        /// </summary>
        public void ReleaseUpListener()
        {
            m_upFunc = null;
        }

        /// <summary>
        /// 添加长按回调
        /// </summary>
        public void AddLongPressListener(Action callBack)
        {
            m_longPressFunc = callBack;
        }

        /// <summary>
        /// 移除长按回调
        /// </summary>
        public void ReleaseLongPressListener()
        {
            m_longPressFunc = null;
            RefreshActiveState();
        }



        /// <summary>
        /// 驱动活跃按钮的长按/双击判定
        /// </summary>
        internal static void TickActiveButtons()
        {
            for (int i = m_activeButtons.Count - 1; i >= 0; i--)
            {
                UIButton button = m_activeButtons[i];

                if (button == null)
                {
                    m_activeButtons.RemoveAt(i);

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
                        m_clickFunc?.Invoke();
                    }
                    else if (m_clickTimes >= 2)
                    {
                        m_clickTimes = 0;
                        m_doubleClickFunc?.Invoke();
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

                        m_longPressFunc?.Invoke();
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

            m_activeButtons.Add(this);
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

            m_activeButtons.Remove(this);
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