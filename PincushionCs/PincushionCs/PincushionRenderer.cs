using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Abstract base class that creates or replaces a mesh with sampled points.
    /// </summary>
    public abstract class PincushionRenderer : MonoBehaviour
    {
        /// <summary>
        /// The object that renders the points.
        /// </summary>
        protected GameObject points;
        /// <summary>
        /// The renderer component.
        /// </summary>
        private Renderer myRenderer;


        private void Awake()
        {
            myRenderer = GetComponent<Renderer>();

            // Create the points object.
            points = new GameObject();
            Transform t = points.transform;
            t.parent = transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            Vector3 s = transform.localScale;
            t.localScale = new Vector3(1 / s.x, 1 / s.y, 1 / s.z);
            
            Initialize();
            
            Sample();
        }


        /// <summary>
        /// Sample points and set the mesh(es).
        /// </summary>
        public void Sample()
        {
            Vector3 s = transform.localScale;
            PincushionManager instance = PincushionManager.Instance;
            float pointsPerM = instance.pointsPerM * s.magnitude;
            
            // Scale the number of points.
            if (instance.scalePointsPerMByCameraDistance)
            {
                pointsPerM *= 1f / (0.1f * Vector3.Distance(Camera.main.transform.position, transform.position));
            }
            
            SampleMesh(pointsPerM, instance);
        }


        /// <summary>
        /// Toggle the visibility of the original mesh.
        /// </summary>
        /// <param name="visible">If true, the mesh will be visible.</param>
        public void SetOriginalMeshVisibility(bool visible)
        {
            myRenderer.enabled = visible;
        }
        
        
        /// <summary>
        /// Toggle the visibility of the sampled mesh(es).
        /// </summary>
        /// <param name="visible">If true, the mesh(es) will be visible.</param>
        public void SetSampledMeshVisibility(bool visible)
        {
            points.SetActive(visible);
        }
        
        
        /// <summary>
        /// Initialize on Awake().
        /// </summary>
        protected abstract void Initialize();


        /// <summary>
        /// Sample points, create the sampled mesh, and set the material.
        /// </summary>
        protected abstract void SampleMesh(float pointsPerM, PincushionManager instance);
    }
}