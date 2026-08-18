using CloudService;
using DG.Tweening;
using Invariable;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



namespace HotUpdate
{
    public class MainPanel : UIPanel
    {
        public RectTransform m_tsTest;
        public TextMeshProUGUI m_textTest;
        public Image m_imgTest;
        public UIButton m_btnAuth;



        private void Awake()
        {
            GameManager.Instance.InvokeEventCallBack<object>(InvariableConst.Event_Launcher_StartGame, null); // 销毁热更新面板
        }

        private void Start()
        {
            PlayBGM();
            PlayBtnAnim();

            RectTransform authAnchor = (RectTransform)m_btnAuth.transform;
            SdkManager.Instance.SyncPlatformUserInfo(authAnchor);
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
            m_tsTest.DOAnchorPos(Vector2.zero, 1f).SetTarget(m_tsTest).SetEase(Ease.InSine).OnComplete(() =>
            {
                m_tsTest.DOAnchorPos(new Vector2(0, -500), 1f).SetTarget(m_tsTest).SetEase(Ease.OutSine);
            });
        }

        /// <summary>
        /// 测试功能1
        /// </summary>
        public void OnTestClick1()
        {
            string str = SdkManager.Instance.GetCloudData("Test1", "");

            if (string.IsNullOrEmpty(str))
            {
                SdkManager.Instance.SetCloudData("Test1", "测试数据1");
                m_textTest.text = "写入测试数据1";
            }
            else
            {
                m_textTest.text = str;
            }
        }

        /// <summary>
        /// 测试功能2
        /// </summary>
        public void OnTestClick2()
        {
            SdkManager.Instance.SetCloudData("Score", "100");
            CloudManager.Instance.ReportRankScore("Score", 100);
            m_textTest.text = "上传排行榜积分";
        }

        /// <summary>
        /// 测试功能3
        /// </summary>
        public void OnTestClick3()
        {
            ConfigManager.GetRoleRuneByID(11, (config) =>
            {
                m_textTest.text = config.Param.ToString();
            });
        }

        /// <summary>
        /// 测试功能4
        /// </summary>
        public void OnTestClick4()
        {
            CloudManager.Instance.GetAllCloudData("Score", (list) =>
            {
                if (this == null || m_textTest == null)
                {
                    return;
                }

                StringBuilder str = new StringBuilder();
                str.Append("排行榜数据如下：");

                if (list == null)
                {
                    m_textTest.text = str.ToString();

                    return;
                }

                for (int i = 0; i < list.Count; i++)
                {
                    string score = "";
                    string nickName = "";
                    string avatarUrl = "";

                    if (list[i] != null && list[i].Data != null)
                    {
                        list[i].Data.TryGetValue("Score", out score);
                        list[i].Data.TryGetValue(CloudDataKeys.ProfileNickName, out nickName);
                        list[i].Data.TryGetValue(CloudDataKeys.ProfileAvatarUrl, out avatarUrl);
                    }

                    str.Append($"\n序号：{i}\n积分：{score}\n昵称：{nickName}");

                    if (i == 0 && !string.IsNullOrEmpty(avatarUrl))
                    {
                        Utils.SetRemoteImage(m_imgTest, "", avatarUrl, false, null);
                    }
                }
                m_textTest.text = str.ToString();
            });
        }

        /// <summary>
        /// 授权按钮点击，发起平台授权
        /// </summary>
        public void OnAuthClick()
        {
            SdkManager.Instance.RequestPlatformUserInfoAuth((RectTransform)m_btnAuth.transform);
        }
    }
}