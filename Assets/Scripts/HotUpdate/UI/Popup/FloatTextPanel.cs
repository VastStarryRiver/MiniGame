using DG.Tweening;
using Invariable;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



namespace HotUpdate
{
    public class FloatTextPanel : UIPanel
    {
        public GameObject m_objItem;

        private List<string> m_content = null;
        private List<RectTransform> m_items = null;
        private List<string> m_timerKeys = null;
        private Dictionary<RectTransform, TextMeshProUGUI> m_textCache = null;
        private int m_index1;
        private int m_index2;



        private void Awake()
        {
            m_content = new List<string>();
            m_items = new List<RectTransform>();
            m_timerKeys = new List<string>();
            m_textCache = new Dictionary<RectTransform, TextMeshProUGUI>();
            EnsureTimerKeys(8);
        }

        private void OnEnable()
        {
            m_content.Clear();
            m_index1 = 1;
            m_index2 = 1;
        }

        private void OnDisable()
        {
            transform.DOKill();

            if (m_items != null)
            {
                for (int i = 0; i < m_items.Count; i++)
                {
                    if (m_items[i] == null)
                    {
                        continue;
                    }

                    m_items[i].DOKill();
                    PoolUtils.ReleaseGameObject(HotUpdateConst.Pool_FloatTextItem, m_items[i].gameObject);
                }

                m_items.Clear();
            }

            if (GameManager.HasInstance)
            {
                EnsureTimerKeys(m_index2 - 1);

                for (int i = 1; i < m_index2; i++)
                {
                    GameManager.Instance.CancelInvokeByKey(GetTimerKey(i));
                }
            }
        }



        /// <summary>
        /// 显示一条浮动提示文本
        /// </summary>
        public void ShowInfo(string content)
        {
            gameObject.SetActive(true);

            m_content.Add(content);

            RectTransform trans = GetItem();

            if (trans == null)
            {
                return;
            }

            GetItemText(trans).text = content;
            trans.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(trans);
            trans.anchoredPosition = new Vector2(0, 200);
            trans.DOAnchorPos(new Vector2(0, 300), 0.5f).SetTarget(trans).OnComplete(() =>
            {
                string timerKey = GetTimerKey(m_index2);
                GameManager.Instance.DelayCallSeconds(timerKey, () =>
                {
                    RecycleItem(trans);

                    m_index1++;

                    if (m_index1 > m_content.Count)
                    {
                        gameObject.SetActive(false);
                    }
                }, 0.5f);

                m_index2++;
            });
        }

        /// <summary>
        /// 从对象池取出浮动文本条目
        /// </summary>
        private RectTransform GetItem()
        {
            GameObject go = PoolUtils.GetGameObject(HotUpdateConst.Pool_FloatTextItem, m_objItem, transform);

            if (go == null)
            {
                return null;
            }

            RectTransform trans = go.transform as RectTransform;

            if (!m_items.Contains(trans))
            {
                m_items.Add(trans);
            }

            return trans;
        }

        /// <summary>
        /// 归还浮动文本条目到对象池
        /// </summary>
        private void RecycleItem(RectTransform trans)
        {
            if (trans == null)
            {
                return;
            }

            trans.DOKill();
            m_items.Remove(trans);
            PoolUtils.ReleaseGameObject(HotUpdateConst.Pool_FloatTextItem, trans.gameObject);
        }

        /// <summary>
        /// 获取条目文本组件（字典缓存）
        /// </summary>
        private TextMeshProUGUI GetItemText(RectTransform trans)
        {
            if (m_textCache.TryGetValue(trans, out TextMeshProUGUI text) && text != null)
            {
                return text;
            }

            // 动态复用 item，首次查找文本节点后缓存
            text = trans.Find("Text_Content").GetComponent<TextMeshProUGUI>();
            m_textCache[trans] = text;

            return text;
        }

        /// <summary>
        /// 预生成飘字计时器 key，避免运行时插值分配
        /// </summary>
        private void EnsureTimerKeys(int maxIndex)
        {
            m_timerKeys ??= new List<string>();

            while (m_timerKeys.Count < maxIndex)
            {
                m_timerKeys.Add(HotUpdateConst.Timer_FloatTextPanel_Prefix + (m_timerKeys.Count + 1));
            }
        }

        /// <summary>
        /// 按序号取预生成的计时器 key
        /// </summary>
        private string GetTimerKey(int index)
        {
            EnsureTimerKeys(index);

            return m_timerKeys[index - 1];
        }
    }
}