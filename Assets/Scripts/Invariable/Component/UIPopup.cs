using UnityEngine;
using DG.Tweening;



namespace Invariable
{
    public class UIPopup : MonoBehaviour
    {
        public RectTransform m_trans = null;



        private void Awake()
        {
            m_trans.localScale = Vector3.zero;
            m_trans.DOScale(new Vector3(1, 1, 1), 0.3f).SetEase(Ease.OutBack);
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