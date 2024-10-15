using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// This script manages the Renderer that renders the sampled points mesh as well as the original object.
    /// </summary>
    public class PincushionRenderer : MonoBehaviour
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