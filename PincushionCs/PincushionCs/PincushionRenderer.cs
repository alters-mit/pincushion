using System;
using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// This script creates or replaces a mesh with sampled points.
    /// </summary>
    public abstract class PincushionRenderer<T> : MonoBehaviour where T: Component
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
        public PincushionCreationMode mode = PincushionCreationMode.replace;

        
        private void Awake()
        {
            // Get the renderer.
            T meshContainer = GetComponent<T>();
            Mesh mesh = GetMesh(meshContainer);
            
            
            // Decide what to do with the material and points.
            if (mode == PincushionCreationMode.create)
            {
                Create(mesh);
            }
            else if (mode == PincushionCreationMode.replace)
            {
                ReplaceMesh(meshContainer, mesh);
            }
            else if (mode == PincushionCreationMode.createAndHideOriginal)
            {
                Create(mesh).SetOriginalVisibility(false);
            }
            else
            {
                throw new Exception("Invalid mode: " + mode);
            }
        }

        private PincushionVisibilityToggler Create(Mesh mesh)
        {
            // Instantiate.
            GameObject go = new GameObject();
            // Match my transform.
            Transform t = transform;
            go.transform.position = t.position;
            go.transform.rotation = t.rotation;
            go.transform.localScale = t.localScale;
            
            // Assign the mesh.
            T meshContainer = go.AddComponent<T>();
            ReplaceMesh(meshContainer, mesh);
            
            // Render.
            PincushionVisibilityToggler pincushionVisibilityToggler = go.AddComponent<PincushionVisibilityToggler>();
            pincushionVisibilityToggler.originalGameObject = gameObject;
            pincushionVisibilityToggler.myRenderer = SetCreatedMesh(meshContainer);
            // Parent myself.
            t.parent = go.transform;
            return pincushionVisibilityToggler;
        }


        protected abstract Mesh GetMesh(T meshContainer);


        protected abstract void ReplaceMesh(T meshContainer, Mesh mesh);


        protected abstract Material GetMaterial();


        protected abstract Renderer SetCreatedMesh(T meshContainer);
    }
}