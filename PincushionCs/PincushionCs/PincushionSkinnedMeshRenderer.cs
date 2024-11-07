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
        /// A cached array of triangles from the source mesh.
        /// </summary>
        private UIntPtr[] sourceTriangles;
        /// <summary>
        /// A cached array of points, used to quickly re-sample positions.
        /// </summary>
        private Vector3[] sampledPoints;
        /// <summary>
        /// A cached array of normals, used to quickly re-sample positions.
        /// </summary>
        private Vector3[] sampledNormals;
        /// <summary>
        /// A cached array of triangles, used to quickly re-sample positions.
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
        
        
        public override void Initialize()
        {
            base.Initialize();
            
            skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            bakedMesh = new Mesh();
            sampledMeshFilter = points.AddComponent<MeshFilter>();
            sampledMeshRenderer = points.AddComponent<MeshRenderer>();
        }


        protected override void SampleMesh(float pointsPerM, PincushionManager instance)
        {
            sampledMeshRenderer.sharedMaterial = instance.material;
            // Sample the triangles.
            sourceTriangles = skinnedMeshRenderer.sharedMesh.GetTriangles();
            sampledTriangles = skinnedMeshRenderer.sharedMesh.GetSampledTriangles(pointsPerM, transform.localScale.magnitude, sourceTriangles);
            // Allocate the sampling arrays.
            sampledPoints = new Vector3[sampledTriangles.Length / 3];
            sampledNormals = new Vector3[sampledTriangles.Length / 3];
            // Create the mesh.
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[sampledTriangles.Length / 3];
            mesh.normals = new Vector3[sampledTriangles.Length / 3];
            mesh.triangles = new int[sampledTriangles.Length];
            sampledMeshFilter.mesh = mesh;
        }


        private void OnRenderObject()
        {
            if (sampledMeshRenderer.enabled)
            {
                skinnedMeshRenderer.BakeMesh(bakedMesh);
                // Set the positions of the points.
                sampledMeshFilter.mesh.SetVerticesFromSampledTriangles(bakedMesh, sourceTriangles, 
                    sampledPoints, sampledNormals, sampledTriangles);
                sampledMeshFilter.mesh.SetPointTopology();           
            }
        }
    }
}