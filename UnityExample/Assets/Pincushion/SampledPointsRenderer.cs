using UnityEngine;


namespace Pincushion
{
    [RequireComponent(typeof(MeshFilter))]
    public class SampledPointsRenderer : MonoBehaviour
    {
        public float pointsPerM = 0.015f;
        public Color color = new Color(0.9f, 0.9f, 0.9f);
        public float pointSize = 0.02f;
        private ComputeBuffer m_buffer;
        private Material m_material;
        private static Shader _shader;
        private static int m_colorIndex;
        private static int m_transformIndex;
        private static int m_PointSizeIndex;
        private static int m_bufferIndex;
        private static Shader m_shader
        {
            get
            {
                if (_shader == null)
                {
                    _shader = Shader.Find("Pincushion/PointsShaderStatic");
                    m_colorIndex = m_shader.FindPropertyIndex("_Color");
                    m_transformIndex = m_shader.FindPropertyIndex("_Transform");
                    m_PointSizeIndex = m_shader.FindPropertyIndex("_PointSize");
                    m_bufferIndex = m_shader.FindPropertyIndex("_PointBuffer");
                }
                return _shader;
            }
        }


        private void Awake()
        {
            // Sample the points.
            Vector3[] points = GetComponent<MeshFilter>().sharedMesh.GetSampledPoints(pointsPerM);
            m_buffer = new ComputeBuffer(points.Length, 12);
            m_buffer.SetData(points);
            
            // Source: https://github.com/keijiro/Pcx/blob/ffc344756b9320584a02a738c8b9d328090e1bc3/Packages/jp.keijiro.pcx/Runtime/PointCloudRenderer.cs#L96C61
            m_material = new Material(m_shader)
            {
                hideFlags = HideFlags.DontSave,
            };
            m_material.EnableKeyword("_COMPUTE_BUFFER");
            m_material.SetColor(m_colorIndex, color);
            m_material.SetMatrix(m_transformIndex, transform.localToWorldMatrix);
            m_material.SetFloat(m_PointSizeIndex, pointSize);
        }


        private void OnRenderObject()
        {
            m_material.SetBuffer(m_bufferIndex, m_buffer);
            Graphics.DrawProceduralNow(MeshTopology.Points, m_buffer.count);
        }
    }
}