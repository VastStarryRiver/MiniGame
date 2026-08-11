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
        private Dictionary<RectTransform, TextMeshProUGUI> m_textCache = null;
        private int m_index1;
        private int m_index2;



        private void Awake()
        {
            m_content = new List<string>();
            m_items = new List<RectTransform>();
            m_textCache = new Dictionary<RectTransform, TextMeshProUGUI>();
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
                    m_items[i].gameObject.SetActive(false);
                }
            }

            if (GameManager.HasInstance)
            {
                for (int i = 1; i < m_index2; i++)
                {
                    GameManager.Instance.CancelInvokeByKey($"{HotUpdateConst.Timer_FloatTextPanel_Prefix}{i}");
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
            GetItemText(trans).text = content;
            trans.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(trans);
            trans.anchoredPosition = new Vector2(0, 200);
            trans.DOAnchorPos(new Vector2(0, 300), 0.5f).SetTarget(trans).OnComplete(() =>
            {
                GameManager.Instance.DelayCallSeconds($"{HotUpdateConst.Timer_FloatTextPanel_Prefix}{m_index2}", () =>
                {
                    trans.gameObject.SetActive(false);

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
        /// 获取可复用的浮动文本条目
        /// </summary>
        private RectTransform GetItem()
        {
            for (int i = 0; i < m_items.Count; i++)
            {
                if (!m_items[i].gameObject.activeSelf)
                {
                    return m_items[i];
                }
            }

            RectTransform trans = GameObject.Instantiate(m_objItem, Vector3.zero, Quaternion.identity, transform).GetComponent<RectTransform>();
            m_items.Add(trans);

            return trans;
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
    }
}