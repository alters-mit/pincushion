using System;
using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Mesh data used by PincushionSkinnedMeshRenderer.
    /// </summary>
    public struct PincushionMesh
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
    }
}