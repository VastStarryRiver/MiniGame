using UnityEngine;
using DG.Tweening;



namespace Invariable
{
    public class UIPopup : MonoBehaviour
    {
        public RectTransform m_trans = null;



        private void Awake()
        {
            CanvasGroup canvasGroup = m_trans.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, 0.2f).SetEase(Ease.Linear);
        }

        public void Close()
        {
            m_trans.DOScale(new Vector3(0, 0, 0), 0.2f).SetEase(Ease.InSine).OnComplete(() =>
            {
                Utils.CloseUIPrefabPanel(gameObject.name);
            });
        }
    }
}