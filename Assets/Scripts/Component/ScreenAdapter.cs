using UnityEngine;



[ExecuteInEditMode]
public class ScreenAdapter : MonoBehaviour
{
    private void OnEnable()
    {
        SdkManager.Instance.AddScreenAdapter(this);
    }

    private void OnDisable()
    {
        SdkManager.Instance.RemoveScreenAdapter(this);
    }
}