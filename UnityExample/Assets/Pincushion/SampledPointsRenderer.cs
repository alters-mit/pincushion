using UnityEngine;


namespace Pincushion
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class SampledPointsRenderer : MonoBehaviour
    {
        public float pointsPerM = 0.015f;


        private void Awake()
        {
            // Sample the points.
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            Mesh mesh = meshFilter.mesh;
            mesh.vertices =  mesh.GetSampledPoints(pointsPerM);
            mesh.SetIndices(new int[mesh.vertices.Length], MeshTopology.Points, 0);
            GetComponent<MeshRenderer>().material.shader = Shader.Find("Pincushion/PointsShaderStatic");
        }


        private static void GetQuad(Vector3 position)
        {
            
        }
    }
}