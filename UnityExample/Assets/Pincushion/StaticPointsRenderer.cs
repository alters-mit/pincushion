using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// This script manages the MeshRenderer that renders the sampled points mesh as well as the original object.
    /// </summary>
    public class StaticPointsRenderer : MonoBehaviour
    {
        /// <summary>
        /// My MeshRenderer.
        /// </summary>
        [HideInInspector]
        public MeshRenderer meshRenderer;
        /// <summary>
        /// The original GameObject. This is assumed to be parented to me.
        /// </summary>
        [HideInInspector]
        public GameObject originalGameObject;

        
        /// <summary>
        /// Show/hide my MeshRenderer.
        /// </summary>
        /// <param name="show">If true, show.</param>
        public void SetMyVisibility(bool show)
        {
            meshRenderer.enabled = show;
        }
        
        
        /// <summary>
        /// Show/hide the child original object.
        /// </summary>
        /// <param name="show">If true, show.</param>
        public void SetOriginalVisibility(bool show)
        {
            originalGameObject.SetActive(show);
        }
    }
}