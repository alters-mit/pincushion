using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Generate sampled points.
    /// Create a new mesh composed of multiple quads, one per sampled point, that are then statically batched.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Renderer))]
    public class PincushionMeshRenderer : PincushionRenderer
    {
        /// <summary>
        /// The points will be rendered with this material.
        /// </summary>
        public Material material;
        /// <summary>
        /// If true, points will always render at the same size, regardless of distance.
        /// If false, scale the points normally. 
        /// </summary>
        public bool constantScaling;
        /// <summary>
        /// Increase the number of points on closer objects. 
        /// </summary>
        public bool scalePointsPerMByCameraDistance;
        /// <summary>
        /// The parent of the points.
        /// </summary>
        private GameObject points;
        /// <summary>
        /// My MeshRenderer.
        /// </summary>
        private MeshRenderer mr;


        private void Awake()
        {
            mr = GetComponent<MeshRenderer>();
            Set();
        }


        public override void Set()
        {
            points = new GameObject();
            Transform t = points.transform;
            t.parent = transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;

            if (scalePointsPerMByCameraDistance)
            {
                pointsPerM *= 1f / (0.1f * Vector3.Distance(Camera.main.transform.position, transform.position));
            }

            // Sample the points.
            Mesh mesh = GetComponent<MeshFilter>().mesh.GetSampledMesh(
                pointsPerM, transform.localScale.magnitude);
            points.AddComponent<MeshFilter>().mesh = mesh;
            
            // Set the material.
            material.SetFloat("_PointSize", pointRadius);
            if (occludeBackFacing)
            {
                material.EnableKeyword("_OCCLUDE_BACKFACING");   
            }
            if (constantScaling)
            {
                material.EnableKeyword("_CONSTANT_SCALING");   
            }
            points.AddComponent<MeshRenderer>().material = material;
        }


        public override void SetOriginalMeshVisibility(bool visible)
        {
            mr.enabled = visible;
        }


        public override void SetSampledMeshVisibility(bool visible)
        {
            points.SetActive(visible);
        }
    }
}