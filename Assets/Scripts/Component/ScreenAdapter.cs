using UnityEngine;



[ExecuteInEditMode]
public class ScreenAdapter : MonoBehaviour
{
    private RectTransform m_tsPanel;
    private Vector2Int m_lastScreenSize;
    private ScreenOrientation m_lastOrientation;



    private void Awake()
    {
        m_tsPanel = GetComponent<RectTransform>();
        m_lastScreenSize = Vector2Int.zero;
        m_lastOrientation = ScreenOrientation.AutoRotation;
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }



    private void Refresh()
    {
        bool isArea = SdkManager.Instance.JudgeSafeArea();

        if (isArea || Screen.width != m_lastScreenSize.x || Screen.height != m_lastScreenSize.y || Screen.orientation != m_lastOrientation)
        {
            m_lastScreenSize.x = Screen.width;
            m_lastScreenSize.y = Screen.height;
            m_lastOrientation = Screen.orientation;

            ApplySafeArea();
        }
    }

    private void ApplySafeArea()
    {
        SdkManager.Instance.SetSafeArea();

        SdkManager.Instance.GetSafeAnchor(out Vector2 anchorMin, out Vector2 anchorMax);

        m_tsPanel.anchorMin = anchorMin;
        m_tsPanel.anchorMax = anchorMax;
    }
}