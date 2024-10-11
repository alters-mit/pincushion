using UnityEngine;


namespace Pincushion
{
    public class StaticPointsRenderer : MonoBehaviour
    {
        [HideInInspector]
        public Renderer r;
        [HideInInspector]
        public GameObject o;

        
        public void SetMyVisibility(bool show)
        {
            r.enabled = show;
        }
        
        
        public void SetOriginalVisibility(bool show)
        {
            o.gameObject.SetActive(show);
        }
    }
}