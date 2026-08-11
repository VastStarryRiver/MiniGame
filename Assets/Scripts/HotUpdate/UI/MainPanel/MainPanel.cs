using DG.Tweening;
using Invariable;
using UnityEngine;



namespace HotUpdate
{
    public class MainPanel : UIPanel
    {
        public UIButton m_btnPlay;
        public RectTransform m_tsPlay;



        private void Awake()
        {
            GameManager.Instance.InvokeEventCallBack<object>(InvariableConst.Event_Launcher_StartGame, null); // 销毁热更新面板
        }

        private void Start()
        {
            PlayBGM();
            PlayBtnAnim();
            m_btnPlay.AddClickListener(OnPlayGameClick);
        }



        /// <summary>
        /// 播放背景音乐
        /// </summary>
        private void PlayBGM()
        {
            AudioManager.Instance.PlayBGM("bgm");
        }

        /// <summary>
        /// 播放开始游戏按钮的动画
        /// </summary>
        private void PlayBtnAnim()
        {
            m_tsPlay.DOAnchorPos(Vector2.zero, 1f).SetTarget(m_tsPlay).SetEase(Ease.InSine).OnComplete(() =>
            {
                m_tsPlay.DOAnchorPos(new Vector2(0, -500), 1f).SetTarget(m_tsPlay).SetEase(Ease.OutSine);
            });
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        private void OnPlayGameClick()
        {
            ConfigManager.GetRoleRuneByID(11, (config) =>
            {
                GameLog.Info(config.Param);
            });
        }
    }
}