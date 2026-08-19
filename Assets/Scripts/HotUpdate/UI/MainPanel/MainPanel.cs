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

        private bool m_hasReportedRank;



        private void Awake()
        {
            GameManager.Instance.InvokeEventCallBack<object>(InvariableConst.Event_Launcher_StartGame, null); // 销毁热更新面板
            m_hasReportedRank = false;
        }

        private void OnEnable()
        {
            ShowAuthButton();
        }

        private void Start()
        {
            PlayBGM();
            PlayBtnAnim();
        }

        private void OnDisable()
        {
            HideAuthButton();
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
        /// 同步平台资料并显示未授权时的锚点按钮
        /// </summary>
        private void ShowAuthButton()
        {
            RectTransform authAnchor = (RectTransform)m_btnAuth.transform;
            SdkManager.Instance.SyncPlatformUserInfo(authAnchor, OnAuthResult);
        }

        /// <summary>
        /// 隐藏授权锚点并销毁平台原生授权按钮
        /// </summary>
        private void HideAuthButton()
        {
            m_btnAuth.gameObject.SetActive(false);
            SdkManager.Instance.DestroyPlatformUserInfoButton();
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
            CloudManager.Instance.GetRankList("Score", CloudRankTypes.World, (list) =>
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

                    if (list[i] != null)
                    {
                        nickName = list[i].NickName ?? "";
                        avatarUrl = list[i].AvatarUrl ?? "";

                        if (list[i].Data != null)
                        {
                            list[i].Data.TryGetValue("Score", out score);
                        }
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
            SdkManager.Instance.RequestPlatformUserInfoAuth((RectTransform)m_btnAuth.transform, OnAuthResult);
        }

        /// <summary>
        /// 平台授权结果回调，成功后上报一次排行榜数据
        /// </summary>
        private void OnAuthResult(bool success)
        {
            if (!success || m_hasReportedRank)
            {
                return;
            }

            m_hasReportedRank = true;

            string scoreText = SdkManager.Instance.GetCloudData("Score", "0");

            if (!double.TryParse(scoreText, out double score))
            {
                return;
            }

            CloudManager.Instance.ReportRankScore("Score", score);
        }
    }
}