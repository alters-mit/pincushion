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
        /// <param name="scale">The uniform scale of the mesh.</param>
        public static SampledPoints GetSampledPoints(this Mesh mesh, float pointsPerM, float scale) 
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
                fixed (Vector3* verticesPointer = mesh.vertices, normalsPointer = mesh.normals)
                {
                    Vec_Vec3_t vertices = new Vec_Vec3_t
                    {
                        ptr = (Vec3_t*)verticesPointer,
                        len = verticesLength,
                        cap = verticesLength
                    };
                    Vec_Vec3_t normals = new Vec_Vec3_t
                    {
                        ptr = (Vec3_t*)normalsPointer,
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
                            float totalArea = Ffi.set_area(scale, &vertices, &triangles, &areasVec);
                            // Get the number of points.
                            int numPoints = (int)Ffi.get_num_points(totalArea, pointsPerM);
                            // Allocate the arrays.
                            Vector3[] points = new Vector3[numPoints];
                            Vector3[] sampledNormals = new Vector3[numPoints * 4];
                            UIntPtr pointsLength = (UIntPtr)numPoints;
                            UIntPtr sampledNormalsLength = (UIntPtr)sampledNormals.Length;
                            // Sample the points.
                            fixed (Vector3* pointsPointer = points, sampledNormalsPointer = sampledNormals) 
                            {
                                Vec_Vec3_t pointsVec = new Vec_Vec3_t
                                {
                                    ptr = (Vec3_t*)pointsPointer,
                                    len = pointsLength,
                                    cap = pointsLength
                                };
                                Vec_Vec3_t sampledNormalsVec = new Vec_Vec3_t
                                {
                                    ptr = (Vec3_t*)sampledNormalsPointer,
                                    len = sampledNormalsLength,
                                    cap = sampledNormalsLength
                                };
                                Ffi.sample_points(totalArea, &vertices, &triangles, &normals, &areasVec,
                                    &pointsVec, &sampledNormalsVec);
                            }
                            return new SampledPoints
                            {
                                points = points,
                                normals = sampledNormals,
                            };
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
                            float totalArea = Ffi.set_area(scale, &vertices, &triangles, &areasVec);
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
        /// <param name="originalNormals">The normals of the original mesh.</param>
        /// <param name="sampledTriangles">The pre-sampled triangles.</param>
        public static void SetVerticesFromSampledTriangles(this Mesh mesh, Vector3[] originalVertices, 
            Vector3[] originalNormals, UIntPtr[] sampledTriangles)
        {
            Vector3[] points = new Vector3[sampledTriangles.Length / 3];
            Vector3[] normals = new Vector3[sampledTriangles.Length / 3];
            UIntPtr pointsLength = (UIntPtr)points.Length;
            UIntPtr originalVerticesLength = (UIntPtr)originalVertices.Length;
            unsafe
            {
                fixed (Vector3* pointsPointer = points, normalsPointer = normals,
                       originalVerticesPointer = originalVertices, originalNormalsPointer = originalNormals)
                {
                    Vec_Vec3_t pointsVec = new Vec_Vec3_t
                    {
                        ptr = (Vec3_t*)pointsPointer,
                        len = pointsLength,
                        cap = pointsLength
                    };
                    Vec_Vec3_t normalsVec = new Vec_Vec3_t
                    {
                        ptr = (Vec3_t*)normalsPointer,
                        len = pointsLength,
                        cap = pointsLength
                    };
                    Vec_Vec3_t originalVerticesVec = new Vec_Vec3_t
                    {
                        ptr = (Vec3_t*)originalVerticesPointer,
                        len = originalVerticesLength,
                        cap = originalVerticesLength
                    };
                    Vec_Vec3_t originalNormalsVec = new Vec_Vec3_t
                    {
                        ptr = (Vec3_t*)originalNormalsPointer,
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
                        Ffi.set_points_from_sampled_triangles(&originalVerticesVec, &originalNormalsVec,
                            &sampledTrianglesVec, &pointsVec, &normalsVec);
                    }
                }
            }
            mesh.vertices = points;
            mesh.normals = normals;
            mesh.triangles = new int[sampledTriangles.Length];
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
    } 
}