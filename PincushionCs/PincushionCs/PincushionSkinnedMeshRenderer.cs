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
    public class PincushionSkinnedMeshRenderer : PincushionRenderer<PincushionSkinnedMeshRenderer>
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

        private ComputeBuffer sourceVerticesBuffer;
        private ComputeBuffer sourceNormalsBuffer;
        private ComputeBuffer sampledTrianglesBuffer;
        
        
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
            UIntPtr[] sourceTriangles = skinnedMeshRenderer.sharedMesh.GetTriangles();
            int[] sampledTriangles = skinnedMeshRenderer.sharedMesh.GetSampledTriangles(pointsPerM, transform.localScale.magnitude, sourceTriangles);
            int numSourceVertices = skinnedMeshRenderer.sharedMesh.vertexCount;

            sourceVerticesBuffer = new ComputeBuffer(numSourceVertices, 12);
            sourceNormalsBuffer = new ComputeBuffer(numSourceVertices, 12);
            sampledTrianglesBuffer = new ComputeBuffer(sampledTriangles.Length / 3, 12);
            
            // Set the triangles buffer.
            sampledTrianglesBuffer.SetData(sampledTriangles);
            material.SetBuffer("sourceVertices", sourceVerticesBuffer);
            material.SetBuffer("sourceNormals", sourceNormalsBuffer);
            material.SetBuffer("sampledTriangles", sampledTrianglesBuffer);
            
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


        private void OnRenderObject()
        {
            if (sampledMeshRenderer.enabled)
            {
                // Bake the mesh to get the vertices and normals.
                skinnedMeshRenderer.BakeMesh(sourceMesh);
                
                // Set the vertex and normal buffers.
                sourceVerticesBuffer.SetData(sourceMesh.vertices);
                sourceNormalsBuffer.SetData(sourceMesh.normals);
                material.SetBuffer("sourceVertices", sourceVerticesBuffer);
                material.SetBuffer("sourceNormals", sourceNormalsBuffer);
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