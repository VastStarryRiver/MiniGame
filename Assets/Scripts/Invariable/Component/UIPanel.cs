using UnityEngine;



namespace Invariable
{
    public class UIPanel : MonoBehaviour
    {
        /// <summary>
        /// 关闭当前面板（有弹窗组件则走弹窗关闭）
        /// </summary>
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