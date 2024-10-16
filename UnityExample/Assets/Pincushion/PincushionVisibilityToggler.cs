using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Show/hide the sampled points mesh and the original mesh.
    /// </summary>
    public class PincushionVisibilityToggler : MonoBehaviour
    {
        /// <summary>
        /// My Renderer.
        /// </summary>
        [HideInInspector]
        public Renderer myRenderer;
        /// <summary>
        /// The original GameObject. This is assumed to be parented to me.
        /// </summary>
        [HideInInspector]
        public GameObject originalGameObject;
        
        
        /// <summary>
        /// Show/hide my Renderer.
        /// </summary>
        /// <param name="show">If true, show.</param>
        public void SetMyVisibility(bool show)
        {
            myRenderer.enabled = show;
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