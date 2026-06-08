using UnityEngine;
using Invariable;
using DG.Tweening;



namespace HotUpdate
{
    public class MainPanel : UIPanel
    {
        private void Awake()
        {

        }

        private void Start()
        {
            GameManager.Instance.InvokeEventCallBack("Launcher_StartGame");
        }
    }
}