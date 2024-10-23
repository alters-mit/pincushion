using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Abstract base class that creates or replaces a mesh with sampled points.
    /// </summary>
    public abstract class PincushionRenderer : MonoBehaviour
    {
        /// <summary>
        /// The number of points per square meter.
        /// </summary>
        public float pointsPerM = 80f;
        /// <summary>
        /// The radius of each point in meters.
        /// </summary>
        public float pointRadius = 0.02f;
        /// <summary>
        /// If true, hide points facing away from the camera.
        /// </summary>
        public bool occludeBackFacing = true;
        

        /// <summary>
        /// Sample points and set the mesh(es).
        /// </summary>
        public abstract void Set();

        
        /// <summary>
        /// Toggle the visibility of the original mesh.
        /// </summary>
        /// <param name="visible">If true, the mesh will be visible.</param>
        public abstract void SetOriginalMeshVisibility(bool visible);
        
        
        /// <summary>
        /// Toggle the visibility of the sampled mesh(es).
        /// </summary>
        /// <param name="visible">If true, the mesh(es) will be visible.</param>
        public abstract void SetSampledMeshVisibility(bool visible);
    }
}