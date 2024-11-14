using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Sample points on a MeshRenderer and convert it into a mesh.
    /// The mesh is sampled exactly once unless manually resampled from PincushionManager.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    public class PincushionMeshRenderer : PincushionRenderer
    {
        private MeshFilter meshFilter;
        private MeshFilter pointsMeshFilter;
        private MeshRenderer pointsMeshRenderer;

        public override void Initialize()
        {
            base.Initialize();
            meshFilter = GetComponent<MeshFilter>();
            pointsMeshFilter = points.AddComponent<MeshFilter>();
            pointsMeshRenderer = points.AddComponent<MeshRenderer>();
        }

        protected override int SampleMesh(float pointsPerM)
        {
            // Sample the points.
            Mesh mesh = meshFilter.mesh.GetSampledMesh(pointsPerM, transform.localScale.magnitude);
            pointsMeshFilter.mesh = mesh;
            // Set the material.
            pointsMeshRenderer.sharedMaterial = material;
            return mesh.vertexCount;
        }


        protected override string GetShaderName()
        {
            return "PincushionStatic";
        }
    }
}