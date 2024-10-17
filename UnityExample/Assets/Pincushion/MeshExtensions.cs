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

        public static UIntPtr[] GetSampledTriangles(this Mesh mesh, float pointsPerM)
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
                            UIntPtr[] sampledTriangles = new UIntPtr[numPoints * 3];
                            UIntPtr sampledTrianglesLength = (UIntPtr)(sampledTriangles.Length);
                            // Sample the points.
                            fixed (UIntPtr* sampledTrianglesPointer = sampledTriangles) 
                            {
                                Vec_size_t sampledTrianglesVec = new Vec_size_t
                                {
                                    ptr = sampledTrianglesPointer,
                                    len = sampledTrianglesLength,
                                    cap = sampledTrianglesLength
                                };
                                Ffi.sample_triangles(&triangles, &areasVec, totalArea, &sampledTrianglesVec);
                            }
                            return sampledTriangles;
                        }
                    }
                }
            }
        }


        public static void SetVerticesFromSampledTriangles(this Mesh mesh, Vector3[] originalVertices, UIntPtr[] sampledTriangles)
        {
            Vector3[] points = new Vector3[sampledTriangles.Length / 3];
            unsafe
            {
                fixed (Vector3* pointsPointer = points, originalVerticesPointer = originalVertices)
                {
                    UIntPtr pointsLength = (UIntPtr)(points.Length * 3);
                    Vec_float_t pointsVec = new Vec_float_t
                    {
                        ptr = (float*)pointsPointer,
                        len = pointsLength,
                        cap = pointsLength
                    };
                    UIntPtr originalVerticesLength = (UIntPtr)(originalVertices.Length * 3);
                    Vec_float_t originalVerticesVec = new Vec_float_t
                    {
                        ptr = (float*)originalVerticesPointer,
                        len = originalVerticesLength,
                        cap = originalVerticesLength
                    };
                    // Deterministically sample the points.
                    UIntPtr sampledTrianglesLength = (UIntPtr)(sampledTriangles.Length);
                    fixed (UIntPtr* sampledTrianglesPointer = sampledTriangles) 
                    {
                        Vec_size_t sampledTrianglesVec = new Vec_size_t
                        {
                            ptr = sampledTrianglesPointer,
                            len = sampledTrianglesLength,
                            cap = sampledTrianglesLength
                        };
                        Ffi.set_points_from_sampled_triangles(&originalVerticesVec, &sampledTrianglesVec, &pointsVec);
                    }
                }
            }
            mesh.vertices = points;
            mesh.triangles = new int[sampledTriangles.Length];
        }
        
        
        /// <summary>
        /// Uniformly sample points on a mesh.
        /// Convert the points to icosahedron.
        /// Combine the icosahedrons into a single mesh.
        /// </summary>
        /// <param name="mesh">(this)</param>
        /// <param name="pointsPerM">The number of points per square meter.</param>
        /// <param name="radius">The radius of each icosahedron.</param>
        public static Mesh GetIcosahedrons(this Mesh mesh, float pointsPerM, float radius) 
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
                            // Allocate the icosahedron arrays.
                            Vector3[] icosahedronVertices = new Vector3[numPoints * 12];
                            UIntPtr numIcosahedronVertices = (UIntPtr)(icosahedronVertices.Length * 3);
                            UIntPtr[] icosahedronIndices = new UIntPtr[numPoints * 60];
                            UIntPtr numIcosahedronIndices = (UIntPtr)(icosahedronIndices.Length);
                            Vector2[] icosahedronUvs = new Vector2[numPoints * 12];
                            UIntPtr numIcosahedronUvs = (UIntPtr)(icosahedronUvs.Length * 2);
                            // Allocate the array.
                            Vector3[] points = new Vector3[numPoints];
                            UIntPtr pointsLength = (UIntPtr)(numPoints * 3);
                            // Sample the points.
                            fixed (Vector3* pointsPointer = points, icosahedronVerticesPointer = icosahedronVertices) 
                            {
                                Vec_float_t pointsVec = new Vec_float_t
                                {
                                    ptr = (float*)pointsPointer,
                                    len = pointsLength,
                                    cap = pointsLength
                                };
                                Vec_float_t icosahedronVerticesVec = new Vec_float_t
                                {
                                    ptr = (float*)icosahedronVerticesPointer,
                                    len = numIcosahedronVertices,
                                    cap = numIcosahedronVertices
                                };
                                fixed (UIntPtr* icosahedronIndicesPointer = icosahedronIndices)
                                {
                                    Vec_size_t icosahedronIndicesVec = new Vec_size_t
                                    {
                                        ptr = icosahedronIndicesPointer,
                                        len = numIcosahedronIndices,
                                        cap = numIcosahedronIndices
                                    };

                                    fixed (Vector2* icosahedronUvsPointer = icosahedronUvs)
                                    {
                                        Vec_float_t icosahedronUvsVec = new Vec_float_t
                                        {
                                            ptr = (float*)icosahedronUvsPointer,
                                            len = numIcosahedronUvs,
                                            cap = numIcosahedronUvs
                                        };
                                        
                                        // Sample the points and get spheres.
                                        Ffi.points_to_icosahedrons(&vertices, &triangles, &areasVec,
                                            totalArea, radius, &pointsVec, &icosahedronVerticesVec, 
                                            &icosahedronIndicesVec, &icosahedronUvsVec);
                                    
                                        // Build the mesh.
                                        Mesh pointsMesh = new Mesh();
                                        pointsMesh.vertices = icosahedronVertices;
                                        pointsMesh.triangles = Array.ConvertAll(icosahedronIndices, uIntPtrToInt);
                                        pointsMesh.uv = icosahedronUvs;
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