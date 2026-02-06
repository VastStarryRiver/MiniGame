using UnityEngine;



namespace Invariable
{
    public class UIPanel : MonoBehaviour
    {
        public void Close()
        {
            UIManager.Instance.CloseUIPanel(name);
        }
    }
}