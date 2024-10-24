using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Sample points on a MeshRenderer and convert it into a mesh.
    /// The mesh is sampled exactly once unless manually requested via Set().
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Renderer))]
    public class PincushionMeshRenderer : PincushionRenderer
    {
        /// <summary>
        /// My MeshRenderer.
        /// </summary>
        private MeshRenderer mr;
        
        
        public override void SetOriginalMeshVisibility(bool visible)
        {
            mr.enabled = visible;
        }

        
        protected override void Initialize()
        {
            mr = GetComponent<MeshRenderer>();
        }


        protected override void SetMesh()
        {
            // Sample the points.
            Mesh mesh = GetComponent<MeshFilter>().mesh.GetSampledMesh(
                pointsPerM, transform.localScale.magnitude);
            points.AddComponent<MeshFilter>().mesh = mesh;
            // Create and set the material.
            points.AddComponent<MeshRenderer>().material = GetMaterial();
        }
    }
}