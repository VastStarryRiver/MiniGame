using UnityEngine;



namespace Invariable
{
    public class UIPanel : MonoBehaviour
    {
        public void Close()
        {
            UIPopup uiPopup = gameObject.GetComponent<UIPopup>();

            if (uiPopup == null)
            {
                Utils.CloseUIPrefabPanel(gameObject.name);
            }
            else
            {
                uiPopup.Close();
            }
        }
    }
}