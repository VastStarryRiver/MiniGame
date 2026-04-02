using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;



namespace Invariable
{
    public class UIDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public ScrollRect m_scroll = null;
        public int m_layer = 0;

        private RectTransform m_parent = null;
        private Action<int, Vector2> m_dragFunc = null;



        private void Awake()
        {
            m_parent = transform.parent?.GetComponent<RectTransform>();
        }



        public void OnBeginDrag(PointerEventData eventData)
        {
            m_scroll?.OnBeginDrag(eventData);

            if (m_parent != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(m_parent, eventData.position, Utils.UICamera[m_layer], out Vector2 pos);
                m_dragFunc?.Invoke(1, pos);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            m_scroll?.OnDrag(eventData);

            if (m_parent != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(m_parent, eventData.position, Utils.UICamera[m_layer], out Vector2 pos);
                m_dragFunc?.Invoke(2, pos);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            m_scroll?.OnEndDrag(eventData);

            if (m_parent != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(m_parent, eventData.position, Utils.UICamera[m_layer], out Vector2 pos);
                m_dragFunc?.Invoke(3, pos);
            }
        }

        public void AddDragListener(Action<int, Vector2> Action)
        {
            m_dragFunc = Action;
        }

        public void ReleaseDragListener()
        {
            m_dragFunc = null;
        }
    }
}