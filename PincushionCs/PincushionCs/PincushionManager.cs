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
        /// This controls what gets occluded and what occludes.
        /// </summary>
        public PincushionOcclusionMode occlusionMode = PincushionOcclusionMode.Behind;
        /// <summary>
        /// Set the objects to this material.
        /// </summary>
        [HideInInspector]
        public Material material;
        public static int sourceMeshesLayer;
        public static int sampledMeshesLayer;
        private Camera mainCamera;
        private Camera distanceCamera;
        private int sourceMeshesCullingMask;
        private int sampledMeshesCullingMask;
        [SerializeField]
        private RenderTexture rt;
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
            // Prepare the Pincushion replacement shader.
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            // Set the material.
            if (occlusionMode == PincushionOcclusionMode.Behind)
            {
                // Prepare the distance shader.
                material = new Material(Shader.Find("Pincushion/Distance"));

                // Set the render texture.
                if (rt == null)
                {
                    rt = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.RFloat);
                }
                
                // Set the distance camera.
                if (distanceCamera == null)
                {
                    GameObject distanceCameraObject = new GameObject();
                    Transform distanceCameraTransform = distanceCameraObject.transform;
                    distanceCameraTransform.parent = mainCamera.transform;
                    distanceCameraTransform.localPosition = Vector3.zero;
                    distanceCameraTransform.localRotation = Quaternion.identity;
                    distanceCameraTransform.localScale = Vector3.one;
                    distanceCamera = distanceCameraObject.AddComponent<Camera>();
                    // Copy parameters.
                    distanceCamera.CopyFrom(mainCamera);
                    // Set the replacement shader.
                    distanceCamera.SetReplacementShader(Shader.Find("Pincushion/Distance"), "");
                    Shader.SetGlobalTexture("_PincushionDistanceTex", rt);
                    // Render to the texture.
                    distanceCamera.targetTexture = rt;
                }
                
                SetShader();

                // Set the culling masks.
                mainCamera.cullingMask = sampledMeshesCullingMask;
                distanceCamera.cullingMask = sourceMeshesCullingMask;
                
                // Set the main camera's replacement shader.
                mainCamera.SetReplacementShader(Shader.Find("Pincushion/PincushionReplacement"), "");
            }
            else
            {
                // Prepare the Pincushion shader.
                if (material == null)
                {
                    material = new Material(Shader.Find("Pincushion/Pincushion"));          
                }
                SetShader();
                if (occlusionMode == PincushionOcclusionMode.Backfacing)
                {
                    Shader.EnableKeyword("_OCCLUDE_BACKFACING");   
                }

                // Hide the distance camera.
                if (distanceCamera != null)
                {
                    distanceCamera.enabled = false;
                }
                // Set the main camera to see everything.
                mainCamera.cullingMask = ~0;
                // Remove the replacement shader.
                mainCamera.ResetReplacementShader();
            }

            // Get all renderers in the scene, including inactive ones.
            PincushionRenderer[] pincushions = FindObjectsOfType<PincushionRenderer>(true);
            bool showOriginalMeshes = occlusionMode == PincushionOcclusionMode.None ||
                                      occlusionMode == PincushionOcclusionMode.Behind;
            for (int i = 0; i < pincushions.Length; i++)
            {
                // Sample the mesh and apply rendering settings.
                pincushions[i].Sample();
                // Set visibility.
                pincushions[i].SetOriginalMeshVisibility(showOriginalMeshes);
                pincushions[i].SetSampledMeshVisibility(true);
            }
        }


        private void Awake()
        {
            // Set the layers. For now, we're using the names of some default layers.
            sourceMeshesLayer = LayerMask.NameToLayer("Default");
            sampledMeshesLayer = LayerMask.NameToLayer("TransparentFX");
            sourceMeshesCullingMask = 1 << sourceMeshesLayer;
            sampledMeshesCullingMask = 1 << sampledMeshesLayer;
            
            // Initialize the pincushions.
            PincushionRenderer[] pincushions = FindObjectsOfType<PincushionRenderer>(true);
            for (int i = 0; i < pincushions.Length; i++)
            {
                pincushions[i].Initialize();
            }
            
            // Initialize Pincushion.
            Sample();
        }

        /// <summary>
        /// Set the global shader values.
        /// </summary>
        private void SetShader()
        {
            Shader.SetGlobalColor("_PincushionColor", color);
            Shader.SetGlobalTexture("_PincushionMainTex", texture);
            Shader.SetGlobalFloat("_PincushionPointSize", pointRadius);
            if (constantScaling)
            {
                Shader.EnableKeyword("_CONSTANT_SCALING");
            }  
        }


        private void OnDestroy()
        {
            if (rt)
            {
                rt.Release();
            }
        }
    }
}