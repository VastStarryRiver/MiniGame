using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;



namespace Invariable
{
    public class GameLoadingPanel : MonoBehaviour
    {
        private Slider m_sliProgress;
        private TextMeshProUGUI m_textDes;
        private TextMeshProUGUI m_textProgress;



        private void Awake()
        {
            m_sliProgress = gameObject.transform.Find("parent/Sli_Progress").GetComponent<Slider>();
            m_textDes = gameObject.transform.Find("parent/Text_Des").GetComponent<TextMeshProUGUI>();
            m_textProgress = gameObject.transform.Find("parent/Text_Progress").GetComponent<TextMeshProUGUI>();
        }



        public void SetProgress(float nowDownloadNum, float needDownloadNum, string progressStr = "")
        {
            m_sliProgress.value = nowDownloadNum / needDownloadNum;

            if (!string.IsNullOrEmpty(progressStr))
            {
                m_textProgress.text = progressStr;
            }
            else
            {
                m_textProgress.text = nowDownloadNum + "/" + needDownloadNum;
            }
        }

        public void SetDes(string text)
        {
            m_textDes.text = text;
        }
    }
}