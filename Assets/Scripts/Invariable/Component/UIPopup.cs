using DG.Tweening;
using UnityEngine;



namespace Invariable
{
    public class UIPopup : MonoBehaviour
    {
        public RectTransform m_tsTrans;



        private void OnEnable()
        {
            m_tsTrans.DOKill();
            m_tsTrans.localScale = Vector3.one;
            CanvasGroup canvasGroup = m_tsTrans.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, 0.2f).SetTarget(m_tsTrans).SetEase(Ease.Linear);
        }



        /// <summary>
        /// 关闭弹窗并播放缩放退出动画
        /// </summary>
        public void Close()
        {
            m_tsTrans.DOScale(new Vector3(0, 0, 0), 0.2f).SetTarget(m_tsTrans).SetEase(Ease.InSine).OnComplete(() =>
            {
                Utils.CloseUIPrefabPanel(gameObject.name);
            });
        }
    }
}