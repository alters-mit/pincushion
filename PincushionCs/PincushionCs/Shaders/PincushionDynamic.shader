// Render deformable (dynamic) (skinned mesh) pincushion meshes.
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
			
			Buffer<float3> sourceVertices;
			Buffer<float3> sourceNormals;
			Buffer<int3> sampledTriangles;

			// Source: https://github.com/cinight/MinimalCompute/blob/master/Assets/06_Compute_Mesh/06_4_SkinnedMeshBuffer_DiffMesh/SkinnedMeshBuffer_diffMesh.shader
			inline void get_position(in uint vertexID, out float3 vertex, out float3 normal)
			{
				// Get the triangle.
				int3 tri = sampledTriangles[vertexID];

				// Sample the point in the center of the triangle.
				vertex = sourceVertices[tri.x] * u +
					sourceVertices[tri.y] * v +
						sourceVertices[tri.z] * w;
				
				// Get an average of the triangle's normals.
				normal = (
					sourceNormals[tri.x] +
					sourceNormals[tri.y] +
					sourceNormals[tri.z]
					)
				/ 3;
			}

			v2g vert (appdata v, uint vid : SV_VertexID)
			{
				v2g o;
				// set all values in the v2g o to 0.0
				UNITY_INITIALIZE_OUTPUT(v2g, o);
				// setup the instanced id to be accessed
				UNITY_SETUP_INSTANCE_ID(v);
				// copy instance id in the appdata v to the v2g o
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.vertex = v.vertex;
				
				float3 normal;
				get_position(vid, o.vertex.xyz, normal);

				#if _OCCLUDE_BACKFACING
				
				occlude_backfacing(UnityObjectToWorldNormal(normal), v, o);

				#endif
							
				return o;
			}
			
			ENDCG
			
		}
	}
}