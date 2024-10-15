using System;
using System.Linq;
using UnityEngine;


namespace Pincushion
{
    /// <summary>
    /// Mesh extensions that enabled the usage of points sampled by Pincushion.
    /// </summary>
    public static class MeshExtensions
    {
        /// <summary>
        /// Uniformly sample points on a mesh.
        /// </summary>
        /// <param name="mesh">(this)</param>
        /// <param name="pointsPerM">The number of points per square meter.</param>
        public static Vector3[] GetSampledPoints(this Mesh mesh, float pointsPerM) 
        {
            // Get the casted indices.
            UIntPtr[] indices = Array.ConvertAll(mesh.triangles, intToUIntPtr);
            unsafe
            {
                // All `fixed` statements are boilerplate C#-to-Rust declarations.
                fixed (Vector3* verticesPointerVec3 = mesh.vertices)
                {
                    UIntPtr verticesLength = (UIntPtr)(mesh.vertices.Length * 3);
                    Vec_float_t vertices = new Vec_float_t
                    {
                        ptr = (float*)verticesPointerVec3,
                        len = verticesLength,
                        cap = verticesLength
                    };
                    int numTriangles = mesh.triangles.Length / 3;
                    // Allocate an array of areas.
                    float[] areas = new float[numTriangles];
                    UIntPtr areasLength = (UIntPtr)numTriangles;
                    fixed (float* areasPointer = areas)
                    {
                        Vec_float_t areasVec = new Vec_float_t
                        {
                            ptr = areasPointer,
                            len = areasLength,
                            cap = areasLength
                        };
                        UIntPtr indicesLength = (UIntPtr)indices.Length;
                        fixed (UIntPtr* indicesPointer = indices)
                        {
                            Vec_size_t triangles = new Vec_size_t
                            {
                                ptr = indicesPointer,
                                len = indicesLength,
                                cap = indicesLength
                            };

                            // Get the areas and the total area.
                            float totalArea = Ffi.get_areas(&vertices, &triangles, &areasVec);
                            // Get the number of points.
                            int numPoints = (int)Ffi.get_num_points(totalArea, pointsPerM);
                            // Allocate the array.
                            Vector3[] points = new Vector3[numPoints];
                            UIntPtr pointsLength = (UIntPtr)(numPoints * 3);
                            // Sample the points.
                            fixed (Vector3* pointsPointer = points) 
                            {
                                Vec_float_t pointsVec = new Vec_float_t()
                                {
                                    ptr = (float*)pointsPointer,
                                    len = pointsLength,
                                    cap = pointsLength
                                };
                                Ffi.sample_points(&vertices, &triangles, &areasVec, totalArea, &pointsVec);
                            }
                            return points;
                        }
                    }
                }
            }
        }
        
        
        /// <summary>
        /// Uniformly sample points on a mesh.
        /// Convert the points to icosahedra.
        /// Combine the icosahedra into a single mesh.
        /// </summary>
        /// <param name="mesh">(this)</param>
        /// <param name="pointsPerM">The number of points per square meter.</param>
        /// <param name="radius">The radius of each icosahedron.</param>
        public static Mesh GetIcosahedra(this Mesh mesh, float pointsPerM, float radius) 
        {
            // Get the casted indices.
            UIntPtr[] indices = Array.ConvertAll(mesh.triangles, intToUIntPtr);
            unsafe
            {
                // All `fixed` statements are boilerplate C#-to-Rust declarations.
                fixed (Vector3* verticesPointerVec3 = mesh.vertices)
                {
                    UIntPtr verticesLength = (UIntPtr)(mesh.vertices.Length * 3);
                    Vec_float_t vertices = new Vec_float_t
                    {
                        ptr = (float*)verticesPointerVec3,
                        len = verticesLength,
                        cap = verticesLength
                    };
                    int numTriangles = mesh.triangles.Length / 3;
                    // Allocate an array of areas.
                    float[] areas = new float[numTriangles];
                    UIntPtr areasLength = (UIntPtr)numTriangles;
                    fixed (float* areasPointer = areas)
                    {
                        Vec_float_t areasVec = new Vec_float_t
                        {
                            ptr = areasPointer,
                            len = areasLength,
                            cap = areasLength
                        };
                        UIntPtr indicesLength = (UIntPtr)indices.Length;
                        fixed (UIntPtr* indicesPointer = indices)
                        {
                            Vec_size_t triangles = new Vec_size_t
                            {
                                ptr = indicesPointer,
                                len = indicesLength,
                                cap = indicesLength
                            };

                            // Get the areas and the total area.
                            float totalArea = Ffi.get_areas(&vertices, &triangles, &areasVec);
                            // Get the number of points.
                            int numPoints = (int)Ffi.get_num_points(totalArea, pointsPerM);
                            // Allocate the icosahedra arrays.
                            Vector3[] icosahedraVertices = new Vector3[numPoints * 12];
                            UIntPtr numIcosahedraVertices = (UIntPtr)(icosahedraVertices.Length * 3);
                            UIntPtr[] icosahedraIndices = new UIntPtr[numPoints * 60];
                            UIntPtr numIcosahedraIndices = (UIntPtr)(icosahedraIndices.Length * 3);
                            // Allocate the array.
                            Vector3[] points = new Vector3[numPoints];
                            UIntPtr pointsLength = (UIntPtr)(numPoints * 3);
                            // Sample the points.
                            fixed (Vector3* pointsPointer = points, icosahedraVerticesPointer = icosahedraVertices) 
                            {
                                Vec_float_t pointsVec = new Vec_float_t
                                {
                                    ptr = (float*)pointsPointer,
                                    len = pointsLength,
                                    cap = pointsLength
                                };
                                Vec_float_t icosahedraVerticesVec = new Vec_float_t
                                {
                                    ptr = (float*)icosahedraVerticesPointer,
                                    len = numIcosahedraVertices,
                                    cap = numIcosahedraVertices
                                };
                                fixed (UIntPtr* icosahedraIndicesPointer = icosahedraIndices)
                                {
                                    Vec_size_t icosahedraIndicesVec = new Vec_size_t
                                    {
                                        ptr = icosahedraIndicesPointer,
                                        len = numIcosahedraIndices,
                                        cap = numIcosahedraIndices
                                    };
                                    
                                    // Sample the points and get spheres.
                                    Ffi.points_to_icosahedra(&vertices, &triangles, &areasVec,
                                        totalArea, radius, &pointsVec, 
                                        &icosahedraVerticesVec, &icosahedraIndicesVec);
                                    
                                    // Build the mesh.
                                    Mesh pointsMesh = new Mesh();
                                    pointsMesh.vertices = icosahedraVertices;
                                    pointsMesh.triangles = Array.ConvertAll(icosahedraIndices, uIntPtrToInt);
                                    pointsMesh.RecalculateNormals();
                                    pointsMesh.RecalculateTangents();
                                    pointsMesh.RecalculateBounds();
                                    return pointsMesh;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        
        /// <summary>
        /// Set the mesh topology of the sampled points.
        /// </summary>
        /// <param name="mesh">(this)</param>
        public static void SetPointTopology(this Mesh mesh)
        {
            int length = mesh.vertices.Length;
            mesh.SetIndices(Enumerable.Range(0, length).ToArray(), 0, length, MeshTopology.Points, 0);
        }



        private static UIntPtr intToUIntPtr(int i)
        {
            return (UIntPtr)i;
        }
        
        
        private static int uIntPtrToInt(UIntPtr i)
        {
            return (int)i;
        }
    } 
}