using System;
using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Generate sampled points.
    /// Create a new mesh composed of multiple icosahedrons (20-sided die), one per sampled point.
    /// Alternatively, create the mesh and replace the original mesh.
    ///
    /// It is reasonable to render sampled points and icosahedrons because:
    ///
    /// - We're assuming that this is a static mesh. It can change position, rotation, etc. but it won't deform.
    /// - Yes, we're multiplying the number of vertices, but computers are really good at handling vertices.
    /// - The dynamic renderer uses a geometry shader, which adds vertices on the GPU. Why not just cache the vertices?
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Renderer))]
    public class PincushionStaticRenderer : PincushionRenderer
    {
        /// <summary>
        /// The points will be rendered with this material.
        /// </summary>
        public Material material;
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
            Vector3[] points = GetComponent<MeshFilter>().mesh.GetSampledPoints(pointsPerM);
            
            // Get a quad.
            // Source: https://docs.unity3d.com/Manual/Example-CreatingaBillboardPlane.html
            Mesh quadMesh = new Mesh();
            float s = pointRadius / 2;
            quadMesh.vertices = new []
            {
                new Vector3(-s, s, 0),
                new Vector3(s, s, 0),
                new Vector3(-s, -s, 0),
                new Vector3(s, -s, 0),
            };
            quadMesh.triangles = new []
            {
                0, 2, 1,
                2, 3, 1
            };
            quadMesh.normals = new[]
            {
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward,
            };
            quadMesh.uv = new [] 
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };

            // Create game objects.
            for (int i = 0; i < points.Length; i++)
            {
                GameObject quad = new GameObject();
                quad.AddComponent<MeshFilter>().sharedMesh = quadMesh;
                quad.AddComponent<MeshRenderer>().sharedMaterial = material;

                // Set the transform of the quad.
                Transform q = quad.transform;
                q.parent = t;
                q.localPosition = points[i];
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