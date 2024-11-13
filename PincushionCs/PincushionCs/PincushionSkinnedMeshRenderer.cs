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
        /// <summary>
        /// The buffer used to store the vertices of the source mesh.
        /// </summary>
        private ComputeBuffer sourceVerticesBuffer;
        /// <summary>
        /// The buffer used to store the normals of the source mesh.
        /// </summary>
        private ComputeBuffer sourceNormalsBuffer;
        /// <summary>
        /// The buffer used to store the sampled triangles.
        /// </summary>
        private ComputeBuffer sampledTrianglesBuffer;
        /// <summary>
        /// The property ID of the source vertices buffer.
        /// </summary>
        private int sourceVerticesId;
        /// <summary>
        /// The property ID of the source normals buffer.
        /// </summary>
        private int sourceNormalsId;
        
        
        public override void Initialize()
        {
            base.Initialize();
            
            skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            sourceMesh = new Mesh();
            sampledMeshFilter = points.AddComponent<MeshFilter>();
            sampledMeshRenderer = points.AddComponent<MeshRenderer>();
        }


        protected override void SampleMesh(float pointsPerM)
        {
            sampledMeshRenderer.sharedMaterial = material;
            // Sample the triangles.
            Mesh skinnedMesh = skinnedMeshRenderer.sharedMesh;
            UIntPtr[] sourceTriangles = skinnedMesh.GetTriangles();
            int[] sampledTriangles = skinnedMesh.GetSampledTriangles(pointsPerM, transform.localScale.magnitude, sourceTriangles);
            int numSourceVertices = skinnedMesh.vertexCount;

            sourceVerticesBuffer = new ComputeBuffer(numSourceVertices, 12);
            sourceNormalsBuffer = new ComputeBuffer(numSourceVertices, 12);
            sampledTrianglesBuffer = new ComputeBuffer(sampledTriangles.Length / 3, 12);
            
            // Set the triangles buffer.
            sampledTrianglesBuffer.SetData(sampledTriangles);
            sourceVerticesId = Shader.PropertyToID("_PincushionSourceVertices");
            sourceNormalsId = Shader.PropertyToID("_PincushionSourceNormals");
            int sourceTrianglesId = Shader.PropertyToID("_PincushionSampledTriangles");
            material.SetBuffer(sourceVerticesId, sourceVerticesBuffer);
            material.SetBuffer(sourceNormalsId, sourceNormalsBuffer);
            material.SetBuffer(sourceTrianglesId, sampledTrianglesBuffer);
            
            // Create the mesh.
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[sampledTriangles.Length / 3];
            mesh.normals = new Vector3[sampledTriangles.Length / 3];
            mesh.triangles = new int[sampledTriangles.Length];
            mesh.SetPointTopology();
            sampledMeshFilter.mesh = mesh;
        }


        protected override string GetShaderName()
        {
            return "PincushionDynamic";
        }


        private void Update()
        {
            if (PincushionManager.Instance.renderMode != PincushionRenderMode.DoNot)
            {
                // Bake the mesh to get the vertices and normals.
                skinnedMeshRenderer.BakeMesh(sourceMesh);
                
                // Set the vertex and normal buffers.
                sourceVerticesBuffer.SetData(sourceMesh.vertices);

                // We only care about normals when we have to find backfacing points.
                if (PincushionManager.Instance.renderMode == PincushionRenderMode.HideBackfacing)
                {
                    sourceNormalsBuffer.SetData(sourceMesh.normals);
                }
            }
        }


        private void OnDestroy()
        {
            sourceVerticesBuffer.Release();
            sourceNormalsBuffer.Release();
            sampledTrianglesBuffer.Release();
        }
    }
}