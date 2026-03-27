using UnityEngine;



namespace Invariable
{
    public class UIPanel : MonoBehaviour
    {
        public void Close()
        {
            Utils.CloseUIPrefabPanel(gameObject.name);
        }
    }
}