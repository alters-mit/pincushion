using System;
using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Sample points on a SkinnedMeshRenderer and convert it into a mesh.
    ///
    /// On Set() and Awake(), the *triangles* of the mesh are sampled.
    ///
    /// Per-frame, bake the SkinnedMeshRenderer mesh and use the baked mesh and the cached triangles to resample the points.
    ///
    /// For the sake of performance, when points are sampled, they will always be in the center of the sampled triangles.
    /// This is in contrast to PincushionMeshRenderer, in which points are jostled randomly within their sampled triangles.
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