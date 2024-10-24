using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Generate sampled points.
    /// Create a new mesh composed of multiple quads, one per sampled point, that are then statically batched.
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