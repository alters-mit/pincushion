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
        /// Returns a new mesh containing the sampled data (points, triangles, normals).
        /// </summary>
        /// <param name="mesh">(this)</param>
        /// <param name="pointsPerM">The number of points per square meter.</param>
        /// <param name="scale">The uniform scale of the mesh.</param>
        public static Mesh GetSampledMesh(this Mesh mesh, float pointsPerM, float scale) 
        {
            // Get the casted indices.
            UIntPtr[] indices = mesh.GetTriangles();
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
                            Vector3[] sampledPoints = new Vector3[numPoints];
                            Vector3[] sampledNormals = new Vector3[numPoints];
                            UIntPtr pointsLength = (UIntPtr)numPoints;
                            UIntPtr sampledNormalsLength = (UIntPtr)sampledNormals.Length;
                            // Sample the points.
                            fixed (Vector3* pointsPtr = sampledPoints, sampledNormalsPtr = sampledNormals) 
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

                                Mesh sampledMesh = new Mesh();
                                sampledMesh.vertices = sampledPoints;
                                sampledMesh.normals = sampledNormals;
                                sampledMesh.triangles = new int[sampledMesh.vertices.Length * 6];
                                sampledMesh.SetPointTopology();
                                return sampledMesh;
                            }
                        }
                    }
                }
            }
        }

        
        /// <summary>
        /// Returns the triangles at which points can be sampled.
        /// This is useful for deformable meshes in situations where the positions will change but not the triangles we want to derive positions from.
        /// </summary>
        /// <param name="mesh">(this)</param>
        /// <param name="pointsPerM">Points per meter squared of the mesh's surface area.</param>
        /// <param name="scale">The uniform scale of the mesh.</param>
        /// <param name="sourceTriangles">The source mesh's triangles as UIntPtr values.</param>
        public static int[] GetSampledTriangles(this Mesh mesh, float pointsPerM, float scale, UIntPtr[] sourceTriangles)
        {
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
                        fixed (UIntPtr* sourceTrianglesPtr = sourceTriangles)
                        {
                            Vec_Triangle_t triangles = new Vec_Triangle_t
                            {
                                ptr = (Triangle_t*)sourceTrianglesPtr,
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
                            return Array.ConvertAll(sampledTriangles, uIntPtrToInt);
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


        /// <summary>
        /// Returns the triangle indices, converted to UIntPtr values.
        /// </summary>
        /// <param name="mesh">(this)</param>
        public static UIntPtr[] GetTriangles(this Mesh mesh)
        {
            return Array.ConvertAll(mesh.triangles, intToUIntPtr);
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