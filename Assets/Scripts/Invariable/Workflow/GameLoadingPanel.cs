using TMPro;
using UnityEngine;
using UnityEngine.UI;



namespace Invariable
{
    public class GameLoadingPanel : MonoBehaviour
    {
        public Slider m_sliProgress;
        public TextMeshProUGUI m_textDes;



        /// <summary>
        /// 设置加载进度条
        /// </summary>
        public void SetProgress(float nowDownloadNum, float needDownloadNum)
        {
            m_sliProgress.value = nowDownloadNum / needDownloadNum;
        }

        /// <summary>
        /// 设置加载提示文本
        /// </summary>
        public void SetDes(string text)
        {
            m_textDes.text = text;
        }
    }
}