// This shader is used for all render modes except OccludeBehind.
// It is assigned to the PincushionRenderer.
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
			#pragma multi_compile _ _OCCLUDE_BACKFACING
			#pragma multi_compile _ _CONSTANT_SCALING
			#pragma multi_compile _ _OCCLUDE_BEHIND

			uniform half4 _PincushionColor;
			uniform half _PincushionPointSize;
			uniform sampler2D _PincushionMainTex;

			#if _OCCLUDE_BEHIND

			uniform sampler2D _PincushionDistanceTex; 
			
			#endif

			#include "UnityCG.cginc"

			struct appdata
			{
			    float4 vertex : POSITION;

				#if _OCCLUDE_BACKFACING
				
				float4 normal: NORMAL;

				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2g
			{
			    float4 vertex : SV_POSITION;

				#if _OCCLUDE_BACKFACING
				
				float4 color: COLOR;

				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct g2f
			{
			    float4 vertex : POSITION;
				float2 uv : TEXCOORD0;

				#if _OCCLUDE_BACKFACING

				float4 color: COLOR;
				
				#endif

				#if _OCCLUDE_BEHIND
				
				// The distance texture UV.
				float2 distanceUv : TEXCOORD1;
				// The actual distance.
				float distance: TEXCOORD2;

				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2g vert (appdata v)
            {
                v2g o;
				// set all values in the v2g o to 0.0
				UNITY_INITIALIZE_OUTPUT(v2g, o);
				// setup the instanced id to be accessed
				UNITY_SETUP_INSTANCE_ID(v);
				// copy instance id in the appdata v to the v2g o
				UNITY_TRANSFER_INSTANCE_ID(v, o);
							
				o.vertex = v.vertex;

				#if _OCCLUDE_BACKFACING

				// Hide points facing away from the camera.
				// Source: https://discussions.unity.com/t/camera-forward-vector-in-shader/32664/4
				half3 normal = UnityObjectToWorldNormal(v.normal);
				half3 worldVert = mul(unity_ObjectToWorld, v.vertex);
				half3 viewDir = _WorldSpaceCameraPos - worldVert;
				if (dot(viewDir, normal) > 0) {
					o.color = _PincushionColor;
				}
				else
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
				float distanceToCamera = distance(mul(unity_ObjectToWorld, p[0].vertex), _WorldSpaceCameraPos);

				#if _OCCLUDE_BEHIND

				float4 screenPositionFull = ComputeScreenPos(UnityObjectToClipPos(p[0].vertex));
				const float2 distanceUv = screenPositionFull.xy / screenPositionFull.w;

				#endif

				#if _CONSTANT_SCALING
				
				// Keep a constant scale regardless of distance.
				float scale = distanceToCamera * 0.1;

				#else

				float scale = 1;

				#endif

				// Handle eventual rescaling of the renderer by computing average scale.
				float3 scaleX = unity_ObjectToWorld[0].xyz;
				float3 scaleY = unity_ObjectToWorld[1].xyz;
				float3 scaleZ = unity_ObjectToWorld[2].xyz;
				float3 objectScale = float3(length(scaleX), length(scaleY), length(scaleZ));
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

				g2f pIn;
				UNITY_INITIALIZE_OUTPUT(g2f, pIn);
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_TRANSFER_INSTANCE_ID(i, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				
				pIn.vertex = UnityObjectToClipPos(v[0]);
				pIn.uv = float2(1.0f, 0.0f);

				#if _OCCLUDE_BACKFACING
				
				pIn.color = p[0].color;

				#endif

				#if _OCCLUDE_BEHIND

				pIn.distanceUv = distanceUv;
				pIn.distance = distanceToCamera;

				#endif
				
				triStream.Append(pIn);

				pIn.vertex = UnityObjectToClipPos(v[1]);
				pIn.uv = float2(1.0f, 1.0f);

				#if _OCCLUDE_BACKFACING
				
				pIn.color = p[0].color;

				#endif
				
				#if _OCCLUDE_BEHIND

				pIn.distanceUv = distanceUv;
				pIn.distance = distanceToCamera;

				#endif
				
				triStream.Append(pIn);

				pIn.vertex = UnityObjectToClipPos(v[2]);
				pIn.uv = float2(0.0f, 0.0f);

				#if _OCCLUDE_BACKFACING
				
				pIn.color = p[0].color;

				#endif
				
				#if _OCCLUDE_BEHIND

				pIn.distanceUv = distanceUv;
				pIn.distance = distanceToCamera;

				#endif
				
				triStream.Append(pIn);

				pIn.vertex = UnityObjectToClipPos(v[3]);
				pIn.uv = float2(0.0f, 1.0f);

				#if _OCCLUDE_BACKFACING
				
				pIn.color = p[0].color;

				#endif
				
				#if _OCCLUDE_BEHIND

				pIn.distanceUv = distanceUv;
				pIn.distance = distanceToCamera;

				#endif
				
				triStream.Append(pIn);
			}

			half4 frag(g2f i) : SV_Target
			{
				#if _OCCLUDE_BACKFACING

				// The color was set via calculating the normal.
				return tex2D(_PincushionMainTex, i.uv) * i.color;

				#elif _OCCLUDE_BEHIND

				// Sample the distance texture and compare to the vertex's distance.
				if (i.distance < tex2D(_PincushionDistanceTex, i.distanceUv).r + 0.01)
				{
					return tex2D(_PincushionMainTex, i.uv) * _PincushionColor;
				}
				return float4(0, 0, 0, 0);

				#else

				return _PincushionColor;

				#endif
				
			}
		
		ENDCG
		}
	}
}