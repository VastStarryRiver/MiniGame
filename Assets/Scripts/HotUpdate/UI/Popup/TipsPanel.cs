using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.U2D;
using Invariable;



namespace HotUpdate
{
    public class TipsPanel : UIPanel
    {
        public TextMeshProUGUI m_textTitle;
        public TextMeshProUGUI m_textContent;
        public TextMeshProUGUI m_textBtn1;
        public TextMeshProUGUI m_textBtn2;
        public UIButton m_btn1;
        public UIButton m_btn2;



        public void ShowInfo(string content = "", string text1 = "", string text2 = "", Action callBack1 = null, Action callBack2 = null, string title = "")
        {
            if (string.IsNullOrEmpty(title))
            {
                m_textTitle.text = "提示";
            }
            else
            {
                m_textTitle.text = title;
            }

            m_textContent.text = content;

            if (!string.IsNullOrEmpty(text2))
            {
                m_btn2.gameObject.SetActive(true);

                m_textBtn2.text = text2;

                m_btn2.AddClickListener(() =>
                {
                    callBack2?.Invoke();
                    Close();
                });
            }
            else
            {
                m_btn2.gameObject.SetActive(false);
            }

            m_textBtn1.text = text1;

            m_btn1.AddClickListener(() =>
            {
                callBack1?.Invoke();
                Close();
            });
        }
    }
}