using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Invariable;



namespace HotUpdate
{
    public class FloatTextPanel : UIPanel
    {
        public GameObject m_item;

        private List<string> m_content;
        private List<RectTransform> m_items;
        private int m_index1;
        private int m_index2;



        private void Awake()
        {
            m_content = new List<string>();
            m_items = new List<RectTransform>();
        }

        private void OnEnable()
        {
            m_content.Clear();
            m_index1 = 1;
            m_index2 = 1;
        }

        private void OnDisable()
        {
            for (int i = 1; i < m_index2; i++)
            {
                GameManager.Instance.CancelInvokeByKey($"FloatTextPanel_{i}");
            }
        }



        public void ShowInfo(string content)
        {
            gameObject.SetActive(true);

            m_content.Add(content);

            RectTransform trans = GetItem();
            trans.Find("Text_Content").GetComponent<TextMeshProUGUI>().text = content;
            trans.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(trans);
            trans.anchoredPosition = new Vector2(0, 200);
            trans.DOAnchorPos(new Vector2(0, 300), 0.5f).OnComplete(() =>
            {
                GameManager.Instance.DelayCallSeconds($"FloatTextPanel_{m_index2}", () =>
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

        private RectTransform GetItem()
        {
            for (int i = 0; i < m_items.Count; i++)
            {
                if (!m_items[i].gameObject.activeSelf)
                {
                    return m_items[i];
                }
            }

            RectTransform trans = GameObject.Instantiate(m_item, Vector3.zero, Quaternion.identity, transform).GetComponent<RectTransform>();
            m_items.Add(trans);

            return trans;
        }
    }
}