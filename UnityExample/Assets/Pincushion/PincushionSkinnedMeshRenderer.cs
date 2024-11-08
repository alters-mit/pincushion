using System;
using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Sample points on a SkinnedMeshRenderer and convert it into a mesh.
    ///
    /// Initially, the *triangles* of the mesh are sampled.
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
        /// The sampled mesh data.
        /// </summary>
        private PincushionSampledMesh sampledMesh;
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
        private Mesh sourceMesh;
        
        
        public override void Initialize()
        {
            base.Initialize();
            
            skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            sourceMesh = new Mesh();
            sampledMeshFilter = points.AddComponent<MeshFilter>();
            sampledMeshRenderer = points.AddComponent<MeshRenderer>();
        }


        protected override void SampleMesh(float pointsPerM, PincushionManager instance)
        {
            sampledMeshRenderer.sharedMaterial = instance.material;
            // Sample the triangles.
            UIntPtr[] sourceTriangles = skinnedMeshRenderer.sharedMesh.GetTriangles();
            UIntPtr[] sampledTriangles = skinnedMeshRenderer.sharedMesh.GetSampledTriangles(pointsPerM, transform.localScale.magnitude, sourceTriangles);
            // Allocate the sampling data.
            sampledMesh = new PincushionSampledMesh
            {
                vertices = new Vector3[sampledTriangles.Length / 3],
                triangles = sampledTriangles,
                normals = new Vector3[sampledTriangles.Length / 3],
                sourceTriangles = sourceTriangles
            };
            // Create the mesh.
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[sampledMesh.vertices.Length];
            mesh.normals = new Vector3[sampledMesh.normals.Length];
            mesh.triangles = new int[sampledMesh.triangles.Length];
            sampledMeshFilter.mesh = mesh;
        }


        private void OnRenderObject()
        {
            if (sampledMeshRenderer.enabled)
            {
                skinnedMeshRenderer.BakeMesh(sourceMesh);
                // Set the positions of the points.
                sampledMeshFilter.mesh.SetVerticesFromSampledTriangles(sourceMesh, sampledMesh);
                sampledMeshFilter.mesh.SetPointTopology();           
            }
        }
    }
}