using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;



namespace Invariable
{
    public class CircleImage : Image
    {
        [Range(3, 100)]
        [SerializeField]
        private int m_segments = 100;
        [SerializeField]
        private bool m_isStartByTopCenter = false;



        protected override void OnEnable()
        {
            base.OnEnable();
            RegisterDirtyLayoutCallback(ModifyRectTranform);
        }

        protected override void OnDisable()
        {
            UnregisterDirtyLayoutCallback(ModifyRectTranform);
            base.OnDisable();
        }



        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            float degreeDelta = 2.0f * Mathf.PI / m_segments;
            float startDegree = m_isStartByTopCenter ? degreeDelta / 2 : 0;

            float tw = rectTransform.rect.width;
            float th = rectTransform.rect.height;

            Vector4 uv = overrideSprite != null ? DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;
            float radius = tw * 0.5f;
            float uvCenterX = (uv.x + uv.z) * 0.5f;
            float uvCenterY = (uv.y + uv.w) * 0.5f;
            float uvScaleX = (uv.z - uv.x) / tw;
            float uvScaleY = (uv.w - uv.y) / th;

            // 填充顶点数据 按本地坐标系方式处理
            // 1:圆中心点
            vh.AddVert(Vector3.zero, color, new Vector2(uvCenterX, uvCenterY));
            // 2:圆周上的点
            int verticeCount = m_segments + 1;

            for (int i = 1; i < verticeCount; i++)
            {
                float cosA = Mathf.Cos(startDegree + degreeDelta * i);
                float sinA = Mathf.Sin(startDegree + degreeDelta * i);
                Vector3 pos = new Vector3(cosA * radius, sinA * radius, 0f);
                vh.AddVert(pos, color, new Vector2(pos.x * uvScaleX + uvCenterX, pos.y * uvScaleY + uvCenterY));
            }

            // 填充顶点索引
            int triangleCount = m_segments * 3;

            for (int i = 0, index = 1; i < triangleCount - 3; i += 3, ++index)
            {
                vh.AddTriangle(0, index, index + 1);
            }

            vh.AddTriangle(0, verticeCount - 1, 1);
        }



        /// <summary>
        /// 按宽度修正圆形区域尺寸
        /// </summary>
        private void ModifyRectTranform()
        {
            // 修正大小，以宽度为准
            float tw = rectTransform.rect.width;
            float th = rectTransform.rect.height;

            if (tw != th)
            {
                rectTransform.sizeDelta = new Vector2(tw, tw);
            }
        }
    }
}