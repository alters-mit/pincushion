using System;

using UnityEngine;

namespace Pincushion
{
    public static class PincushionMeshExtensions
    {
        public Vector3[] GetSampledPoints(this Mesh mesh, float pointsPerM) 
        {
            // Get the casted indices.
            UIntPtr[] indices = Array.ConvertAll(mesh.triangles, new Converter<int, UIntPtr>(intToUint64));
            // Allocate an array of areas.
            float[] areas = new float[indices.Length];
            unsafe 
            {
                fixed (float* verticesPointer = (float*)mesh.vertices, areasPointer = areas)
                {
                    UIntPtr length = mesh.vertices.Length * 3;
                    Vec_float_t vertices = new Vec_float_t()
                    {
                        ptr = verticesPointer,
                        len = length,
                        cap = length
                    };
                    Vec_float_t areasVec = new Vec_float_t()
                    {
                        ptr = areasPointer,
                        len = length,
                        cap = length
                    };
                    fixed (Uint64* indicesPointer = indices)
                    {
                        Vec_size_t triangles = new Vec_float_t()
                        {
                            ptr = indicesPointer,
                            len = length,
                            cap = length
                        };

                        // Get the areas and the total area.
                        float totalArea = Ffi.get_areas(&vertices, &triangles, &areasVec);
                        // Get the number of points.
                        UIntPtr numPoints = Ffi.get_num_points(total_area, pointsPerM);
                        // Allocate the array.
                        Vector3[] points = new Vector3[numPoints];
                        int pointsLength = numPoints * 3;
                        // Sample the points.
                        fixed (float* pointsPointer = (float*)points) 
                        {
                            Vec_float_t pointsVec = new Vec_float_t()
                            {
                                ptr = pointsPointer,
                                len = pointsLength,
                                cap = pointsLength
                            };
                            sample_points(&vertices, &triangles, &areasVec, totalArea, &pointsVec);
                        }
                        return points;
                    }
                }
            }
        }


        private static UIntPtr intToUint64(int i)
        {
            return i as UIntPtr;
        }
    } 
}