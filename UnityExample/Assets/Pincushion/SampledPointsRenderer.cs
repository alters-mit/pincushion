using UnityEngine;


namespace Pincushion
{
    [RequireComponent(typeof(MeshFilter))]
    public class SampledPointsRenderer : MonoBehaviour
    {
        public Mesh mesh;
        public float pointsPerM = 0.015f;
        public float pointSize = 0.02f;


        private void Awake()
        {
            // Sample the points.
            GetComponent<MeshFilter>().mesh = mesh.GetSamplesMesh(transform, pointsPerM, pointSize);
        }
    }
}