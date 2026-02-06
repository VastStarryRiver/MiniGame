using UnityEngine;
using UnityEngine.UI;
using System;



namespace Invariable
{
    public class LoopScrollList : ScrollRect
    {
        private int m_type = 0;
        private int m_totalCount = 0;
        private Action<int, RectTransform> m_updateFunc;
        private Action<Vector2> m_onValueChangedFunc;
        private float m_lastOffset;



        protected override void Awake()
        {
            base.Awake();

            content = transform.Find("Viewport/Content") as RectTransform;
            viewport = transform.Find("Viewport") as RectTransform;

            onValueChanged.AddListener((pos) =>
            {
                if (m_type == 1)
                {
                    if (content.sizeDelta.x <= viewport.sizeDelta.x)
                    {
                        return;
                    }

                    UpdateHorizonalItem(pos);
                }
                else if (m_type == 2)
                {
                    if (content.sizeDelta.y <= viewport.sizeDelta.y)
                    {
                        return;
                    }

                    UpdateVerticalItem(pos);
                }
                else
                {
                    return;
                }

                m_onValueChangedFunc?.Invoke(pos);
            });
        }



        public void AddOnValueChangedListener(Action<Vector2> Action)
        {
            m_onValueChangedFunc = Action;
        }

        /// <summary>
        /// 初始化循环列表
        /// </summary>
        /// <param name="type">1横向 2纵向</param>
        /// <param name="showCount">需要比可见区域的元素多两个</param>
        /// <param name="totalCount">总元素个数</param>
        /// <param name="updateFunc">刷新函数</param>
        /// <param name="callBack">回调函数</param>
        public void Init(RectTransform tsCell, int type, int showCount, int totalCount, Action<int, RectTransform> updateFunc, Action callBack = null)
        {
            m_type = type;
            m_updateFunc = updateFunc;
            m_totalCount = totalCount;

            Utils.HideAllChildren(content);

            for (int i = 0; i < showCount; i++)
            {
                RectTransform tsItem = null;

                if (i > content.childCount - 1)
                {
                    tsItem = Instantiate(tsCell.gameObject, content).transform as RectTransform;
                }
                else
                {
                    tsItem = content.GetChild(i) as RectTransform;
                }

                tsItem.name = "Ts_Item" + i;

                if (m_type == 1)
                {
                    tsItem.anchoredPosition = new Vector2(i * tsItem.sizeDelta.x, 0);
                }
                else if (m_type == 2)
                {
                    tsItem.anchoredPosition = new Vector2(0, i * -tsItem.sizeDelta.y);
                }

                tsItem.gameObject.SetActive(true);

                m_updateFunc.Invoke(i, tsItem);
            }

            if (m_type == 1)
            {
                content.sizeDelta = new Vector2(tsCell.sizeDelta.x * m_totalCount, content.sizeDelta.y);
            }
            else if (m_type == 2)
            {
                content.sizeDelta = new Vector2(content.sizeDelta.x, tsCell.sizeDelta.y * m_totalCount);
            }

            content.anchoredPosition = Vector2.zero;

            callBack?.Invoke();
        }

        private void UpdateHorizonalItem(Vector2 pos)
        {
            RectTransform tsCell = null;
            RectTransform tsCell1 = content.GetChild(0) as RectTransform;
            RectTransform tsCell2 = content.GetChild(content.childCount - 1) as RectTransform;

            int id = -1;

            if (pos.x - m_lastOffset > 0)
            {
                if (content.anchoredPosition.x + tsCell1.anchoredPosition.x + tsCell1.sizeDelta.x + 0.1f <= 0)
                {
                    id = int.Parse(tsCell2.name.Replace("Ts_Item", "")) + 1;

                    if (id >= 0 && id < m_totalCount)
                    {
                        tsCell1.anchoredPosition = new Vector2(tsCell2.anchoredPosition.x + tsCell1.sizeDelta.x, 0);
                        tsCell1.SetAsLastSibling();
                        tsCell = tsCell1;
                    }
                }
            }
            else
            {
                if (tsCell2.anchoredPosition.x - 0.1f >= -content.anchoredPosition.x + viewport.rect.width)
                {
                    id = int.Parse(tsCell1.name.Replace("Ts_Item", "")) - 1;

                    if (id >= 0 && id < m_totalCount)
                    {
                        tsCell2.anchoredPosition = new Vector2(tsCell1.anchoredPosition.x - tsCell2.sizeDelta.x, 0);
                        tsCell2.SetAsFirstSibling();
                        tsCell = tsCell2;
                    }
                }
            }

            if (tsCell != null)
            {
                tsCell.name = "Ts_Item" + id;
                m_updateFunc.Invoke(id, tsCell);
            }

            m_lastOffset = pos.x;
        }

        private void UpdateVerticalItem(Vector2 pos)
        {
            RectTransform tsCell = null;
            RectTransform tsCell1 = content.GetChild(0) as RectTransform;
            RectTransform tsCell2 = content.GetChild(content.childCount - 1) as RectTransform;

            int id = -1;

            if (pos.y - m_lastOffset < 0)
            {
                if (-tsCell1.anchoredPosition.y + tsCell1.sizeDelta.y + 0.1f <= content.anchoredPosition.y)
                {
                    id = int.Parse(tsCell2.name.Replace("Ts_Item", "")) + 1;

                    if (id >= 0 && id < m_totalCount)
                    {
                        tsCell1.anchoredPosition = new Vector2(0, tsCell2.anchoredPosition.y - tsCell1.sizeDelta.y);
                        tsCell1.SetAsLastSibling();
                        tsCell = tsCell1;
                    }
                }
            }
            else
            {
                if (-tsCell2.anchoredPosition.y - 0.1f >= viewport.rect.height + content.anchoredPosition.y)
                {
                    id = int.Parse(tsCell1.name.Replace("Ts_Item", "")) - 1;

                    if (id >= 0 && id < m_totalCount)
                    {
                        tsCell2.anchoredPosition = new Vector2(0, tsCell1.anchoredPosition.y + tsCell2.sizeDelta.y);
                        tsCell2.SetAsFirstSibling();
                        tsCell = tsCell2;
                    }
                }
            }

            if (tsCell != null)
            {
                tsCell.name = "Ts_Item" + id;
                m_updateFunc.Invoke(id, tsCell);
            }

            m_lastOffset = pos.y;
        }
    }
}