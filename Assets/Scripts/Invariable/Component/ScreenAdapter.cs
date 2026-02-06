using UnityEngine;



namespace Invariable
{
    [ExecuteInEditMode]
    public class ScreenAdapter : MonoBehaviour
    {
        public bool m_isMoveDown;



        private void OnEnable()
        {
            SdkManager.Instance.AddScreenAdapter(this);
        }

        private void OnDisable()
        {
            SdkManager.Instance.RemoveScreenAdapter(this);
        }
    }
}