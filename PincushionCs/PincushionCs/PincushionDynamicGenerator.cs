using System;
using System.Linq;
using UnityEngine;


namespace Pincushion
{
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class PincushionDynamicGenerator : PincushionGenerator
    {
        public Color color = Color.gray;


        protected override void Awake()
        {
            SkinnedMeshRenderer skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            // Sample the points.
            Mesh mesh = Instantiate(skinnedMeshRenderer.sharedMesh);
            Vector3[] points = mesh.GetSampledPoints(pointsPerM);
            mesh.triangles = new int[points.Length * 3];
            mesh.vertices = points;
            mesh.SetPointTopology();
            skinnedMeshRenderer.sharedMesh = mesh;
            // Set the material.
            Material material = new Material(Shader.Find("Pincushion/DynamicPoints"));
            material.SetColor("_Color", color); ;
            material.SetFloat("_PointSize", pointRadius);
            // Decide what to do with the material.
            if (mode == PincushionStaticCreationMode.create)
            {
                Create(material, skinnedMeshRenderer);
            }
            else if (mode == PincushionStaticCreationMode.replace)
            {
                skinnedMeshRenderer.material = material;
            }
            else if (mode == PincushionStaticCreationMode.createAndHideOriginal)
            {
                Create(material, skinnedMeshRenderer).SetOriginalVisibility(false);
            }
            else
            {
                throw new Exception("Invalid mode: " + mode);
            }
        }
        
        
        /// <summary>
        /// Create a new object to render the sampled points.
        /// </summary>
        /// <param name="material">The point cloud material.</param>
        /// <param name="skinnedMeshRenderer">The original renderer.</param>
        private PincushionRenderer Create(Material material, SkinnedMeshRenderer skinnedMeshRenderer)
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
            smr.sharedMesh = skinnedMeshRenderer.sharedMesh;

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