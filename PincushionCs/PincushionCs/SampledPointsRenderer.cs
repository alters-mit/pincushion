using System;
using UnityEngine;


namespace Pincushion
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Renderer))]
    public class SampledPointsRenderer : MonoBehaviour
    {
        public float pointsPerM = 0.015f;
        public float pointSize = 0.02f;
        public Color color = new Color(0.9f, 0.9f, 0.9f);
        private ComputeBuffer m_buffer;
        private Material m_material;


        private void Awake()
        {
            // Sample the points.
            Vector3[] points = GetComponent<MeshFilter>().mesh.GetSampledPoints(pointsPerM);

            // Source: https://discussions.unity.com/t/how-to-pass-a-structured-buffer-in-to-fragment-shader/784320/2
            m_buffer = new ComputeBuffer (points.Length, 12, ComputeBufferType.Default);
            m_buffer.SetData(points);
            // Create the material.
            m_material = new Material(Shader.Find("Pincushion/StaticPoints"));
            GetComponent<MeshFilter>().mesh.vertices = points;
            // Set material values.
            m_material.SetFloat("_PointSize", pointSize);
            m_material.SetColor("_Color", color);
            GetComponent<Renderer>().material = m_material;
        }
        

        private void OnDestroy()
        {
            m_buffer.Release();
        }
    }
}