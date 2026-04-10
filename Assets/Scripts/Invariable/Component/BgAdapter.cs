using UnityEngine;



namespace Invariable
{
    [ExecuteInEditMode]
    public class BgAdapter : MonoBehaviour
    {
        public float m_width = 1536.0f;
        public float m_height = 2688.0f;



        private void Start()
        {
            RectTransform trans = transform.GetComponent<RectTransform>();
            RectTransform parent = transform.parent.GetComponent<RectTransform>();
            float mult1 = parent.rect.width / m_width;
            float mult2 = parent.rect.height / m_height;
            float mult = mult1 > mult2 ? mult1 : mult2;
            trans.sizeDelta = new Vector2(m_width * mult, m_height * mult);
        }
    }
}