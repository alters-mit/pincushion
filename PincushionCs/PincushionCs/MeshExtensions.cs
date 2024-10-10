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

        public static Mesh GetSamplesMesh(this Mesh mesh, Transform transform, float pointsPerM, float pointRadius)
        {
            // Sample some points.
            Vector3[] points = mesh.GetSampledPoints(pointsPerM);
            // Start to combine point meshes.
            CombineInstance[] combine = new CombineInstance[points.Length];
            Mesh point = GetIcosahedron(pointRadius);
            // Set the position.
            for (int i = 0; i < combine[0].mesh.vertices.Length; i++)
            {
                combine[0].mesh.vertices[i] += points[0];
            }
            combine[0].transform = transform.localToWorldMatrix;
            // Scale the mesh.
            for (int i = 1; i < combine.Length; i++)
            {
                combine[i].mesh = new Mesh();
            }
        }


        /// <summary>
        /// Source: https://superhedralcom.wordpress.com/2020/05/17/building-the-unit-icosahedron/
        /// </summary>
        /// <param name="r">The radius of the icosahedron.</param>
        private static Mesh GetIcosahedron(float r)
        {
            float t = r * (1.0f + (float) Math.Sqrt(5.0)) / 2.0f;

            Mesh mesh = new Mesh();
            mesh.vertices = new []
            {
                new Vector3(-r, t, 0),
                new Vector3(r, t, 0),
                new Vector3(-r, -t, 0),
                new Vector3(r, -t, 0),
                new Vector3(0, -r, t),
                new Vector3(0, r, t),
                new Vector3(0, -r, -t),
                new Vector3(0, r, -t),
                new Vector3(t, 0, -r),
                new Vector3(t, 0, r),
                new Vector3(-t, 0, -r),
                new Vector3(-t, 0, r),
            };
            mesh.triangles = new[]
            {
                0, 11, 5,
                0, 5, 1,
                0, 1, 7,
                0, 7, 10,
                0, 10, 11,
                1, 5, 9,
                5, 11, 4,
                11, 10, 2,
                10, 7, 6,
                7, 1, 8,
                3, 9, 4,
                3, 4, 2,
                3, 2, 6,
                3, 6, 8,
                3, 8, 9,
                4, 9, 5,
                2, 4, 11,
                6, 2, 10,
                8, 6, 7,
                9, 8, 1,
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }


        private static UIntPtr intToUIntPtr(int i)
        {
            return (UIntPtr)i;
        }
    } 
}