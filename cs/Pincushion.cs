using System;

using UnityEngine;

namespace Pincushion
{
    public static class PincushionMeshExtensions
    {
        public Vector3[] GetSampledPoints(this Mesh mesh, float pointsPerCm) 
        {
            // Get the casted indices.
            UIntPtr[] indices = Array.ConvertAll(mesh.triangles, new Converter<int, UIntPtr>(intToUint64));
            unsafe 
            {
                fixed (float* verticesPointer = (float*)mesh.vertices)
                {
                    UIntPtr length = mesh.vertices.length * 3;
                    Vec_float_t vertices = new Vec_float_t()
                    {
                        ptr = verticesPointer,
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

                        // Get the volume.
                        float volume = Ffi.get_volume_ffi(&vertices, &triangles);
                        // Get the number of points.
                        UIntPtr numPoints = Ffi.get_num_points_ffi(volume, pointsPerCm);
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
                            sample_points_ffi(&vertices, &triangles, &pointsVec);
                        }
                        return points;
                    }
                }
            }
        }


        public UIntPtr GetSampledPoints(this Mesh mesh, float pointsPerCm, ref Vector3[] points) 
        {
            // Get the casted indices.
            UIntPtr[] indices = Array.ConvertAll(mesh.triangles, new Converter<int, UIntPtr>(intToUint64));
            unsafe 
            {
                fixed (float* verticesPointer = (float*)mesh.vertices, (float*)points)
                {
                    UIntPtr length = mesh.vertices.length * 3;
                    Vec_float_t vertices = new Vec_float_t()
                    {
                        ptr = verticesPointer,
                        len = length,
                        cap = length
                    };
                    UIntPtr pointsLength = points.length * 3;
                    Vec_float_t pointsVec = new Vec_float_t()
                    {
                        ptr = pointsPointer,
                        len = pointsLength,
                        cap = pointsLength
                    };
                    fixed (Uint64* indicesPointer = indices)
                    {
                        Vec_size_t triangles = new Vec_float_t()
                        {
                            ptr = indicesPointer,
                            len = length,
                            cap = length
                        };
                        // Sample the points.
                        return sample_points_ffi_from_ppcm(&vertices, &triangles, &points);
                    }
                }
            }
        }


        public int GetSampledPoints(this Mesh mesh, float pointsPerCm, ref Vector3[] points)
        {
            return (int)GetSampledPoints(mesh, pointsPerCm, numPoints, ref points);
        }


        private static UIntPtr intToUint64(int i)
        {
            return i as UIntPtr;
        }
    } 
}