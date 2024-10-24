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
    public class PincushionSkinnedMeshRenderer : PincushionRenderer
    {
        /// <summary>
        /// The renderer. This is set on Awake().
        /// </summary>
        private SkinnedMeshRenderer skinnedMeshRenderer;
        /// <summary>
        /// A cached array of indices of vertices, used to quickly re-sample positions.
        /// </summary>
        private UIntPtr[] sampledTriangles;
        /// <summary>
        /// The MeshFilter that handles the sampled points.
        /// </summary>
        private MeshFilter sampledMeshFilter;
        /// <summary>
        /// The MeshRenderer that handles the sampled points.
        /// </summary>
        private MeshRenderer sampledMeshRenderer;
        /// <summary>
        /// This is used to re-sample points.
        /// </summary>
        private Mesh bakedMesh;
        
        
        public override void SetOriginalMeshVisibility(bool visible)
        {
            skinnedMeshRenderer.enabled = visible;
        }
        
        
        protected override void Initialize()
        {
            skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            bakedMesh = new Mesh();
        }


        protected override void SetMesh()
        {
            // Sample the triangles.
            sampledTriangles = skinnedMeshRenderer.sharedMesh.GetSampledTriangles(pointsPerM, transform.localScale.magnitude);
            // Create the mesh.
            Mesh mesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakedMesh);
            // Deterministically set sampled points.
            mesh.SetVerticesFromSampledTriangles(bakedMesh, sampledTriangles);
            mesh.SetPointTopology();
            // Set the mesh.
            sampledMeshFilter = points.AddComponent<MeshFilter>();
            sampledMeshFilter.mesh = mesh;
            sampledMeshRenderer = points.AddComponent<MeshRenderer>();
            sampledMeshRenderer.material = GetMaterial();
        }


        private void Update()
        {
            if (sampledMeshRenderer.enabled)
            {
                skinnedMeshRenderer.BakeMesh(bakedMesh);
                // Set the positions of the points.
                sampledMeshFilter.mesh.SetVerticesFromSampledTriangles(bakedMesh, sampledTriangles);
                sampledMeshFilter.mesh.SetPointTopology();           
            }
        }
    }
}