using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



namespace Invariable
{
    public class LoopScrollList : ScrollRect
    {
        private int m_type;
        private int m_totalCount;
        private Action<int, RectTransform> m_updateFunc = null;
        private Action<Vector2> m_onValueChangedFunc = null;
        private Dictionary<RectTransform, LoopScrollItem> m_itemCache = null;
        private float m_lastOffset;



        protected override void Start()
        {
            base.Start();

            onValueChanged.AddListener((pos) =>
            {
                if (m_type == 1)
                {
                    if (content.sizeDelta.x <= viewport.rect.width)
                    {
                        return;
                    }

                    UpdateHorizonalItem(pos);
                }
                else if (m_type == 2)
                {
                    if (content.sizeDelta.y <= viewport.rect.height)
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



        /// <summary>
        /// 添加滚动值变化回调
        /// </summary>
        public void AddOnValueChangedListener(Action<Vector2> callBack)
        {
            m_onValueChangedFunc = callBack;
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

                SetItemIndex(tsItem, i);

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

        /// <summary>
        /// 刷新当前可见的全部列表项
        /// </summary>
        public void RefreshAllItem()
        {
            for (int i = 0; i < content.childCount; i++)
            {
                RectTransform tsItem = content.GetChild(i) as RectTransform;
                LoopScrollItem item = GetCachedItem(tsItem);

                if (item == null)
                {
                    continue;
                }

                m_updateFunc.Invoke(item.m_index, tsItem);
            }
        }

        /// <summary>
        /// 横向滚动时回收并复用列表项
        /// </summary>
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
                    id = GetItemIndex(tsCell2) + 1;

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
                    id = GetItemIndex(tsCell1) - 1;

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
                SetItemIndex(tsCell, id);
                m_updateFunc.Invoke(id, tsCell);
            }

            m_lastOffset = pos.x;
        }

        /// <summary>
        /// 纵向滚动时回收并复用列表项
        /// </summary>
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
                    id = GetItemIndex(tsCell2) + 1;

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
                    id = GetItemIndex(tsCell1) - 1;

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
                SetItemIndex(tsCell, id);
                m_updateFunc.Invoke(id, tsCell);
            }

            m_lastOffset = pos.y;
        }

        /// <summary>
        /// 读取列表项索引
        /// </summary>
        private int GetItemIndex(RectTransform tsItem)
        {
            LoopScrollItem item = GetCachedItem(tsItem);

            if (item == null)
            {
                return -1;
            }

            return item.m_index;
        }

        /// <summary>
        /// 写入列表项索引
        /// </summary>
        private void SetItemIndex(RectTransform tsItem, int index)
        {
            LoopScrollItem item = GetOrAddItem(tsItem);
            item.m_index = index;
            tsItem.name = "Ts_Item" + index;
        }

        /// <summary>
        /// 读取已缓存的列表项组件
        /// </summary>
        private LoopScrollItem GetCachedItem(RectTransform tsItem)
        {
            if (tsItem == null)
            {
                return null;
            }

            m_itemCache ??= new Dictionary<RectTransform, LoopScrollItem>();

            if (m_itemCache.TryGetValue(tsItem, out LoopScrollItem item) && item != null)
            {
                return item;
            }

            item = tsItem.GetComponent<LoopScrollItem>();

            if (item != null)
            {
                m_itemCache[tsItem] = item;
            }

            return item;
        }

        /// <summary>
        /// 获取或挂载列表项组件并缓存
        /// </summary>
        private LoopScrollItem GetOrAddItem(RectTransform tsItem)
        {
            LoopScrollItem item = GetCachedItem(tsItem);

            if (item != null)
            {
                return item;
            }

            item = tsItem.gameObject.AddComponent<LoopScrollItem>();
            m_itemCache[tsItem] = item;

            return item;
        }
    }
}