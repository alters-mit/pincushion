using System;
using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Generate sampled points.
    /// Either set this mesh's vertices and rendering parameters to show the mesh or create a new mesh + parameters.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class PincushionGenerator : MonoBehaviour
    {
        /// <summary>
        /// The number of points per square meter.
        /// </summary>
        public float pointsPerM = 0.015f;
        /// <summary>
        /// The radius of each point in meters.
        /// </summary>
        public float pointRadius = 0.02f;
        /// <summary>
        /// The color of each point.
        /// </summary>
        public Color color = new Color(0.9f, 0.9f, 0.9f);
        /// <summary>
        /// What to do with the points once they've been sampled.
        /// </summary>
        public CreationMode mode = CreationMode.replace;

        public Material material;


        private void Awake()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            Mesh pointsMesh = meshFilter.mesh.GetIcosahedra(pointsPerM, pointRadius);

            // Decide what to do with the material and points.
            if (mode == CreationMode.create)
            {
                Create(pointsMesh);
            }
            else if (mode == CreationMode.replace)
            {
                meshFilter.mesh = pointsMesh;
                GetComponent<MeshRenderer>().material = material;
            }
            else if (mode == CreationMode.createAndHideOriginal)
            {
                Create(pointsMesh).SetOriginalVisibility(false);
            }
            else
            {
                throw new Exception("Invalid mode: " + mode);
            }
        }


        /// <summary>
        /// Create a new object to render the sampled points.
        /// </summary>
        /// <param name="mesh">The sampled mesh.</param>
        private PincushionRenderer Create(Mesh mesh)
        {
            // Create a new object.
            GameObject go = new GameObject();
            // Match my transform.
            Transform t = transform;
            go.transform.position = t.position;
            go.transform.rotation = t.rotation;
            go.transform.localScale = t.localScale;
            
            go.AddComponent<MeshFilter>().mesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            // Set the material.
            mr.material = material;
            // Render.
            PincushionRenderer staticPointsRenderer = go.AddComponent<PincushionRenderer>();
            staticPointsRenderer.originalGameObject = gameObject;
            staticPointsRenderer.meshRenderer = mr;
            // Parent myself.
            t.parent = go.transform;
            return staticPointsRenderer;
        }
    }
}