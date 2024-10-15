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
        /// What to do with the points once they've been sampled.
        /// </summary>
        public CreationMode mode = CreationMode.replace;
        /// <summary>
        /// The points will be rendered with this material.
        /// </summary>
        public Material material;


        private void Start()
        {
            // Get the underlying mesh.
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            SkinnedMeshRenderer skinnedMeshRenderer;
            Mesh pointsMesh;
            bool isStatic;
            if (meshFilter == null)
            {
                skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
                pointsMesh = skinnedMeshRenderer.sharedMesh.GetIcosahedra(pointsPerM, pointRadius);
                isStatic = true;
            }
            else
            {
                skinnedMeshRenderer = null;
                pointsMesh = meshFilter.mesh.GetIcosahedra(pointsPerM, pointRadius);
                isStatic = false;
            }

            // Decide what to do with the material and points.
            if (mode == CreationMode.create)
            {
                Create(pointsMesh, isStatic);
            }
            else if (mode == CreationMode.replace)
            {
                if (meshFilter != null)
                {
                    meshFilter.mesh = pointsMesh;
                    GetComponent<Renderer>().material = material;
                }
                else if (skinnedMeshRenderer != null)
                {
                    skinnedMeshRenderer.sharedMesh = pointsMesh;
                    skinnedMeshRenderer.material = material;
                }
            }
            else if (mode == CreationMode.createAndHideOriginal)
            {
                Create(pointsMesh, isStatic).SetOriginalVisibility(false);
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
        /// <param name="isStaticMesh">If true, this is a static mesh (MeshRenderer). If false, this is a dynamic mesh (SkinnedMeshRenderer).</param>
        private PincushionRenderer Create(Mesh mesh, bool isStaticMesh)
        {
            // Create a copy of this object.
            GameObject go = Instantiate(gameObject);
            // Prevent an infinite loop.
            Destroy(go.GetComponent<PincushionGenerator>());
            // Match my transform.
            Transform t = transform;
            go.transform.position = t.position;
            go.transform.rotation = t.rotation;
            go.transform.localScale = t.localScale;

            Renderer r;
            if (isStaticMesh)
            {
                go.GetComponent<MeshFilter>().mesh = mesh;
                r = go.GetComponent<MeshRenderer>();      
            }
            else
            {
                r = go.GetComponent<SkinnedMeshRenderer>();
                ((SkinnedMeshRenderer)r).sharedMesh = mesh;
            }

            // Set the material.
            r.material = material;
            
            // Render.
            PincushionRenderer pincushionRenderer = go.AddComponent<PincushionRenderer>();
            pincushionRenderer.originalGameObject = gameObject;
            pincushionRenderer.myRenderer = r;
            // Parent myself.
            t.parent = go.transform;
            return pincushionRenderer;
        }
    }
}