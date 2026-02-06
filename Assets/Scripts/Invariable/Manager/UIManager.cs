using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Invariable
{
    public class UIManager : Singleton<UIManager>
    {
        private Dictionary<string, UIPanel> m_allPanel = null;
        public Dictionary<string, UIPanel> AllPanel
        {
            get
            {
                m_allPanel ??= new Dictionary<string, UIPanel>();
                return m_allPanel;
            }
        }

        public void AddUIPanel(string name, UIPanel uiPanel)
        {
            m_allPanel ??= new Dictionary<string, UIPanel>();

            if (!m_allPanel.ContainsKey(name))
            {
                m_allPanel.Add(name, uiPanel);
            }
        }

        public void CloseAllUIPanel()
        {
            List<string> names = new List<string>();

            foreach (var item in m_allPanel)
            {
                names.Add(item.Key);
            }

            for (int i = 0; i < names.Count; i++)
            {
                CloseUIPanel(names[i]);
            }

            m_allPanel.Clear();
        }

        public void CloseUIPanel(string name)
        {
            if (m_allPanel.ContainsKey(name))
            {
                GameObject.Destroy(m_allPanel[name].gameObject);
                m_allPanel.Remove(name);
            }
        }
    }
}