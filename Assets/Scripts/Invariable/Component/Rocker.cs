using System;
using UnityEngine;



namespace Invariable
{
    public class Rocker : MonoBehaviour
    {
        private RectTransform m_tsParent;
        public RectTransform m_tsHandle;

        private bool m_isSetCurrPos;
        private Action<Vector2> m_moveFunc = null;
        private Action m_stayFunc = null;
        private float m_radius = 0f;
        private Vector2 m_lastMoveValue;



        private void Awake()
        {
            m_tsParent = GetComponent<RectTransform>();
            m_radius = m_tsParent.rect.width * 0.5f;

            if (m_radius <= 0f)
            {
                m_radius = 100f;
            }
        }

        private void Update()
        {
            if (m_tsHandle == null || m_tsHandle.parent != transform)
            {
                return;
            }
            else if (!Input.GetMouseButton(0) && Input.touchCount <= 0)
            {
                ResetHandle();
                m_moveFunc?.Invoke(Vector2.zero);
                m_lastMoveValue = Vector2.zero;
                gameObject.SetActive(false);
                m_isSetCurrPos = false;

                return;
            }

            Vector2 point = Vector2.zero;

            if (Input.GetMouseButton(0))
            {
                point = Input.mousePosition;
            }
            else if (Input.touchCount > 0)
            {
                point = Input.GetTouch(0).position;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(Utils.UIRoot, point, Utils.UICamera[0], out point);

            if (!m_isSetCurrPos)
            {
                m_isSetCurrPos = true;
                m_tsParent.anchoredPosition = point;
                gameObject.SetActive(true);
            }

            Vector2 moveDir = point - m_tsParent.anchoredPosition;
            Vector2 clamped = Vector2.ClampMagnitude(moveDir, m_radius);
            m_tsHandle.anchoredPosition = clamped;

            Vector2 moveValue;

            if (clamped == Vector2.zero)
            {
                m_stayFunc?.Invoke();
                moveValue = Vector2.zero;
            }
            else
            {
                float strength = clamped.magnitude / m_radius;
                moveValue = clamped.normalized * strength;
            }

            if (moveValue != m_lastMoveValue)
            {
                m_lastMoveValue = moveValue;
                m_moveFunc?.Invoke(moveValue);
            }
        }

        private void OnDisable()
        {
            ResetHandle();
            m_isSetCurrPos = false;
            m_lastMoveValue = Vector2.zero;
        }



        /// <summary>
        /// 设置摇杆移动回调（方向归一化 × 力度 0~1）
        /// </summary>
        public void SetMoveFunc(Action<Vector2> callBack)
        {
            m_moveFunc = callBack;
        }

        /// <summary>
        /// 设置摇杆静止回调
        /// </summary>
        public void SetStayFunc(Action callBack)
        {
            m_stayFunc = callBack;
        }

        /// <summary>
        /// 手柄视觉回中（不触发移动回调）
        /// </summary>
        private void ResetHandle()
        {
            if (m_tsHandle != null)
            {
                m_tsHandle.anchoredPosition = Vector2.zero;
            }
        }
    }
}