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
        /// <summary>
        /// The material used to render the points.
        /// </summary>
        protected Material material;


        /// <summary>
        /// Sample points and set the mesh(es).
        /// </summary>
        /// <param name="cam">The main camera.</param>
        public void Sample(Camera cam)
        {
            PincushionManager instance = PincushionManager.Instance;
            float pointsPerM = instance.pointsPerM;
            
            // Multiply the number of points.
            if (instance.multiplyPointsPerMByObjectScale)
            {
                pointsPerM *= transform.localScale.magnitude;
            }
            if (instance.multiplyPointsPerMByCameraDistance)
            {
                pointsPerM *= 1f / (0.1f * Vector3.Distance(cam.transform.position, transform.position));
            }
            
            SampleMesh(pointsPerM);
        }


        /// <summary>
        /// Toggle the visibility of the original mesh.
        /// </summary>
        /// <param name="visible">If true, the mesh will be visible.</param>
        public void SetSourceMeshVisibility(bool visible)
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
        /// Initialize the renderer. This is called by PincushionManager.
        /// </summary>
        public virtual void Initialize()
        {
            myRenderer = GetComponent<Renderer>();

            // Create the points object.
            points = new GameObject();
            Transform t = points.transform;
            t.parent = transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            Vector3 s = t.parent.localScale;
            t.localScale = new Vector3(1 / s.x, 1 / s.y, 1 / s.z);
            
            // Set the layers.
            myRenderer.gameObject.layer = PincushionManager.sourceMeshesLayer;
            points.gameObject.layer = PincushionManager.sampledMeshesLayer;
            points.name = "Sampled Mesh";
            
            // Set the material.
            material = new Material(Shader.Find("Pincushion/" + GetShaderName()));
        }


        /// <summary>
        /// Sample points, create the sampled mesh, and set the material.
        /// </summary>
        protected abstract void SampleMesh(float pointsPerM);


        /// <summary>
        /// Returns the name of the shader used to render the sampled points.
        /// </summary>
        /// <returns></returns>
        protected abstract string GetShaderName();
    }
}