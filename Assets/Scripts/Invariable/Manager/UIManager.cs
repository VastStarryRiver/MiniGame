using System.Collections.Generic;
using UnityEngine;



namespace Invariable
{
    public class UIManager : Singleton<UIManager>
    {
        private static readonly HashSet<string> PooledPanelNames = new HashSet<string>
        {
            "TipsPanel",
        };

        private Dictionary<string, UIPanel> m_allPanel = null;

        public Dictionary<string, UIPanel> AllPanel
        {
            get
            {
                m_allPanel ??= new Dictionary<string, UIPanel>();

                return m_allPanel;
            }
        }



        /// <summary>
        /// 注册已打开的 UI 面板
        /// </summary>
        public void AddUIPanel(string name, UIPanel uiPanel)
        {
            if (!AllPanel.ContainsKey(name))
            {
                AllPanel.Add(name, uiPanel);
            }
        }

        /// <summary>
        /// 判断面板是否启用池化复用
        /// </summary>
        public bool IsPooledPanel(string name)
        {
            return PooledPanelNames.Contains(name);
        }

        /// <summary>
        /// 关闭并清空全部 UI 面板
        /// </summary>
        public void CloseAllUIPanel()
        {
            List<string> names = new List<string>();

            foreach (KeyValuePair<string, UIPanel> item in AllPanel)
            {
                names.Add(item.Key);
            }

            for (int i = 0; i < names.Count; i++)
            {
                CloseUIPanel(names[i]);
            }
        }

        /// <summary>
        /// 关闭指定名称的 UI 面板（池化名单内为隐藏复用）
        /// </summary>
        public void CloseUIPanel(string name)
        {
            if (!AllPanel.TryGetValue(name, out UIPanel panel))
            {
                return;
            }

            if (PooledPanelNames.Contains(name))
            {
                panel.gameObject.SetActive(false);

                return;
            }

            GameObject.Destroy(panel.gameObject);
            AllPanel.Remove(name);
        }
    }
}