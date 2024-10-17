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
    public class PincushionDynamicRenderer : PincushionRenderer
    {
        /// <summary>
        /// The color of each point.
        /// </summary>
        public Color pointsColor = Color.gray;
        /// <summary>
        /// The renderer. This is set on Awake().
        /// </summary>
        private SkinnedMeshRenderer skinnedMeshRenderer;

        private Color originalColor;
        private UIntPtr[] sampledTriangles;
        private MeshFilter sampledMeshFilter;
        private MeshRenderer sampledMeshRenderer;
        private Mesh bakedMesh;


        private void Awake()
        {
            skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            originalColor = skinnedMeshRenderer.material.color;
            bakedMesh = new Mesh();
            Set();
        }


        public override void Set()
        {
            // Sample the triangles.
            sampledTriangles = skinnedMeshRenderer.sharedMesh.GetSampledTriangles(pointsPerM, transform.localScale.magnitude);
            // Create the mesh.
            Mesh mesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakedMesh);
            // Deterministically set sampled points.
            mesh.SetVerticesFromSampledTriangles(bakedMesh.vertices, sampledTriangles);
            mesh.SetPointTopology();
            
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


        public override void SetOriginalMeshVisibility(bool visible)
        {
            skinnedMeshRenderer.material.SetColor("_Color", visible ? originalColor : new Color(0, 0, 0, 0));
        }


        public override void SetSampledMeshVisibility(bool visible)
        {
            sampledMeshRenderer.enabled = visible;
        }


        private void Update()
        {
            if (sampledMeshRenderer.enabled)
            {
                skinnedMeshRenderer.BakeMesh(bakedMesh);
                // Set the positions of the points.
                sampledMeshFilter.mesh.SetVerticesFromSampledTriangles(bakedMesh.vertices, sampledTriangles);
                sampledMeshFilter.mesh.SetPointTopology();           
            }
        }
    }
}