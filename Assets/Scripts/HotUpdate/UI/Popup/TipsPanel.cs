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
        public void ShowInfo(string content = "", string text1 = "", string text2 = "", Action callBack1 = null, Action callBack2 = null)
        {
            transform.Find("Ts_Parent/Ts_Layout2/Text_Content").GetComponent<TextMeshProUGUI>().text = content;

            GameObject btn2 = transform.Find("Ts_Parent/Ts_Layout/Btn_2").gameObject;

            if (!string.IsNullOrEmpty(text2))
            {
                btn2.SetActive(true);

                transform.Find("Ts_Parent/Ts_Layout/Btn_2/Text_2").GetComponent<TextMeshProUGUI>().text = text2;

                transform.Find("Ts_Parent/Ts_Layout/Btn_2").GetComponent<UIButton>().AddClickListener(() =>
                {
                    callBack2?.Invoke();
                    Close();
                });
            }
            else
            {
                btn2.SetActive(false);
            }

            transform.Find("Ts_Parent/Ts_Layout/Btn_1/Text_1").GetComponent<TextMeshProUGUI>().text = text1;

            transform.Find("Ts_Parent/Ts_Layout/Btn_1").GetComponent<UIButton>().AddClickListener(() =>
            {
                callBack1?.Invoke();
                Close();
            });
        }
    }
}