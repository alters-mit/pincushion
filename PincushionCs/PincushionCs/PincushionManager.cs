using UnityEngine;

namespace Pincushion
{
    public class PincushionManager : MonoBehaviour
    {
        /// <summary>
        /// The texture used to render the points.
        /// Can be null, in which case a default texture is used.
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
        /// If true, points will always render at the same size, regardless of distance.
        /// If false, scale the points normally. 
        /// </summary>
        public bool constantScaling;
        /// <summary>
        /// Increase the number of points on closer objects. 
        /// </summary>
        public bool scalePointsPerMByCameraDistance;
        /// <summary>
        /// The color of each point.
        /// </summary>
        public Color color = Color.white;
        /// <summary>
        /// Toggles whether to show the original meshes on awake.
        /// </summary>
        public bool showOriginalMeshes;
        /// <summary>
        /// Toggles whether to show the sampled meshes on awake.
        /// </summary>
        public bool showSampledMeshes = true;
        /// <summary>
        /// This controls what gets occluded and what occludes.
        /// </summary>
        public PincushionOcclusionMode occlusionMode = PincushionOcclusionMode.SourceMesh;
        /// <summary>
        /// Set the objects to this material.
        /// </summary>
        [HideInInspector]
        public Material material;
        private static PincushionManager _instance;
        public static PincushionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PincushionManager>();
                }
                return _instance;
            }
        }


        public void Sample()
        {
            // Load a default texture.
            if (texture == null)
            {
                texture = Resources.Load<Texture2D>("pincushion_point");
            }
            // Set the material.
            if (occlusionMode == PincushionOcclusionMode.SourceMesh)
            {
                // Prepare the distance shader.
                material = new Material(Shader.Find("Pincushion/Distance"));
            }
            else
            {
                material = new Material(Shader.Find("Pincushion/Pincushion"));
                material.SetColor("_Color", color);
                material.SetTexture("_MainTex", texture);
                material.SetFloat("_PointSize", pointRadius);
                if (occlusionMode == PincushionOcclusionMode.Backfacing)
                {
                    material.EnableKeyword("_OCCLUDE_BACKFACING");   
                }
                if (constantScaling)
                {
                    material.EnableKeyword("_CONSTANT_SCALING");   
                }          
            }

            // Get all renderers in the scene, including inactive ones.
            PincushionRenderer[] pincushions = FindObjectsOfType<PincushionRenderer>(true);
            // Set visibility.
            for (int i = 0; i < pincushions.Length; i++)
            {
                pincushions[i].SetOriginalMeshVisibility(showOriginalMeshes);
                pincushions[i].SetSampledMeshVisibility(showSampledMeshes);
            }
        }
    }
}