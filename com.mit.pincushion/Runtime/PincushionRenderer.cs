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
        /// The shader keyword to apply as mask to the rendered points.
        /// </summary>
        private const string APPLY_MASK = "_PINCUSHION_APPLY_MASK";
        
        
        /// <summary>
        /// The random seed used to sample points.
        /// This changes every time points are resampled.
        /// </summary>
        public ulong seed;
        /// <summary>
        /// The material used to render the points.
        /// </summary>
        protected Material material;
        /// <summary>
        /// The object that renders the points.
        /// </summary>
        protected GameObject points;
        /// <summary>
        /// The renderer component.
        /// </summary>
        private Renderer myRenderer;
        /// <summary>
        /// The cached indices of `mask` in a random order.
        /// </summary>
        private UIntPtr[] maskIndices;
        /// <summary>
        /// This will be sent to the shader to mask some values.
        /// The values will be either 0 (false) or 1 (true).
        /// </summary>
        private uint[] mask;
        /// <summary>
        /// The compute buffer containing the `mask`.
        /// </summary>
        private ComputeBuffer maskBuffer;


        /// <summary>
        /// Sample points and set the mesh(es).
        /// </summary>
        /// <param name="cam">The main camera.</param>
        public void Sample(Camera cam)
        {
            PincushionManager instance = PincushionManager.Instance;
            float pointsPerM = instance.pointsPerM;
            
            // Multiply the number of points.
            if (instance.multiplyPointsPerMByObjectScale)
            {
                pointsPerM *= transform.localScale.magnitude;
            }
            if (instance.multiplyPointsPerMByCameraDistance)
            {
                pointsPerM *= 1f / (0.1f * Vector3.Distance(cam.transform.position, transform.position));
            }
            
            // Sample the points.
            int numSampledPoints = SampleMesh(pointsPerM);
            
            // Allocate the mask arrays.
            mask = new uint[numSampledPoints];
            maskIndices = new UIntPtr[numSampledPoints];
            if (maskBuffer != null)
            {
                maskBuffer.Release();
            }
            // Allocate and set the mask buffer.
            maskBuffer = new ComputeBuffer(numSampledPoints, 4);
            material.SetBuffer(PincushionManager.maskId, maskBuffer);
            
            // Set the mask indices.
            unsafe
            {
                UIntPtr num = (UIntPtr)maskIndices.Length;
                fixed (UIntPtr* maskIndicesPtr = maskIndices)
                {
                    Vec_size_t indices = new Vec_size_t
                    {
                        ptr = maskIndicesPtr,
                        len = num,
                        cap = num
                    };
                    Ffi.set_mask_indices(&indices, seed);
                }
            }
        }


        /// <summary>
        /// Toggle the visibility of the original mesh.
        /// </summary>
        /// <param name="visible">If true, the mesh will be visible.</param>
        public void SetSourceMeshVisibility(bool visible)
        {
            myRenderer.enabled = visible;
        }
        
        
        /// <summary>
        /// Toggle the visibility of the sampled mesh(es).
        /// </summary>
        /// <param name="visible">If true, the mesh(es) will be visible.</param>
        public void SetSampledMeshVisibility(bool visible)
        {
            points.SetActive(visible);
        }


        /// <summary>
        /// Initialize the renderer. This is called by PincushionManager.
        /// </summary>
        public virtual void Initialize()
        {
            myRenderer = GetComponent<Renderer>();

            // Create the points object.
            points = new GameObject();
            Transform t = points.transform;
            t.parent = transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            Vector3 s = t.parent.localScale;
            t.localScale = new Vector3(1 / s.x, 1 / s.y, 1 / s.z);
            
            // Set the layers.
            myRenderer.gameObject.layer = PincushionManager.sourceMeshesLayer;
            points.gameObject.layer = PincushionManager.sampledMeshesLayer;
            points.name = "Sampled Mesh";
            
            // Set the material.
            material = new Material(Shader.Find("Pincushion/Pincushion"));
        }


        /// <summary>
        /// Apply a rendering mask.
        /// The number of visible points will be `vertices.Length * factor`
        /// </summary>
        /// <param name="factor">A factor between 0 and 1.</param>
        public void SetMask(float factor)
        {
            material.EnableKeyword(APPLY_MASK);
            unsafe
            {
                UIntPtr num = (UIntPtr)maskIndices.Length;
                fixed (UIntPtr* maskIndicesPtr = maskIndices)
                {
                    Vec_size_t indices = new Vec_size_t
                    {
                        ptr = maskIndicesPtr,
                        len = num,
                        cap = num
                    };
                    fixed (uint* maskPtr = mask)
                    {
                        Vec_uint32_t maskV = new Vec_uint32_t
                        {
                            ptr = maskPtr,
                            len = num,
                            cap = num
                        };
                        Ffi.set_mask(factor, &indices, &maskV);         
                    }
                }
            }
            maskBuffer.SetData(mask);
        }


        
        /// <summary>
        /// Disable the rendering mask.
        /// </summary>
        public void ShowAll()
        {
            material.DisableKeyword(APPLY_MASK);
        }


        /// <summary>
        /// Returns a mesh's points, transformed by the transform matrix.
        /// </summary>
        public Vector3[] GetTransformedPoints()
        {
            return GetSampledMesh().GetTransformedPoints(transform.localToWorldMatrix);
        }


        /// <summary>
        /// Returns the number of sampled points.
        /// </summary>
        public int GetNumSampledPoints()
        {
            return GetSampledMesh().vertexCount;
        }


        /// <summary>
        /// Returns the mesh of sampled points.
        /// </summary>
        /// <returns></returns>
        protected abstract Mesh GetSampledMesh();
        

        /// <summary>
        /// Sample points, create the sampled mesh, and set the material.
        /// </summary>
        /// <param name="pointsPerM">The number of points per square meter.</param>
        protected abstract int SampleMesh(float pointsPerM);
        

        private void OnDestroy()
        {
            if (maskBuffer != null)
            {
                maskBuffer.Release();             
            }
        }
    }
}