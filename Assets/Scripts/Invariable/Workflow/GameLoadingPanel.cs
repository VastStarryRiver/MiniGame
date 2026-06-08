using UnityEngine;
using UnityEngine.UI;
using TMPro;



namespace Invariable
{
    public class GameLoadingPanel : MonoBehaviour
    {
        public Slider m_sliProgress;
        public TextMeshProUGUI m_textDes;



        public void SetProgress(float nowDownloadNum, float needDownloadNum)
        {
            m_sliProgress.value = nowDownloadNum / needDownloadNum;
        }

        public void SetDes(string text)
        {
            m_textDes.text = text;
        }
    }
}