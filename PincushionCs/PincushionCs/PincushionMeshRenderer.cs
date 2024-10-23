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
        public bool keepConstantScaling = false;
        /// <summary>
        /// The parent of the points.
        /// </summary>
        private GameObject pointsParent;
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
            pointsParent = new GameObject();
            Transform t = pointsParent.transform;
            t.parent = transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;

            // Sample the points.
            SampledPoints sampledPoints = GetComponent<MeshFilter>().mesh.GetSampledPoints(
                pointsPerM, transform.localScale.magnitude);
            
            // Get a quad.
            // Source: https://docs.unity3d.com/Manual/Example-CreatingaBillboardPlane.html
            Mesh quadMesh = new Mesh();
            float s = pointRadius / 2;
            quadMesh.vertices = new []
            {
                new Vector3(-s, -s, 0),
                new Vector3(s, -s, 0),
                new Vector3(-s, s, 0),
                new Vector3(s, s, 0),
            };
            quadMesh.triangles = new []
            {
                0, 2, 1,
                2, 3, 1
            };
            // This gets set per-mesh.
            quadMesh.normals = new Vector3[4];
            quadMesh.uv = new [] 
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };

            if (occludeBackFacing)
            {
                material.EnableKeyword("_OCCLUDE_BACKFACING");   
            }

            if (keepConstantScaling)
            {
                material.SetInt("_KeepConstantScaling", 1);
            }

            // Create game objects.
            for (int i = 0; i < sampledPoints.points.Length; i++)
            {
                // Set the mesh.
                GameObject quad = new GameObject();
                MeshFilter meshFilter = quad.AddComponent<MeshFilter>();
                meshFilter.mesh = quadMesh;
                
                // Set the normals.
                Vector3[] normals = new Vector3[4];
                for (int j = 0; j < 4; j++)
                {
                    normals[j] = sampledPoints.normals[i];
                }
                meshFilter.mesh.normals = normals;
                
                quad.AddComponent<MeshRenderer>().sharedMaterial = material;
                
                // Set the transform of the quad.
                Transform q = quad.transform;
                q.parent = t;
                q.localPosition = sampledPoints.points[i];
                q.localRotation = Quaternion.identity;
                q.localScale = Vector3.one;
            }
            
            // Enable static batching.
            StaticBatchingUtility.Combine(pointsParent);
        }


        public override void SetOriginalMeshVisibility(bool visible)
        {
            mr.enabled = visible;
        }


        public override void SetSampledMeshVisibility(bool visible)
        {
            pointsParent.SetActive(visible);
        }
    }
}