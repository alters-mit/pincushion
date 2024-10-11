using System;
using System.Linq;
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
        
        
        public static void SetTopology(this Mesh mesh)
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