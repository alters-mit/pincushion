using System;
using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Generate sampled points.
    /// Either set this mesh's vertices and rendering parameters to show the mesh or create a new mesh + parameters.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Renderer))]
    public class PincushionStaticGenerator : MonoBehaviour
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
        /// What to do with the points once they've been sampled.
        /// </summary>
        public PincushionStaticCreationMode mode = PincushionStaticCreationMode.replace;
        /// <summary>
        /// The points will be rendered with this material.
        /// </summary>
        public Material material;


        private void Start()
        {
            // Get the underlying mesh.
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            Mesh mesh = meshFilter.mesh.GetIcosahedra(pointsPerM, pointRadius);

            // Decide what to do with the material and points.
            if (mode == PincushionStaticCreationMode.create)
            {
                Create(mesh);
            }
            else if (mode == PincushionStaticCreationMode.replace)
            {
                meshFilter.mesh = mesh;
                GetComponent<Renderer>().material = material;
            }
            else if (mode == PincushionStaticCreationMode.createAndHideOriginal)
            {
                Create(mesh).SetOriginalVisibility(false);
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
        private PincushionStaticRenderer Create(Mesh mesh)
        {
            // Instantiate.
            GameObject go = new GameObject();
            // Match my transform.
            Transform t = transform;
            go.transform.position = t.position;
            go.transform.rotation = t.rotation;
            go.transform.localScale = t.localScale;

            go.AddComponent<MeshFilter>().mesh = mesh;
            MeshRenderer r = go.AddComponent<MeshRenderer>();
            // Set the material.
            r.material = material;
            
            // Render.
            PincushionStaticRenderer pincushionRenderer = go.AddComponent<PincushionStaticRenderer>();
            pincushionRenderer.originalGameObject = gameObject;
            pincushionRenderer.myRenderer = r;
            // Parent myself.
            t.parent = go.transform;
            return pincushionRenderer;
        }
    }
}