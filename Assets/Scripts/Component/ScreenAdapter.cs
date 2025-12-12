using UnityEngine;



[ExecuteAlways] // 支持编辑器实时预览
public class ScreenAdapter : MonoBehaviour
{
    private RectTransform m_tsPanel;
    private DrivenRectTransformTracker m_tracker;



    private void OnEnable()
    {
        m_tsPanel = GetComponent<RectTransform>();
        AdjustScreen();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Application.isPlaying)
        {
            return;
        }

        AdjustScreen(); // 编辑器非运行时实时更新
    }
#endif

    private void OnDisable() => m_tracker.Clear();



    private void AdjustScreen()
    {
        if (m_tsPanel == null)
        {
            return;
        }

        // 绑定驱动属性（自动更新）
        m_tracker = new DrivenRectTransformTracker();
        m_tracker.Add(this, m_tsPanel, DrivenTransformProperties.AnchorMin | DrivenTransformProperties.AnchorMax);

        Rect safeArea = Screen.safeArea;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        // 归一化坐标（值是0到1）
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        m_tsPanel.anchorMin = anchorMin;
        m_tsPanel.anchorMax = anchorMax;
    }
}