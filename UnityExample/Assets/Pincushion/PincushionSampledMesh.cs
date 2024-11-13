using System;
using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Sampled and pre-sampled mesh data used by PincushionSkinnedMeshRenderer.
    /// </summary>
    public struct PincushionSampledMesh
    {
        /// <summary>
        /// The sampled points.
        /// </summary>
        public Vector3[] vertices;
        /// <summary>
        /// The pre-sampled triangles.
        /// </summary>
        public UIntPtr[] triangles;
        /// <summary>
        /// The sampled normals.
        /// </summary>
        public Vector3[] normals;
        /// <summary>
        /// The triangles of the source mesh.
        /// </summary>
        public UIntPtr[] sourceTriangles;
    }
}