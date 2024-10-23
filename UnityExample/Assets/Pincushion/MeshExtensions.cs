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
                fixed (Vector3* verticesPtr = mesh.vertices, normalsPtr = mesh.normals)
                {
                    Vec_Vertex_t vertices = new Vec_Vertex_t
                    {
                        ptr = (Vertex_t*)verticesPtr,
                        len = verticesLength,
                        cap = verticesLength
                    };
                    Vec_Vertex_t normals = new Vec_Vertex_t
                    {
                        ptr = (Vertex_t*)normalsPtr,
                        len = verticesLength,
                        cap = verticesLength
                    };
                    fixed (float* areasPtr = areas)
                    {
                        Vec_float_t areasT = new Vec_float_t
                        {
                            ptr = areasPtr,
                            len = areasLength,
                            cap = areasLength
                        };
                        fixed (UIntPtr* indicesPtr = indices)
                        {
                            Vec_Triangle_t triangles = new Vec_Triangle_t
                            {
                                ptr = (Triangle_t*)indicesPtr,
                                len = areasLength,
                                cap = areasLength
                            };

                            Mesh_t meshT = new Mesh_t
                            {
                                vertices = vertices,
                                triangles = triangles,
                                normals = normals
                            };

                            Area_t areaT = new Area_t
                            {
                                total_area = 0,
                                areas = areasT
                            };
                            // Get the areas and the total area.
                            Ffi.set_area(&meshT, scale, &areaT);
                            // Get the number of points.
                            int numPoints = (int)Ffi.get_num_points(areaT.total_area, pointsPerM);
                            // Allocate the arrays.
                            Vector3[] points = new Vector3[numPoints];
                            Vector3[] sampledNormals = new Vector3[numPoints * 4];
                            UIntPtr pointsLength = (UIntPtr)numPoints;
                            UIntPtr sampledNormalsLength = (UIntPtr)sampledNormals.Length;
                            // Sample the points.
                            fixed (Vector3* pointsPtr = points, sampledNormalsPtr = sampledNormals) 
                            {
                                Vec_Vertex_t pointsV = new Vec_Vertex_t
                                {
                                    ptr = (Vertex_t*)pointsPtr,
                                    len = pointsLength,
                                    cap = pointsLength
                                };
                                Vec_Vertex_t sampledNormalsV = new Vec_Vertex_t
                                {
                                    ptr = (Vertex_t*)sampledNormalsPtr,
                                    len = sampledNormalsLength,
                                    cap = sampledNormalsLength
                                };
                                Ffi.sample_points(&meshT, &areaT,  &pointsV, &sampledNormalsV);
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
                fixed (Vector3* verticesPtr = mesh.vertices, normalsPtr = mesh.normals)
                {
                    Vec_Vertex_t vertices = new Vec_Vertex_t
                    {
                        ptr = (Vertex_t*)verticesPtr,
                        len = verticesLength,
                        cap = verticesLength
                    };
                    Vec_Vertex_t normals = new Vec_Vertex_t
                    {
                        ptr = (Vertex_t*)normalsPtr,
                        len = verticesLength,
                        cap = verticesLength
                    };
                    fixed (float* areasPointer = areas)
                    {
                        Vec_float_t areasT = new Vec_float_t
                        {
                            ptr = areasPointer,
                            len = areasLength,
                            cap = areasLength
                        };
                        fixed (UIntPtr* indicesPtr = indices)
                        {
                            Vec_Triangle_t triangles = new Vec_Triangle_t
                            {
                                ptr = (Triangle_t*)indicesPtr,
                                len = areasLength,
                                cap = areasLength
                            };
                            Mesh_t meshT = new Mesh_t
                            {
                                vertices = vertices,
                                triangles = triangles,
                                normals = normals
                            };

                            Area_t areaT = new Area_t
                            {
                                total_area = 0,
                                areas = areasT
                            };
                            // Get the areas and the total area.
                            Ffi.set_area(&meshT, scale, &areaT);
                            // Get the number of points.
                            int numPoints = (int)Ffi.get_num_points(areaT.total_area, pointsPerM);
                            // Allocate the array.
                            UIntPtr[] sampledTriangles = new UIntPtr[numPoints * 3];
                            UIntPtr sampledTrianglesLength = (UIntPtr)numPoints;
                            // Sample the points.
                            fixed (UIntPtr* sampledTrianglesPointer = sampledTriangles) 
                            {
                                Vec_Triangle_t sampledTrianglesVec = new Vec_Triangle_t
                                {
                                    ptr = (Triangle_t*)sampledTrianglesPointer,
                                    len = sampledTrianglesLength,
                                    cap = sampledTrianglesLength
                                };
                                Ffi.sample_triangles(&meshT, &areaT, &sampledTrianglesVec);
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
        public static void SetVerticesFromSampledTriangles(this Mesh mesh, Mesh originalMesh, UIntPtr[] sampledTriangles)
        {
            Vector3[] points = new Vector3[sampledTriangles.Length / 3];
            Vector3[] normals = new Vector3[sampledTriangles.Length / 3];
            UIntPtr pointsLength = (UIntPtr)points.Length;
            UIntPtr originalVerticesLength = (UIntPtr)originalMesh.vertices.Length;
            UIntPtr[] originalTrianglesU = Array.ConvertAll(originalMesh.triangles, intToUIntPtr);
            UIntPtr originalTrianglesLength = (UIntPtr)(originalTrianglesU.Length / 3);
            unsafe
            {
                fixed (Vector3* pointsPtr = points, normalsPtr = normals,
                       originalVerticesPtr = originalMesh.vertices, originalNormalsPtr = originalMesh.normals)
                {
                    Vec_Vertex_t pointsV = new Vec_Vertex_t
                    {
                        ptr = (Vertex_t*)pointsPtr,
                        len = pointsLength,
                        cap = pointsLength
                    };
                    Vec_Vertex_t normalsV = new Vec_Vertex_t
                    {
                        ptr = (Vertex_t*)normalsPtr,
                        len = pointsLength,
                        cap = pointsLength
                    };
                    Vec_Vertex_t originalVerticesV = new Vec_Vertex_t
                    {
                        ptr = (Vertex_t*)originalVerticesPtr,
                        len = originalVerticesLength,
                        cap = originalVerticesLength
                    };
                    Vec_Vertex_t originalNormalsV = new Vec_Vertex_t
                    {
                        ptr = (Vertex_t*)originalNormalsPtr,
                        len = originalVerticesLength,
                        cap = originalVerticesLength
                    };
                    // Deterministically sample the points.
                    fixed (UIntPtr* sampledTrianglesPtr = sampledTriangles, originalTrianglesPtr = originalTrianglesU) 
                    {
                        Vec_Triangle_t sampledTrianglesV = new Vec_Triangle_t
                        {
                            ptr = (Triangle_t*)sampledTrianglesPtr,
                            len = pointsLength,
                            cap = pointsLength
                        };
                        Vec_Triangle_t originalTrianglesV = new Vec_Triangle_t
                        {
                            ptr = (Triangle_t*)originalTrianglesPtr,
                            len = originalTrianglesLength,
                            cap = originalTrianglesLength
                        };
                        // Get the original mesh and the sampled mesh.
                        Mesh_t originalMeshT = new Mesh_t
                        {
                            vertices = originalVerticesV,
                            triangles = originalTrianglesV,
                            normals = originalNormalsV
                        };
                        Mesh_t sampledMesh = new Mesh_t
                        {
                            vertices = pointsV,
                            normals = normalsV,
                            triangles = sampledTrianglesV
                        };
                        Ffi.set_points_from_sampled_triangles(&originalMeshT, &sampledMesh);
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