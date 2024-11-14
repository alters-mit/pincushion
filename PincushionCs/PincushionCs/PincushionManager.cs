using System.Linq;
using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Singleton manager class for Pincushion.
    /// To use Pincushion, add this to the scene, assign mainCamera, and set other parameters as-desired.
    /// See README for more information.
    /// </summary>
    public class PincushionManager : MonoBehaviour
    {
        /// <summary>
        /// The shader keyword corresponding the render mode HideBackfacing.
        /// </summary>
        private const string OCCLUDE_BACKFACING = "_OCCLUDE_BACKFACING";
        /// <summary>
        /// The shader keyword corresponding the render mode OccludeBehind.
        /// </summary>
        private const string OCCLUDE_BEHIND = "_OCCLUDE_BEHIND";


        /// <summary>
        /// The main camera used for viewing the sampled points.
        /// </summary>
        [Header("Camera")]
        public Camera mainCamera;
        /// <summary>
        /// If true, set the background color to Background Color.
        /// </summary>
        public bool setBackgroundColor = true;
        /// <summary>
        /// The solid color of the background.
        /// </summary>
        public Color backgroundColor = Color.black;
        /// <summary>
        /// All source mesh objects will be set to this layer.
        /// </summary>
        public string sourceMeshesLayerName = "Default";
        /// <summary>
        /// All sampled mesh objects will be set to this layer.
        /// </summary>
        public string sampledMeshesLayerName = "TransparentFX";
        /// <summary>
        /// The number of points per square meter.
        /// </summary>
        [Header("Sampling")] 
        public float pointsPerM = 80f;
        /// <summary>
        /// If true, multiply the number of points by the object's initial distance from the camera.
        /// </summary>
        public bool multiplyPointsPerMByCameraDistance;
        /// <summary>
        /// If true, multiply the number of points by the object's initial uniform scale.
        /// </summary>
        public bool multiplyPointsPerMByObjectScale = true;
        /// <summary>
        /// This controls how Pincushion is rendered.
        /// </summary>
        [Header("Rendering")]
        public PincushionRenderMode renderMode = PincushionRenderMode.OccludeBehind;
        /// <summary>
        /// The texture used to render the points.
        /// Can be null, in which case a default texture is used.
        /// </summary>
        public Texture2D texture;
        /// <summary>
        /// The color of each point.
        /// </summary>
        public Color color = Color.white;
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
        /// If true, show every nth point.
        /// </summary>
        public bool showEveryNth;
        /// <summary>
        /// A factor between 0 and 1 that controls how many points will be skipped when rendering.
        /// </summary>
        [Range(0, 1)]
        public float nthFactor = 1;
        /// <summary>
        /// The source meshes' layer.
        /// </summary>
        public static int sourceMeshesLayer;
        /// <summary>
        /// The sampled meshes' layer.
        /// </summary>
        public static int sampledMeshesLayer;
        /// <summary>
        /// The shader property ID for showing every nth point.
        /// </summary>
        public static int nthMaskId;
        /// <summary>
        /// A layer mask for culling everything except the source meshes.
        /// </summary>
        private int sourceMeshesCullingMask;
        /// <summary>
        /// A layer mask for culling everything except the sample meshes.
        /// </summary>
        private int sampledMeshesCullingMask;
        /// <summary>
        /// The camera used for getting the distance of everything in the scene.
        /// </summary>
        private Camera distanceCamera;
        /// <summary>
        /// The distance camera will render to this texture.
        /// </summary>
        private RenderTexture rt;
        /// <summary>
        /// The original clear flags of the camera.
        /// </summary>
        private CameraClearFlags mainCameraClearFlags;
        /// <summary>
        /// Singleton instance. Never call this directly!
        /// </summary>
        private static PincushionManager _instance;
        /// <summary>
        /// Singleton instance.
        /// </summary>
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


        /// <summary>
        /// Initialize or reinitialize Pincushion.
        ///
        /// - Set the rendering parameters depending on the render mode.
        /// - Sample the meshes
        /// </summary>
        public void Set()
        {
            // Load a default texture.
            if (texture == null)
            {
                texture = Resources.Load<Texture2D>("pincushion_point");
            }
            
            // Set global shader properties.
            SetShader();
            
            // Set the background.
            SetPincushionBackground();

            // Show the source meshes.
            if (renderMode == PincushionRenderMode.DoNot)
            {
                // Set the main camera to see everything.
                mainCamera.cullingMask = ~0;
                // Remove the replacement shader.
                mainCamera.ResetReplacementShader();
                // Restore the clear flags.
                mainCamera.clearFlags = mainCameraClearFlags;
            }
            else if (renderMode == PincushionRenderMode.OccludeBehind)
            {
                // Set the render texture.
                if (rt == null)
                {
                    // The render texture is used to store the distance of every pixel from the camera.
                    // Therefore, we only need a single 32bit channel.
                    rt = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.RFloat);
                }
                
                // Set the distance camera.
                if (distanceCamera == null)
                {
                    GameObject distanceCameraObject = new GameObject();
                    distanceCameraObject.name = "Distance Camera";
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
                
                // Set the culling masks.
                mainCamera.cullingMask = sampledMeshesCullingMask;
                distanceCamera.cullingMask = sourceMeshesCullingMask;
            }
            else
            {
                // Hide the distance camera.
                if (distanceCamera != null)
                {
                    distanceCamera.enabled = false;
                }

                // Set the main camera to see everything.
                mainCamera.cullingMask = ~0;
            }
            
            nthMaskId = Shader.PropertyToID("_PincushionNthMask");

            // Set or unset shader keywords depending on the render mode.
            if (renderMode == PincushionRenderMode.HideBackfacing)
            {
                Shader.EnableKeyword(OCCLUDE_BACKFACING);   
            }
            else
            {
                Shader.DisableKeyword(OCCLUDE_BACKFACING);  
            }
            if (renderMode == PincushionRenderMode.OccludeBehind)
            {
                Shader.EnableKeyword(OCCLUDE_BEHIND);
            }
            else
            {
                Shader.DisableKeyword(OCCLUDE_BEHIND);  
            }

            // Decide which meshes to render.
            bool showSourceMeshes = renderMode == PincushionRenderMode.DoNot || 
                                    renderMode == PincushionRenderMode.WithSourceMeshes ||
                                    renderMode == PincushionRenderMode.OccludeBehind;
            bool showSampledMeshes = renderMode != PincushionRenderMode.DoNot;
            // Find the pincushions, including those that are inactive.
            PincushionRenderer[] pincushions = FindObjectsOfType<PincushionRenderer>(true);
            for (int i = 0; i < pincushions.Length; i++)
            {
                // Sample the mesh and apply rendering settings.
                pincushions[i].Sample(mainCamera);
                // Set visibility.
                pincushions[i].SetSourceMeshVisibility(showSourceMeshes);
                pincushions[i].SetSampledMeshVisibility(showSampledMeshes);

                if (showEveryNth)
                {
                    pincushions[i].ShowEveryNth(nthFactor);
                }
                else
                {
                    pincushions[i].ShowAll();
                }
            }
        }


        /// <summary>
        /// Show every nth point.
        /// </summary>
        public void ShowEveryNthPoint()
        {
            PincushionRenderer[] pincushions = FindObjectsOfType<PincushionRenderer>(true);
            for (int i = 0; i < pincushions.Length; i++)
            {
                if (showEveryNth)
                {
                    pincushions[i].ShowEveryNth(nthFactor);
                }
                else
                {
                    pincushions[i].ShowAll();
                }
            }
        }


        private void Awake()
        {
            // Store the clear flags.
            mainCameraClearFlags = mainCamera.clearFlags;

            // Set the layers. For now, we're using the names of some default layers.
            sourceMeshesLayer = LayerMask.NameToLayer(sourceMeshesLayerName);
            sampledMeshesLayer = LayerMask.NameToLayer(sampledMeshesLayerName);
            sourceMeshesCullingMask = 1 << sourceMeshesLayer;
            sampledMeshesCullingMask = 1 << sampledMeshesLayer;

            // Get all renderers in the source meshes layer and add Pincushion renderers.
            foreach (Renderer r in FindObjectsOfType<Renderer>().Where(r => r.gameObject.layer == sourceMeshesLayer))
            {
                if (r is MeshRenderer)
                {
                    PincushionMeshRenderer pincushion = r.gameObject.AddComponent<PincushionMeshRenderer>();
                    // Initialize the renderer.
                    pincushion.Initialize();
                }
                else if (r is SkinnedMeshRenderer)
                {
                    PincushionSkinnedMeshRenderer pincushion = r.gameObject.AddComponent<PincushionSkinnedMeshRenderer>();
                    // Initialize the renderer.
                    pincushion.Initialize();
                }
            }

            // Initialize Pincushion.
            Set();
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
            else
            {
                Shader.DisableKeyword("_CONSTANT_SCALING");
            }
        }


        /// <summary>
        /// Set a solid background color.
        /// </summary>
        private void SetPincushionBackground()
        {
            if (setBackgroundColor)
            {
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = backgroundColor;
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