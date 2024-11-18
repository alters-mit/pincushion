// Render pincushions from SkinnedMeshRenderers.
Shader "Pincushion/PincushionDynamic" {
	SubShader {
			Tags{ "Queue" = "Overlay" "IgnoreProjector" = "True" "RenderType" = "Transparent" "DisableBatching" = "True" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Off
		
		Pass 
		{
			CGPROGRAM

			// These values are used to sample points in the center of triangles.
			static const float t = 0.707106781186547524400844362104849039;
			static const float u = 1 - t;
			static const float v = 0.5 * t;
			static const float w = 1 - u - v;

			Buffer<float3> _PincushionSourceVertices;
			Buffer<float3> _PincushionSourceNormals;
			Buffer<int3> _PincushionSampledTriangles;

			#include "PincushionStructs.cginc"

			inline float4 get_vertex(in appdata i, in uint vid)
			{
				// Get the triangle.
				int3 tri = _PincushionSampledTriangles[vid];
				
				// Sample the point in the center of the triangle.
				return float4(_PincushionSourceVertices[tri.x] * u +
					_PincushionSourceVertices[tri.y] * v +
						_PincushionSourceVertices[tri.z] * w, 1);
			}

			#if _PINCUSHION_OCCLUDE_BACKFACING

			inline float3 get_normal(in appdata v, in uint vid)
			{
				// Get the triangle.
				int3 tri = _PincushionSampledTriangles[vid];
				
				// Get the average of the triangle's normals.
				return (
					_PincushionSourceNormals[tri.x] +
					_PincushionSourceNormals[tri.y] +
					_PincushionSourceNormals[tri.z]
					)
				/ 3;
			}

			#endif

			#include "Pincushion.cginc"
			
			ENDCG
			
		}
	}
}