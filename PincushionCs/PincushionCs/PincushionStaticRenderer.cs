using System;
using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Generate sampled points.
    /// Create a new mesh composed of multiple icosahedrons (12-sided die), one per sampled point.
    /// Alternatively, create the mesh and replace the original mesh.
    ///
    /// It is reasonable to render sampled points and icosahedrons because:
    ///
    /// - We're assuming that this is a static mesh. It can change position, rotation, etc. but it won't deform.
    /// - Yes, we're multiplying the number of vertices, but computers are really good at handling vertices.
    /// - The dynamic renderer uses a geometry shader, which adds vertices on the GPU. Why not just cache the vertices?
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Renderer))]
    public class PincushionStaticRenderer : PincushionRenderer<MeshFilter>
    {
        /// <summary>
        /// The points will be rendered with this material.
        /// </summary>
        public Material material;


        protected override Mesh GetMesh(MeshFilter meshContainer)
        {
            GetComponent<MeshRenderer>().material = material;
            return meshContainer.mesh.GetIcosahedrons(pointsPerM, pointRadius);
        }


        protected override void ReplaceMesh(MeshFilter meshContainer, Mesh mesh)
        {
            meshContainer.mesh = mesh;
        }


        protected override Renderer SetCreatedMesh(MeshFilter meshContainer)
        {
            MeshRenderer r = meshContainer.gameObject.AddComponent<MeshRenderer>();
            r.material = material;
            return r;
        }
    }
}