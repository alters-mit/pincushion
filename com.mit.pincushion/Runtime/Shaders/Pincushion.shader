// Render pincushions.
Shader "Pincushion/Pincushion" {
	SubShader {
			Tags{ "Queue" = "Overlay" "IgnoreProjector" = "True" "RenderType" = "Transparent" "DisableBatching" = "True" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Off
		
		Pass 
		{
			CGPROGRAM

			#pragma target 2.5
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			#pragma multi_compile _ _PINCUSHION_SKINNED_MESH
			#pragma multi_compile _ _PINCUSHION_OCCLUDE_BACKFACING
			#pragma multi_compile _ _PINCUSHION_CONSTANT_SCALING
			#pragma multi_compile _ _PINCUSHION_OCCLUDE_BEHIND
			#pragma multi_compile _ _PINCUSHION_APPLY_MASK

			#include "UnityCG.cginc"

			struct appdata
			{
			    float4 vertex : POSITION;

			    #if _PINCUSHION_OCCLUDE_BACKFACING

			    // This is used to determine if a point is backfacing.
			    float4 normal: NORMAL;

			    #endif
										
			    UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2g
			{
			    float4 vertex : SV_POSITION;

			    #if _PINCUSHION_OCCLUDE_BACKFACING || _PINCUSHION_APPLY_MASK
							
			    // To hide a backfacing point, set its color to (0, 0, 0, 0).
			    // Otherwise, this will be the _PincushionColor
			    float4 color: COLOR;

			    #endif
										
			    UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct g2f
			{
			    float4 vertex : POSITION;
			    float2 uv : TEXCOORD0;

			    #if _PINCUSHION_OCCLUDE_BACKFACING || _PINCUSHION_APPLY_MASK

			    // The color from v2g.
			    float4 color: COLOR;
										
			    #endif

			    #if _PINCUSHION_OCCLUDE_BEHIND
										
			    // The distance texture UV.
			    float2 distanceUv : TEXCOORD1;
			    // The actual distance.
			    float distance: TEXCOORD2;

			    #endif
										
			    UNITY_VERTEX_INPUT_INSTANCE_ID
			    UNITY_VERTEX_OUTPUT_STEREO
			};

			#if _PINCUSHION_SKINNED_MESH

			// These values are used to sample points in the center of triangles.
			static const float t = 0.707106781186547524400844362104849039;
			static const float u = 1 - t;
			static const float v = 0.5 * t;
			static const float w = 1 - u - v;

			StructuredBuffer<float3> _PincushionSourceVertices;
			StructuredBuffer<float3> _PincushionSourceNormals;
			StructuredBuffer<int3> _PincushionSampledTriangles;

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

			#else

			inline float4 get_vertex(in appdata v, in uint vid)
			{
				return v.vertex;
			}

			#if _PINCUSHION_OCCLUDE_BACKFACING

			inline float3 get_normal(in appdata v, in uint vid)
			{
				return v.normal;
			}

			#endif

			#endif

			uniform float4 _PincushionColor;
			uniform float _PincushionPointSize;
			uniform sampler2D _PincushionMainTex;
			static float2 pointUvs[4] = { float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 1) };

			#if _PINCUSHION_OCCLUDE_BEHIND

			uniform sampler2D _PincushionDistanceTex;
									
			#endif

			#if _PINCUSHION_APPLY_MASK

			Buffer<uint> _PincushionMask;

			#endif

			v2g vert (appdata v, uint vid : SV_VertexID)
			{
				v2g o;
				// set all values in the v2g o to 0.0
				UNITY_INITIALIZE_OUTPUT(v2g, o);
				// setup the instanced id to be accessed
				UNITY_SETUP_INSTANCE_ID(v);
				// copy instance id in the appdata v to the v2g o
				UNITY_TRANSFER_INSTANCE_ID(v, o);
													
				o.vertex = get_vertex(v, vid);
							
				#if _PINCUSHION_OCCLUDE_BACKFACING

				// Hide points facing away from the camera.
				// Source: https://discussions.unity.com/t/camera-forward-vector-in-shader/32664/4
				float3 worldVert = mul(unity_ObjectToWorld, v.vertex);
				float3 viewDir = _WorldSpaceCameraPos - worldVert;
				if (dot(viewDir, get_normal(v, vid)) > 0) {
					o.color = _PincushionColor;
				}
				else
				{
					o.color = float4(0, 0, 0, 0);
				}

				#elif _PINCUSHION_APPLY_MASK

				o.color = _PincushionColor;

				#endif

				#if _PINCUSHION_APPLY_MASK

				// Hide some points.
				if (_PincushionMask[vid] == 0)
				{
					o.color = float4(0, 0, 0, 0);
				}
				
				#endif
										
				return o;
			}

			[maxvertexcount(4)]
			void geom(point v2g p[1], inout TriangleStream<g2f> triStream)
			{
				float3 right = normalize(UNITY_MATRIX_IT_MV[0].xyz);
				float3 up = normalize(UNITY_MATRIX_IT_MV[1].xyz);

				#if _PINCUSHION_CONSTANT_SCALING || _PINCUSHION_OCCLUDE_BEHIND
				
				float distanceToCamera = distance(mul(unity_ObjectToWorld, p[0].vertex), _WorldSpaceCameraPos);

				#endif

				#if _PINCUSHION_OCCLUDE_BEHIND

				// To occlude behind, we need the point's coordinates on the pre-rendered distance texture.
				// We assume that the size of the distance texture matches that of the screen.
				float4 screenPositionFull = ComputeScreenPos(UnityObjectToClipPos(p[0].vertex));
				const float2 distanceUv = screenPositionFull.xy / screenPositionFull.w;

				#endif

				#if _PINCUSHION_CONSTANT_SCALING
										
				// Keep a constant scale regardless of distance.
				float scale = distanceToCamera * 0.1;

				#else

				float scale = 1;

				#endif

				// Handle eventual rescaling of the renderer by computing average scale.
				float3 objectScale = float3(length( unity_ObjectToWorld[0].xyz),
					length(unity_ObjectToWorld[1].xyz),
					length(unity_ObjectToWorld[2].xyz));
				float averageScale = (objectScale.x + objectScale.y + objectScale.z) / 3.0;

				scale /= averageScale;
				right *= _PincushionPointSize * scale;
				up *= _PincushionPointSize * scale;

				// Define the four vertices for the billboard in world space.
				float4 v[4];
				v[0] = float4(p[0].vertex + right - up, 1.0f);
				v[1] = float4(p[0].vertex + right + up, 1.0f);
				v[2] = float4(p[0].vertex - right - up, 1.0f);
				v[3] = float4(p[0].vertex - right + up, 1.0f);

				g2f o;
				UNITY_INITIALIZE_OUTPUT(g2f, o);
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_TRANSFER_INSTANCE_ID(i, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				for (int j = 0; j < 4; j++)
				{
					o.vertex = UnityObjectToClipPos(v[j]);
					// The UVs never change.
					o.uv = pointUvs[j];
					
					#if _PINCUSHION_OCCLUDE_BACKFACING || _PINCUSHION_APPLY_MASK

					o.color = p[0].color;

					#endif
								
					#if _PINCUSHION_OCCLUDE_BEHIND

					o.distanceUv = distanceUv;
					o.distance = distanceToCamera;

					#endif

					// Add the vertex.
					triStream.Append(o);
				}
			}

			float4 frag(g2f i) : SV_Target
			{

				#if _PINCUSHION_OCCLUDE_BACKFACING || _PINCUSHION_APPLY_MASK

				float4 color = i.color;

				#else

				float4 color = _PincushionColor;
				
				#endif
							
				#if _PINCUSHION_OCCLUDE_BACKFACING

				// The color was set via calculating the normal.
				return tex2D(_PincushionMainTex, i.uv) * color;

				#elif _PINCUSHION_OCCLUDE_BEHIND

				// Sample the distance texture and compare to the vertex's distance.
				if (i.distance < tex2D(_PincushionDistanceTex, i.distanceUv).r + 0.01)
				{
					return tex2D(_PincushionMainTex, i.uv) * color;
				}
				return float4(0, 0, 0, 0);

				#else

				return tex2D(_PincushionMainTex, i.uv) * color;

				#endif
										
			}
			
			ENDCG
			
		}
	}
}