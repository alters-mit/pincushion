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

			#include "Pincushion.cginc"
			
			Buffer<float3> _PincushionSourceVertices;
			Buffer<float3> _PincushionSourceNormals;
			Buffer<int3> _PincushionSampledTriangles;

			v2g vert (appdata i, uint vid : SV_VertexID)
			{
				v2g o;
				// set all values in the v2g o to 0.0
				UNITY_INITIALIZE_OUTPUT(v2g, o);
				// setup the instanced id to be accessed
				UNITY_SETUP_INSTANCE_ID(i);
				// copy instance id in the appdata v to the v2g o
				UNITY_TRANSFER_INSTANCE_ID(i, o);

				// Get the triangle.
				int3 tri = _PincushionSampledTriangles[vid];
				
				// Sample the point in the center of the triangle.
				o.vertex = float4(_PincushionSourceVertices[tri.x] * u +
					_PincushionSourceVertices[tri.y] * v +
						_PincushionSourceVertices[tri.z] * w, 1);

				#if _OCCLUDE_BACKFACING

				// Get an average of the triangle's normals.
				float3 normal = (
					_PincushionSourceNormals[tri.x] +
					_PincushionSourceNormals[tri.y] +
					_PincushionSourceNormals[tri.z]
					)
				/ 3;
				
				occlude_backfacing(UnityObjectToWorldNormal(normal), i, o);

				#endif
							
				return o;
			}
			
			ENDCG
			
		}
	}
}