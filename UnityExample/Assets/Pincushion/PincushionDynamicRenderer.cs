using UnityEngine;


namespace Pincushion
{
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class PincushionDynamicRenderer : PincushionRenderer<SkinnedMeshRenderer>
    {
        public Color color = Color.gray;
        private SkinnedMeshRenderer skinnedMeshRenderer;
        private Material material;


        protected override Mesh GetMesh(SkinnedMeshRenderer meshContainer)
        {
            skinnedMeshRenderer = meshContainer;
            
            // Create the mesh.
            Mesh mesh = Instantiate(skinnedMeshRenderer.sharedMesh);
            Vector3[] points = mesh.GetSampledPoints(pointsPerM);
            mesh.triangles = new int[points.Length * 3];
            mesh.vertices = points;
            mesh.SetPointTopology();
            
            // Set the material.
            material = new Material(Shader.Find("Pincushion/DynamicPoints"));
            material.SetColor("_Color", color); ;
            material.SetFloat("_PointSize", pointRadius);
            skinnedMeshRenderer.material = material;

            return mesh;
        }


        protected override void ReplaceMesh(SkinnedMeshRenderer meshContainer, Mesh mesh)
        {
            meshContainer.sharedMesh = mesh;
        }


        protected override Renderer SetCreatedMesh(SkinnedMeshRenderer meshContainer)
        {
            // Copy values from my renderer.
            meshContainer.quality = skinnedMeshRenderer.quality;
            meshContainer.bones = skinnedMeshRenderer.bones;
            meshContainer.rootBone = skinnedMeshRenderer.rootBone;
            meshContainer.updateWhenOffscreen = skinnedMeshRenderer.updateWhenOffscreen;
            meshContainer.skinnedMotionVectors = skinnedMeshRenderer.skinnedMotionVectors;
            meshContainer.forceMatrixRecalculationPerRender = skinnedMeshRenderer.forceMatrixRecalculationPerRender;
            meshContainer.material = material;

            return meshContainer;
        }


        protected override Material GetMaterial()
        {
            return material;
        }
    }
}