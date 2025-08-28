using UnityEngine;
using System.Collections.Generic;



public class UIManager : Singleton<UIManager>
{
    public Dictionary<int, GameObject> m_panelList;



    public void Play()
    {
        m_panelList = new Dictionary<int, GameObject>();
    }
}