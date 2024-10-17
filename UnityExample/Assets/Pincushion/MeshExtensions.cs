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
            UIntPtr verticesLength = (UIntPtr)mesh.vertices.Length;
            int numTriangles = indices.Length / 3;
            // Allocate an array of areas.
            float[] areas = new float[numTriangles];
            UIntPtr areasLength = (UIntPtr)numTriangles;
            unsafe
            {
                // All `fixed` statements are boilerplate C#-to-Rust declarations.
                fixed (Vector3* verticesPointerVec3 = mesh.vertices)
                {
                    Vec_Vec3_t vertices = new Vec_Vec3_t
                    {
                        ptr = (Vec3_t*)verticesPointerVec3,
                        len = verticesLength,
                        cap = verticesLength
                    };
                    fixed (float* areasPointer = areas)
                    {
                        Vec_float_t areasVec = new Vec_float_t
                        {
                            ptr = areasPointer,
                            len = areasLength,
                            cap = areasLength
                        };
                        fixed (UIntPtr* indicesPointer = indices)
                        {
                            Vec_Vec3U_t triangles = new Vec_Vec3U_t
                            {
                                ptr = (Vec3U_t*)indicesPointer,
                                len = areasLength,
                                cap = areasLength
                            };
                            // Get the areas and the total area.
                            float totalArea = Ffi.get_areas(&vertices, &triangles, &areasVec);
                            // Get the number of points.
                            int numPoints = (int)Ffi.get_num_points(totalArea, pointsPerM);
                            // Allocate the array.
                            Vector3[] points = new Vector3[numPoints];
                            UIntPtr pointsLength = (UIntPtr)numPoints;
                            // Sample the points.
                            fixed (Vector3* pointsPointer = points) 
                            {
                                Vec_Vec3_t pointsVec = new Vec_Vec3_t
                                {
                                    ptr = (Vec3_t*)pointsPointer,
                                    len = pointsLength,
                                    cap = pointsLength
                                };
                                Ffi.sample_points(totalArea, &vertices, &triangles, &areasVec, &pointsVec);
                            }
                            return points;
                        }
                    }
                }
            }
        }

        
        /// <summary>
        /// Set the triangles at which points can be sampled.
        /// This is useful for deformable meshes in situations where the positions will change but not the triangles we want to derive positions from.
        /// </summary>
        /// <param name="mesh">(this)</param>
        /// <param name="pointsPerM">Points per meter squared of the mesh's surface area.</param>
        /// <param name="scale">The uniform scale of the mesh.</param>
        public static UIntPtr[] GetSampledTriangles(this Mesh mesh, float pointsPerM, float scale)
        {
            // Get the casted indices.
            UIntPtr[] indices = Array.ConvertAll(mesh.triangles, intToUIntPtr);
            UIntPtr verticesLength = (UIntPtr)mesh.vertices.Length;
            int numTriangles = mesh.triangles.Length / 3;
            // Allocate an array of areas.
            float[] areas = new float[numTriangles];
            UIntPtr areasLength = (UIntPtr)numTriangles;
            unsafe
            {
                // All `fixed` statements are boilerplate C#-to-Rust declarations.
                fixed (Vector3* verticesPointerVec3 = mesh.vertices)
                {
                    Vec_Vec3_t vertices = new Vec_Vec3_t
                    {
                        ptr = (Vec3_t*)verticesPointerVec3,
                        len = verticesLength,
                        cap = verticesLength
                    };
                    fixed (float* areasPointer = areas)
                    {
                        Vec_float_t areasVec = new Vec_float_t
                        {
                            ptr = areasPointer,
                            len = areasLength,
                            cap = areasLength
                        };
                        fixed (UIntPtr* indicesPointer = indices)
                        {
                            Vec_Vec3U_t triangles = new Vec_Vec3U_t
                            {
                                ptr = (Vec3U_t*)indicesPointer,
                                len = areasLength,
                                cap = areasLength
                            };
                            // Get the areas and the total area.
                            Ffi.get_areas(&vertices, &triangles, &areasVec);
                            // Scale the areas.
                            float totalArea = Ffi.scale_areas(&areasVec, scale);
                            // Get the number of points.
                            int numPoints = (int)Ffi.get_num_points(totalArea, pointsPerM);
                            // Allocate the array.
                            UIntPtr[] sampledTriangles = new UIntPtr[numPoints * 3];
                            UIntPtr sampledTrianglesLength = (UIntPtr)numPoints;
                            // Sample the points.
                            fixed (UIntPtr* sampledTrianglesPointer = sampledTriangles) 
                            {
                                Vec_Vec3U_t sampledTrianglesVec = new Vec_Vec3U_t
                                {
                                    ptr = (Vec3U_t*)sampledTrianglesPointer,
                                    len = sampledTrianglesLength,
                                    cap = sampledTrianglesLength
                                };
                                Ffi.sample_triangles(totalArea, &triangles, &areasVec, &sampledTrianglesVec);
                            }
                            return sampledTriangles;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Given a pre-sampled array of triangles, sample points on the original mesh.
        /// The sampled points will be vertices on this mesh.
        /// </summary>
        /// <param name="mesh">(this)</param>
        /// <param name="originalVertices">The vertices of the original mesh.</param>
        /// <param name="sampledTriangles">The pre-sampled triangles.</param>
        public static void SetVerticesFromSampledTriangles(this Mesh mesh, Vector3[] originalVertices, UIntPtr[] sampledTriangles)
        {
            Vector3[] points = new Vector3[sampledTriangles.Length / 3];
            UIntPtr pointsLength = (UIntPtr)points.Length;
            UIntPtr originalVerticesLength = (UIntPtr)originalVertices.Length;
            unsafe
            {
                fixed (Vector3* pointsPointer = points, originalVerticesPointer = originalVertices)
                {
                    Vec_Vec3_t pointsVec = new Vec_Vec3_t
                    {
                        ptr = (Vec3_t*)pointsPointer,
                        len = pointsLength,
                        cap = pointsLength
                    };
                    Vec_Vec3_t originalVerticesVec = new Vec_Vec3_t
                    {
                        ptr = (Vec3_t*)originalVerticesPointer,
                        len = originalVerticesLength,
                        cap = originalVerticesLength
                    };
                    // Deterministically sample the points.
                    fixed (UIntPtr* sampledTrianglesPointer = sampledTriangles) 
                    {
                        Vec_Vec3U_t sampledTrianglesVec = new Vec_Vec3U_t
                        {
                            ptr = (Vec3U_t*)sampledTrianglesPointer,
                            len = pointsLength,
                            cap = pointsLength
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
        /// Convert the points to quad meshes.
        /// </summary>
        /// <param name="mesh">(this)</param>
        /// <param name="pointsPerM">The number of points per square meter.</param>
        /// <param name="size">The length of one side of a quad in meters.</param>
        public static Mesh[] GetQuads(this Mesh mesh, float pointsPerM, float size)
        {
            // Sample the points.
            UIntPtr verticesLength = (UIntPtr)(mesh.vertices.Length * 3);
            // Get the casted indices.
            UIntPtr[] indices = Array.ConvertAll(mesh.triangles, intToUIntPtr);
            int numTriangles = mesh.triangles.Length / 3;
            // Allocate an array of areas.
            float[] areas = new float[numTriangles];
            UIntPtr areasLength = (UIntPtr)numTriangles;
            unsafe
            {
                // All `fixed` statements are boilerplate C#-to-Rust declarations.
                fixed (Vector3* verticesPointerVec3 = mesh.vertices)
                {
                    Vec_Vec3_t vertices = new Vec_Vec3_t
                    {
                        ptr = (Vec3_t*)verticesPointerVec3,
                        len = verticesLength,
                        cap = verticesLength
                    };
                    fixed (float* areasPointer = areas)
                    {
                        Vec_float_t areasVec = new Vec_float_t
                        {
                            ptr = areasPointer,
                            len = areasLength,
                            cap = areasLength
                        };
                        fixed (UIntPtr* indicesPointer = indices)
                        {
                            Vec_Vec3U_t triangles = new Vec_Vec3U_t
                            {
                                ptr = (Vec3U_t*)indicesPointer,
                                len = areasLength,
                                cap = areasLength
                            };

                            // Get the areas and the total area.
                            float totalArea = Ffi.get_areas(&vertices, &triangles, &areasVec);
                            
                            // Sample the points.
                            UIntPtr numPoints = Ffi.get_num_points(totalArea, pointsPerM);
                            Vector3[] points = new Vector3[(int)numPoints];
                            fixed (Vector3* pointsPointer = points)
                            {
                                Vec_Vec3_t pointsVec = new Vec_Vec3_t
                                {
                                    ptr = (Vec3_t*)pointsPointer,
                                    len = numPoints,
                                    cap = numPoints
                                };
                                
                                // Get the quads.
                                Quad_Vec3_Vec3U_Vec2_t[] quads = new Quad_Vec3_Vec3U_Vec2_t[(int)numPoints];
                                fixed (Quad_Vec3_Vec3U_Vec2_t* quadsPointer = quads)
                                {
                                    Vec_Quad_Vec3_Vec3U_Vec2_t quadsVec = new Vec_Quad_Vec3_Vec3U_Vec2_t
                                    {
                                        ptr = quadsPointer,
                                        len = numPoints,
                                        cap = numPoints
                                    };
                                    Ffi.points_to_quads(totalArea, size, &vertices, &triangles, &areasVec, &pointsVec, &quadsVec);
                                }
                                
                                // Convert the quads to meshes.
                                Mesh[] meshes = new Mesh[quads.Length];
                                for (int i = 0; i < meshes.Length; i++)
                                {
                                    Mesh m = new Mesh();
                                    m.vertices = new []
                                    {
                                        ToVector3(quads[i].vertex_0),
                                        ToVector3(quads[i].vertex_1),
                                        ToVector3(quads[i].vertex_2),
                                        ToVector3(quads[i].vertex_3),
                                    };
                                    m.triangles = new []
                                    {
                                        0, 2, 1,
                                        2, 3, 1
                                    };
                                    m.uv = new[]
                                    {
                                        new Vector2(0, 0),
                                        new Vector2(1, 0),
                                        new Vector2(0, 1),
                                        new Vector2(1, 1)
                                    };
                                    m.RecalculateNormals();
                                    m.RecalculateTangents();
                                    m.RecalculateBounds();
                                    meshes[i] = m;
                                }
                                return meshes;
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


        private static Vector3 ToVector3(Vec3_t v3)
        {
            return new Vector3(v3.x, v3.y, v3.z);
        }
        

        private static UIntPtr intToUIntPtr(int i)
        {
            return (UIntPtr)i;
        }
    } 
}