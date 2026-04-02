using System;
using UnityEngine;



namespace Invariable
{
    public class Rocker : MonoBehaviour
    {
        public RectTransform parent = null;
        public RectTransform handle = null;

        private bool isSetCurrPos = false;

        private Action m_upFunc = null;
        private Action m_downFunc = null;
        private Action m_leftFunc = null;
        private Action m_rightFunc = null;
        private Action m_moveFunc = null;
        private Action m_stayFunc = null;

        private void Awake()
        {
            parent = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (handle == null || handle.parent != transform)
            {
                return;
            }
            else if (!Input.GetMouseButton(0) && Input.touchCount <= 0)
            {
                gameObject.SetActive(false);
                isSetCurrPos = false;
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

            if (!isSetCurrPos)
            {
                isSetCurrPos = true;
                parent.anchoredPosition = point;
                gameObject.SetActive(true);
            }

            Vector2 moveDir = point - parent.anchoredPosition;

            if (moveDir.x > 0)
            {
                m_rightFunc?.Invoke();
            }

            if (moveDir.x < 0)
            {
                m_leftFunc?.Invoke();
            }

            if (moveDir.y > 0)
            {
                m_upFunc?.Invoke();
            }

            if (moveDir.y < 0)
            {
                m_downFunc?.Invoke();
            }

            if (moveDir == Vector2.zero)
            {
                m_stayFunc?.Invoke();
            }
            else
            {
                m_moveFunc?.Invoke();
            }
        }

        public void SetUpMoveFunc(Action func)
        {
            m_upFunc = func;
        }

        public void SetDownMoveFunc(Action func)
        {
            m_downFunc = func;
        }

        public void SetLeftMoveFunc(Action func)
        {
            m_leftFunc = func;
        }

        public void SetRightMoveFunc(Action func)
        {
            m_rightFunc = func;
        }

        public void SetMoveFunc(Action func)
        {
            m_moveFunc = func;
        }

        public void SetStayFunc(Action func)
        {
            m_stayFunc = func;
        }
    }
}