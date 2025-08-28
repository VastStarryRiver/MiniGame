using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PolygonCollider2D))]
public class PolygonImage : Image
{
    private PolygonCollider2D polyCollider;

    protected override void Awake()
    {
        base.Awake();
        polyCollider = gameObject.GetComponent<PolygonCollider2D>();
    }

    public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        // 转换为UI局部坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out Vector2 localPoint);

        // 转换为碰撞体坐标系（基于轴心修正坐标系）
        Vector2 pivotOffset = rectTransform.rect.size * rectTransform.pivot;
        localPoint += pivotOffset;

        // 检测点是否在多边形碰撞体内
        return polyCollider.OverlapPoint(localPoint);
    }
}