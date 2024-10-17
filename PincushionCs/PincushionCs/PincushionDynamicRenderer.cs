using System;
using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Sample points on a SkinnedMeshRenderer. Use a shader to move the points when the underlying rig moves.
    ///
    /// This component samples points exactly once.
    /// It's therefore not suitable for a mesh that is expected to deform by an extreme degree.
    /// Points are rendered using a geometry shader.
    /// This is relatively inefficient, but I haven't found an alternative that is compatible with the built-in render pipeline.
    /// </summary>
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class PincushionDynamicRenderer : MonoBehaviour
    {
        /// <summary>
        /// The color of each point.
        /// </summary>
        public Color pointsColor = Color.gray;
        /// <summary>
        /// The number of points per square meter.
        /// </summary>
        public float pointsPerM = 0.015f;
        /// <summary>
        /// The radius of each point in meters.
        /// </summary>
        public float pointRadius = 0.02f;
        /// <summary>
        /// The renderer. This is set on Awake().
        /// </summary>
        private SkinnedMeshRenderer skinnedMeshRenderer;

        private Color originalColor;
        private UIntPtr[] sampledTriangles;
        private MeshFilter sampledMeshFilter;
        private MeshRenderer sampledMeshRenderer;
        private float scale;


        private void Awake()
        {
            skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            originalColor = skinnedMeshRenderer.material.color;
            scale = skinnedMeshRenderer.transform.localScale.magnitude;
            Set();
        }


        public void Set()
        {
            // Sample the triangles.
            Mesh originalMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(originalMesh);
            sampledTriangles = originalMesh.GetSampledTriangles(pointsPerM, 1);
            // Create the mesh.
            Mesh mesh = new Mesh();
            // Deterministically set sampled points.
            mesh.SetVerticesFromSampledTriangles(originalMesh.vertices, sampledTriangles);
            mesh.SetPointTopology();
            mesh.RecalculateBounds();


            // Set the material.
            Material material = new Material(Shader.Find("Pincushion/DynamicPoints"));
            material.SetColor("_Color", pointsColor);
            material.SetFloat("_PointSize", pointRadius);
            
            // Create the child object that will hold the sampled points.
            GameObject go = new GameObject();
            Transform t = go.transform;
            t.parent = transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            // Set the mesh.
            sampledMeshFilter = go.AddComponent<MeshFilter>();
            sampledMeshFilter.mesh = mesh;
            sampledMeshRenderer = go.AddComponent<MeshRenderer>();
            sampledMeshRenderer.material = material;
        }


        private void OnRenderObject()
        {
            // Bake the mesh into the samples mesh.
            Mesh mesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(mesh);
            // Set the positions of the points.
            sampledMeshFilter.mesh.SetVerticesFromSampledTriangles(mesh.vertices, sampledTriangles);
        }
    }
}