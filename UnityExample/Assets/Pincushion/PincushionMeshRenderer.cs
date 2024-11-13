using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Sample points on a MeshRenderer and convert it into a mesh.
    /// The mesh is sampled exactly once unless manually resampled from PincushionManager.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Renderer))]
    public class PincushionMeshRenderer : PincushionRenderer
    {
        protected override void SampleMesh(float pointsPerM, PincushionManager instance)
        {
            // Sample the points.
            Mesh mesh = GetComponent<MeshFilter>().mesh.GetSampledMesh(
                pointsPerM, transform.localScale.magnitude);
            points.AddComponent<MeshFilter>().mesh = mesh;
            // Set the material.
            points.AddComponent<MeshRenderer>().sharedMaterial = instance.material;
        }
    }
}