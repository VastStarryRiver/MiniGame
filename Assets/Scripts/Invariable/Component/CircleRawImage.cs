using UnityEngine;
using UnityEngine.UI;



namespace Invariable
{
    public class CircleRawImage : RawImage
    {
        [Range(3, 100)]
        [SerializeField]
        private int m_segements = 100;
        [SerializeField]
        private bool m_startByTopCenter = false;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var mText = mainTexture;
            if (mText == null)
                return;
            var degreeDelta = 2.0f * Mathf.PI / m_segements;
            var startDegree = m_startByTopCenter ? degreeDelta / 2 : 0;

            var tw = rectTransform.rect.width;
            var th = rectTransform.rect.height;

            var uv = new Vector4(uvRect.x, uvRect.y, uvRect.width, uvRect.height);

            var radius = tw * 0.5f;
            var uvCenterX = uv.x + uv.z * 0.5f;
            var uvCenterY = uv.y + uv.w * 0.5f;
            var uvScaleX = uv.z / tw;
            var uvScaleY = uv.w / th;

            //填充顶点数据 按本地坐标系方式处理
            //1:圆中心点
            vh.AddVert(Vector3.zero, color, new Vector2(uvCenterX, uvCenterY));
            //2:圆周上的点
            var verticeCount = m_segements + 1;
            for (int i = 1; i < verticeCount; i++)
            {
                var cosA = Mathf.Cos(startDegree + degreeDelta * i);
                var sinA = Mathf.Sin(startDegree + degreeDelta * i);
                var pos = new Vector3(cosA * radius, sinA * radius, 0f);
                vh.AddVert(pos, color, new Vector2(pos.x * uvScaleX + uvCenterX, pos.y * uvScaleY + uvCenterY));
            }

            //填充顶点索引
            var triangleCount = m_segements * 3;
            for (int i = 0, index = 1; i < triangleCount - 3; i += 3, ++index)
            {
                vh.AddTriangle(0, index, index + 1);
            }
            vh.AddTriangle(0, verticeCount - 1, 1);
        }

        protected override void OnDisable()
        {
            UnregisterDirtyLayoutCallback(ModifyRectTranform);
            base.OnDisable();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            RegisterDirtyLayoutCallback(ModifyRectTranform);
        }

        private void ModifyRectTranform()
        {
            //修正大小，以宽度为准
            var tw = rectTransform.rect.width;
            var th = rectTransform.rect.height;
            if (tw != th)
            {
                rectTransform.sizeDelta = new Vector2(tw, tw);
            }
        }
    }
}