using System;
using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Abstract base class that creates or replaces a mesh with sampled points.
    /// </summary>
    public abstract class PincushionRenderer : MonoBehaviour
    {
        /// <summary>
        /// The color of each point.
        /// </summary>
        public Color color = Color.white;
        /// <summary>
        /// Use this texture to render each point.
        /// </summary>
        public Texture2D texture;
        /// <summary>
        /// The number of points per square meter.
        /// </summary>
        public float pointsPerM = 80f;
        /// <summary>
        /// The radius of each point in meters.
        /// </summary>
        public float pointRadius = 0.02f;
        /// <summary>
        /// If true, hide points facing away from the camera.
        /// </summary>
        public bool occludeBackFacing = true;
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
        /// Toggles whether to show the original mesh on awake.
        /// </summary>
        public bool showOriginalMesh;
        /// <summary>
        /// Toggles whether to show the sampled mesh on awake.
        /// </summary>
        public bool showSampledMesh = true;
        /// <summary>
        /// The object that renders the points.
        /// </summary>
        protected GameObject points;


        private void Awake()
        {
            Initialize();
            Set();
            SetOriginalMeshVisibility(showOriginalMesh);
            SetSampledMeshVisibility(showSampledMesh);
        }


        /// <summary>
        /// Sample points and set the mesh(es).
        /// </summary>
        public void Set()
        {
            // Create the points object.
            points = new GameObject();
            Transform t = points.transform;
            t.parent = transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            
            // Scale the number of points.
            if (scalePointsPerMByCameraDistance)
            {
                pointsPerM *= 1f / (0.1f * Vector3.Distance(Camera.main.transform.position, transform.position));
            }
            
            SetMesh();
        }


        protected abstract void Initialize();


        protected abstract void SetMesh();

        
        /// <summary>
        /// Toggle the visibility of the original mesh.
        /// </summary>
        /// <param name="visible">If true, the mesh will be visible.</param>
        public abstract void SetOriginalMeshVisibility(bool visible);
        
        
        /// <summary>
        /// Toggle the visibility of the sampled mesh(es).
        /// </summary>
        /// <param name="visible">If true, the mesh(es) will be visible.</param>
        public void SetSampledMeshVisibility(bool visible)
        {
            points.SetActive(visible);
        }


        protected Material GetMaterial()
        {
            Material material = new Material(Shader.Find("Pincushion/Pincushion"));
            material.SetColor("_Color", color);
            material.SetTexture("_MainTex", texture);
            material.SetFloat("_PointSize", pointRadius);
            if (occludeBackFacing)
            {
                material.EnableKeyword("_OCCLUDE_BACKFACING");   
            }
            if (constantScaling)
            {
                material.EnableKeyword("_CONSTANT_SCALING");   
            }
            return material;
        }
    }
}