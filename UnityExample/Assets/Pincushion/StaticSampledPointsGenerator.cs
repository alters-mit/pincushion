using System;
using System.Linq;
using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Generate sampled points.
    /// Either set this mesh's vertices and rendering parameters to show the mesh or create a new mesh + parameters.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class StaticSampledPointsGenerator : MonoBehaviour
    {
        /// <summary>
        /// The number of points per square meter.
        /// </summary>
        public float pointsPerM = 0.015f;
        /// <summary>
        /// The size of each point in meters.
        /// </summary>
        public float pointSize = 0.02f;
        /// <summary>
        /// The color of each point.
        /// </summary>
        public Color color = new Color(0.9f, 0.9f, 0.9f);
        /// <summary>
        /// What to do with the points once they've been sampled.
        /// </summary>
        public StaticPointsCreationMode mode = StaticPointsCreationMode.replace;


        private void Awake()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            // Sample the points.
            Vector3[] points = meshFilter.mesh.GetSampledPoints(pointsPerM);
            
            // Create the material.
            Material material = new Material(Shader.Find("Pincushion/StaticPoints"));
            // Set material values.
            material.SetFloat("_PointSize", pointSize);
            material.SetColor("_Color", color);
            
            // Decide what to do with the material and points.
            if (mode == StaticPointsCreationMode.create)
            {
                Create(points, material);
            }
            else if (mode == StaticPointsCreationMode.replace)
            {
                meshFilter.mesh.vertices = points;
                meshFilter.mesh.SetTopology();
                GetComponent<MeshRenderer>().material = material;
            }
            else if (mode == StaticPointsCreationMode.createAndHideOriginal)
            {
                Create(points, material).SetOriginalVisibility(false);
            }
            else
            {
                throw new Exception("Invalid mode: " + mode);
            }
        }


        /// <summary>
        /// Create a new object to render the sampled points.
        /// </summary>
        /// <param name="points">The sampled points.</param>
        /// <param name="material">A material that will be used with the points.</param>
        private StaticPointsRenderer Create(Vector3[] points, Material material)
        {
            // Create a new object.
            GameObject go = new GameObject();
            // Match my transform.
            Transform t = transform;
            go.transform.position = t.position;
            go.transform.rotation = t.rotation;
            go.transform.localScale = t.localScale;
            // Add the mesh and the renderer.
            Mesh sampledMesh = new Mesh();
            sampledMesh.vertices = points;
            sampledMesh.SetTopology();
            int[] indices = Enumerable.Range(0, points.Length).ToArray();
            sampledMesh.SetIndices(indices, 0, indices.Length, MeshTopology.Points, 0);
            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.mesh = sampledMesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            // Set the material.
            mr.material = material;
            // Render.
            StaticPointsRenderer staticPointsRenderer = go.AddComponent<StaticPointsRenderer>();
            staticPointsRenderer.originalGameObject = gameObject;
            staticPointsRenderer.meshRenderer = mr;
            // Parent myself.
            t.parent = go.transform;
            return staticPointsRenderer;
        }
    }
}