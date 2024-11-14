using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Abstract base class that creates or replaces a mesh with sampled points.
    /// </summary>
    public abstract class PincushionRenderer : MonoBehaviour
    {
        /// <summary>
        /// The shader keyword that will let us show every nth point.
        /// </summary>
        private const string SHOW_EVERY_NTH = "_SHOW_EVERY_NTH";
        
        
        /// <summary>
        /// The material used to render the points.
        /// </summary>
        protected Material material;
        /// <summary>
        /// The object that renders the points.
        /// </summary>
        protected GameObject points;
        /// <summary>
        /// The renderer component.
        /// </summary>
        private Renderer myRenderer;
        /// <summary>
        /// The number of sampled points.
        /// </summary>
        private int numSampledPoints;


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
            
            numSampledPoints = SampleMesh(pointsPerM);
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
        /// Show every nth point.
        /// </summary>
        /// <param name="factor">A factor between 0 and 1.</param>
        public void ShowEveryNth(float factor)
        {
            material.EnableKeyword(SHOW_EVERY_NTH);
            int v = factor > 0 ? (int)Mathf.Ceil(numSampledPoints - numSampledPoints / (1 / Mathf.Clamp01(factor))) / 100 : 0;
            if (factor > 0.9f && v == 0)
            {
                v = 1;
            }
            material.SetInt(PincushionManager.showEveryNthId, v);
        }


        
        public void ShowAll()
        {
            material.DisableKeyword(SHOW_EVERY_NTH);
        }


        /// <summary>
        /// Sample points, create the sampled mesh, and set the material.
        /// </summary>
        protected abstract int SampleMesh(float pointsPerM);


        /// <summary>
        /// Returns the name of the shader used to render the sampled points.
        /// </summary>
        /// <returns></returns>
        protected abstract string GetShaderName();
    }
}