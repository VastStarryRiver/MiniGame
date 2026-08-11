using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



namespace Invariable
{
    public class UIDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public ScrollRect m_scrDrag;
        public int m_layer = 0;

        private RectTransform m_parent = null;
        private Action<int, Vector2> m_dragFunc = null;



        private void Awake()
        {
            m_parent = transform.parent?.GetComponent<RectTransform>();
        }



        public void OnBeginDrag(PointerEventData eventData)
        {
            m_scrDrag?.OnBeginDrag(eventData);

            if (m_parent != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(m_parent, eventData.position, Utils.UICamera[m_layer], out Vector2 pos);
                m_dragFunc?.Invoke(1, pos); // 阶段：1=开始拖拽
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            m_scrDrag?.OnDrag(eventData);

            if (m_parent != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(m_parent, eventData.position, Utils.UICamera[m_layer], out Vector2 pos);
                m_dragFunc?.Invoke(2, pos); // 阶段：2=拖拽中
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            m_scrDrag?.OnEndDrag(eventData);

            if (m_parent != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(m_parent, eventData.position, Utils.UICamera[m_layer], out Vector2 pos);
                m_dragFunc?.Invoke(3, pos); // 阶段：3=结束拖拽
            }
        }



        /// <summary>
        /// 添加拖拽回调（阶段参数：1=开始拖拽，2=拖拽中，3=结束拖拽）
        /// </summary>
        public void AddDragListener(Action<int, Vector2> callBack)
        {
            m_dragFunc = callBack;
        }

        /// <summary>
        /// 移除拖拽回调
        /// </summary>
        public void ReleaseDragListener()
        {
            m_dragFunc = null;
        }
    }
}