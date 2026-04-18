using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;



namespace Invariable
{
    public class UIPopup : MonoBehaviour
    {
        public RectTransform m_trans = null;
        public ScrollRect m_scroll = null;



        private void Awake()
        {
            bool isInertia = false;

            if (m_scroll != null && m_scroll.inertia)
            {
                m_scroll.inertia = false;
                isInertia = true;
            }

            m_trans.localScale = Vector3.zero;
            m_trans.DOScale(new Vector3(1, 1, 1), 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                if (m_scroll != null)
                {
                    m_scroll.content.anchoredPosition = Vector2.zero;
                    m_scroll.inertia = isInertia;
                }
            });
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