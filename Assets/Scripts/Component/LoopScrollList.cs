using UnityEngine;
using UnityEngine.UI;
using System;

public class LoopScrollList : ScrollRect
{
    public RectTransform m_itemPrefab;
    private int m_type = 0;
    private int m_totalCount = 0;
    private Action<int,RectTransform> m_updateFunc;
    private Action<Vector2> m_onValueChangedFunc;
    private int m_index1 = -1;
    private int m_index2 = -1;



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
    public void Init(int type, int showCount, int totalCount, Action<int, RectTransform> updateFunc, Action callBack = null)
    {
        m_type = type;
        m_updateFunc = updateFunc;
        m_totalCount = totalCount;

        ConvenientUtility.HideAllChildren(content);

        for (int i = 0; i < showCount; i++)
        {
            Transform item = content.GetChild(i);

            if(item == null)
            {
                item = Instantiate(m_itemPrefab.gameObject, content).transform;
            }

            item.name = "Ts_Item" + i;

            m_updateFunc.Invoke(i, (RectTransform)item);

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)item);
        }

        if(m_type == 1)
        {
            HorizontalLayoutGroup horizontalLayoutGroup = content.GetComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.spacing = 0;
            content.sizeDelta = new Vector2(m_itemPrefab.sizeDelta.x * m_totalCount, content.sizeDelta.y);
        }
        else if (m_type == 2)
        {
            VerticalLayoutGroup verticalLayoutGroup = content.GetComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.spacing = 0;
            content.sizeDelta = new Vector2(content.sizeDelta.x, m_itemPrefab.sizeDelta.y * m_totalCount);
        }

        content.anchoredPosition = Vector2.zero;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        m_index1 = -1;
        m_index2 = -1;

        callBack?.Invoke();
    }

    private void UpdateHorizonalItem(Vector2 pos)
    {
        float offset = viewport.sizeDelta.x - content.sizeDelta.x;
        float min = offset * pos.x;
        float max = min + viewport.sizeDelta.x;

        for (int i = 1; i < m_totalCount - 1; i++)
        {
            RectTransform item1 = content.GetChild(i) as RectTransform;
            float min2 = item1.sizeDelta.x * i;
            float max2 = min2 + item1.sizeDelta.x;

            if (min2 >= min && min2 <= min + 0.1f)
            {
                if (m_index1 != i)
                {
                    m_index1 = i;

                    Transform item2 = content.GetChild(content.childCount - 1);

                    item2.SetSiblingIndex(0);

                    item2.name = "Ts_Item" + i;

                    m_updateFunc.Invoke(i, (RectTransform)item2);

                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)item2);
                }
            }
            else if (max2 <= max && max2 >= max - 0.1f)
            {
                if (m_index2 != i)
                {
                    m_index2 = i;

                    Transform item2 = content.GetChild(0);

                    item2.SetSiblingIndex(content.childCount - 1);

                    item2.name = "Ts_Item" + i;

                    m_updateFunc.Invoke(i, (RectTransform)item2);

                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)item2);
                }

                break;
            }
        }
    }

    private void UpdateVerticalItem(Vector2 pos)
    {
        float offset = viewport.sizeDelta.y - content.sizeDelta.y;
        float min = offset * pos.y;
        float max = min + viewport.sizeDelta.y;

        for (int i = 1; i < m_totalCount - 1; i++)
        {
            RectTransform item1 = content.GetChild(i) as RectTransform;
            float min2 = item1.sizeDelta.y * i;
            float max2 = min2 + item1.sizeDelta.y;

            if (min2 >= min && min2 <= min + 0.1f)
            {
                if (m_index1 != i)
                {
                    m_index1 = i;

                    Transform item2 = content.GetChild(content.childCount - 1);

                    item2.SetSiblingIndex(0);

                    item2.name = "Ts_Item" + i;

                    m_updateFunc.Invoke(i, (RectTransform)item2);

                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)item2);
                }
            }
            else if (max2 <= max && max2 >= max - 0.1f)
            {
                if (m_index2 != i)
                {
                    m_index2 = i;

                    Transform item2 = content.GetChild(0);

                    item2.SetSiblingIndex(content.childCount - 1);

                    item2.name = "Ts_Item" + i;

                    m_updateFunc.Invoke(i, (RectTransform)item2);

                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)item2);
                }

                break;
            }
        }
    }

    public void UpdateAllItem()
    {
        for (int i = 0; i < content.childCount; i++)
        {
            Transform item = content.GetChild(i);

            int index = int.Parse(item.name.Replace("Ts_Item", ""));

            m_updateFunc.Invoke(index, (RectTransform)item);

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)item);
        }
    }
}