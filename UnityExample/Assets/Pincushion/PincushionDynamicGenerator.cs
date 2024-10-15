using System;
using System.Linq;
using UnityEngine;


namespace Pincushion
{
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class PincushionDynamicGenerator : PincushionGenerator
    {
        public Color color = Color.gray;
        public Texture2D texture;


        protected override void Awake()
        {
            SkinnedMeshRenderer skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            // Sample the points.
            Vector3[] points = skinnedMeshRenderer.sharedMesh.GetSampledPoints(pointsPerM);
            // Set the material.
            Material material = new Material(Shader.Find("Pincushion/DynamicPoints"));
            material.SetColor("_Color", color);
            material.SetTexture("_MainTex", texture);
            material.SetInt("_NumPoints", points.Length);
            material.SetFloat("_PointSize", pointRadius);
            material.SetVectorArray("_Positions", points.Select(p => (Vector4)p).ToArray());
            // Decide what to do with the material.
            if (mode == PincushionStaticCreationMode.create)
            {
                Create(points, material, skinnedMeshRenderer);
            }
            else if (mode == PincushionStaticCreationMode.replace)
            {
                skinnedMeshRenderer.sharedMesh.vertices = points;
                skinnedMeshRenderer.sharedMesh.SetPointTopology();
                skinnedMeshRenderer.material = material;
            }
            else if (mode == PincushionStaticCreationMode.createAndHideOriginal)
            {
                Create(points, material, skinnedMeshRenderer).SetOriginalVisibility(false);
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
        /// <param name="material">The point cloud material.</param>
        /// <param name="skinnedMeshRenderer">The original renderer.</param>
        private PincushionRenderer Create(Vector3[] points, Material material, SkinnedMeshRenderer skinnedMeshRenderer)
        {
            // Instantiate.
            GameObject go = new GameObject();
            // Match my transform.
            Transform t = transform;
            go.transform.position = t.position;
            go.transform.rotation = t.rotation;
            go.transform.localScale = t.localScale;

            // Set the renderer.
            SkinnedMeshRenderer smr = go.AddComponent<SkinnedMeshRenderer>();
            smr.quality = skinnedMeshRenderer.quality;
            smr.bones = skinnedMeshRenderer.bones;
            smr.rootBone = skinnedMeshRenderer.rootBone;
            smr.updateWhenOffscreen = skinnedMeshRenderer.updateWhenOffscreen;
            smr.skinnedMotionVectors = skinnedMeshRenderer.skinnedMotionVectors;
            smr.forceMatrixRecalculationPerRender = skinnedMeshRenderer.forceMatrixRecalculationPerRender;
            smr.material = material;
            
            // Set the mesh.
            Mesh mesh = new Mesh();
            mesh.vertices = points;
            mesh.SetPointTopology();
            smr.sharedMesh = mesh;

            // Render.
            PincushionRenderer pincushionRenderer = go.AddComponent<PincushionRenderer>();
            pincushionRenderer.originalGameObject = gameObject;
            pincushionRenderer.myRenderer = smr;
            // Parent myself.
            t.parent = go.transform;
            return pincushionRenderer;
        }
    }
}