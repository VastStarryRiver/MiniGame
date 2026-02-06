using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;



namespace Invariable
{
    public class MiniInputField : MonoBehaviour, IPointerClickHandler
    {
        private TMP_InputField m_inputField;



        void Awake()
        {
            m_inputField = GetComponent<TMP_InputField>();
        }



        public void OnPointerClick(PointerEventData eventData)
        {
            SdkManager.Instance.ShowKeyboard(m_inputField);
        }
    }
}