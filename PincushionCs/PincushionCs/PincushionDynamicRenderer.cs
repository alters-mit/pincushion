using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Sample points on a SkinnedMeshRenderer. Use a shader to move the points when the underlying rig moves.
    ///
    /// This component samples points exactly once.
    /// It's therefore not suitable for a mesh that is expected to deform by an extreme degree.
    /// Points are rendered using a geometry shader.
    /// This is relatively inefficient, but I haven't found an alternative that is compatible with the built-in render pipeline.
    /// </summary>
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class PincushionDynamicRenderer : PincushionRenderer<SkinnedMeshRenderer>
    {
        /// <summary>
        /// The color of each point.
        /// </summary>
        public Color color = Color.gray;
        /// <summary>
        /// The renderer. This is set on Awake().
        /// </summary>
        private SkinnedMeshRenderer skinnedMeshRenderer;
        /// <summary>
        /// The points' material. This is set on Awake().
        /// </summary>
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
    }
}