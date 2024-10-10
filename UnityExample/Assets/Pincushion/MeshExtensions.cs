using System;
using UnityEngine;


namespace Pincushion
{
    public static class MeshExtensions
    {
        public static Vector3[] GetSampledPoints(this Mesh mesh, float pointsPerM) 
        {
            // Get the casted indices.
            UIntPtr[] indices = Array.ConvertAll(mesh.triangles, intToUIntPtr);
            unsafe
            {
                fixed (Vector3* verticesPointerVec3 = mesh.vertices)
                {
                    UIntPtr verticesLength = (UIntPtr)(mesh.vertices.Length * 3);
                    Vec_float_t vertices = new Vec_float_t
                    {
                        ptr = (float*)verticesPointerVec3,
                        len = verticesLength,
                        cap = verticesLength
                    };
                    // Allocate an array of areas.
                    int numTriangles = mesh.triangles.Length / 3;
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

        public static Mesh GetSamplesMesh(this Mesh mesh, Transform transform, float pointsPerM, float pointSize) 
        {
            // Get the casted indices.
            UIntPtr[] indices = Array.ConvertAll(mesh.triangles, intToUIntPtr);
            unsafe
            {
                fixed (Vector3* verticesPointerVec3 = mesh.vertices)
                {
                    UIntPtr verticesLength = (UIntPtr)(mesh.vertices.Length * 3);
                    Vec_float_t vertices = new Vec_float_t
                    {
                        ptr = (float*)verticesPointerVec3,
                        len = verticesLength,
                        cap = verticesLength
                    };
                    // Allocate an array of areas.
                    int numTriangles = mesh.triangles.Length / 3;
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
                            // Allocate quads.
                            Vector3[,] quadVertices = new Vector3[numPoints, 4];
                            // Sample the points.
                            fixed (Vector3* quadVerticesPointer = quadVertices)
                            {
                                UIntPtr quadVerticesLength = (UIntPtr)(quadVertices.LongLength * 3);
                                Vec_float_t quadVerticesVec = new Vec_float_t()
                                {
                                    ptr = (float*)quadVerticesPointer,
                                    len = quadVerticesLength,
                                    cap = quadVerticesLength
                                };
                                // Get quad vertices.
                                Ffi.sample_quads(&vertices, &triangles, &areasVec, totalArea,
                                    (UIntPtr)numPoints, &quadVerticesVec, pointSize);
                            }
                            // Get the first quad.
                            // Get combined meshes.
                            CombineInstance[] combine = new CombineInstance[numPoints];
                            Matrix4x4 transformMatrix = transform.localToWorldMatrix;
                            for (int i = 0; i < numPoints; i++)
                            {
                                Mesh point = new Mesh();
                                // Copy vertices.
                                point.vertices = new Vector3[4];
                                for (int j = 0; j < 4; j++)
                                {
                           
                                    point.vertices[j] = quadVertices[i, j];
                                }

                                point.triangles = new[]
                                {
                                    0, 2, 1,
                                    2, 3, 1
                                };
                                point.normals = new[]
                                {
                                    -Vector3.forward,
                                    -Vector3.forward,
                                    -Vector3.forward,
                                    -Vector3.forward
                                };
                                point.uv = new[]
                                {
                                    new Vector2(0, 0),
                                    new Vector2(1, 0),
                                    new Vector2(0, 1),
                                    new Vector2(1, 1)
                                };
                                combine[i].mesh = point;
                                combine[i].transform = transformMatrix;
                            }
                            // Combine points.
                            Mesh points = new Mesh();
                            points.CombineMeshes(combine);
                            points.RecalculateBounds();
                            return points;
                        }
                    }
                }
            }
        }


        private static UIntPtr intToUIntPtr(int i)
        {
            return (UIntPtr)i;
        }
    } 
}