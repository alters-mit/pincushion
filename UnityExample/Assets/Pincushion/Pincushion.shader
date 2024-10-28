Shader "Pincushion/Pincushion" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Color ("Color", Color) = (0.9, 0.9, 0.9, 1)
		_PointSize("Point Size", Float) = 0.02
	}
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

			#include "UnityCG.cginc"

			half4 _Color;
			half _PointSize;
			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct appdata
			{
			    float4 vertex : POSITION;
				float4 normal: NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2g
			{
			    float4 position : SV_POSITION;
				float4 color: COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct g2f
			{
			    float4 position : POSITION;
				float4 color: COLOR;
				float2 uv : TEXCOORD0;
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
				
                o.position = v.vertex;

				#if _OCCLUDE_BACKFACING

				// Source: https://discussions.unity.com/t/camera-forward-vector-in-shader/32664/4
				half3 normal = UnityObjectToWorldNormal(v.normal);
				half3 worldVert = mul(unity_ObjectToWorld, v.vertex);
				half3 viewDir = _WorldSpaceCameraPos - worldVert;
				if (dot(viewDir, normal) > 0) {
					o.color = _Color;
				}
				else
				{
					o.color = float4(0, 0, 0, 0);
				}

				#else

				o.color = _Color;

				#endif
				
                return o;
            }

			[maxvertexcount(4)]
			void geom(point v2g p[1], inout TriangleStream<g2f> triStream)
			{
				float3 right = normalize(UNITY_MATRIX_IT_MV[0].xyz);
				float3 up = normalize(UNITY_MATRIX_IT_MV[1].xyz);
				// Adjust point size based on parameters.

				#if _CONSTANT_SCALING
				
				// 0.1 is an arbitrary constant.
				float distanceToCam = distance(mul(unity_ObjectToWorld, p[0].position), _WorldSpaceCameraPos);

				#else

				float distanceToCam = 1;

				#endif

				// Counter eventual rescaling of the renderer by computing average scale.
				float3 scaleX = unity_ObjectToWorld[0].xyz;
				float3 scaleY = unity_ObjectToWorld[1].xyz;
				float3 scaleZ = unity_ObjectToWorld[2].xyz;
				float3 objectScale = float3(length(scaleX), length(scaleY), length(scaleZ));
				float averageScale = (objectScale.x + objectScale.y + objectScale.z) / 3.0;

				float scale = distanceToCam / averageScale; 
				right *= _PointSize * scale;
				up *= _PointSize * scale;

				// Define the four vertices for the billboard in world space
				float4 v[4];
				v[0] = float4(p[0].position + right - up, 1.0f);
				v[1] = float4(p[0].position + right + up, 1.0f);
				v[2] = float4(p[0].position - right - up, 1.0f);
				v[3] = float4(p[0].position - right + up, 1.0f);

				// Define g2f for each vertex
				g2f pIn;
				// set all values in the g2f o to 0.0
				UNITY_INITIALIZE_OUTPUT(g2f, pIn);
				// setup the instanced id to be accessed
				UNITY_SETUP_INSTANCE_ID(p[0]);
				// copy instance id in the v2f i[0] to the g2f o
				UNITY_TRANSFER_INSTANCE_ID(p[0], pIn);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(pIn);
				
				pIn.position = UnityObjectToClipPos(v[0]);
				pIn.uv = float2(1.0f, 0.0f);;
				pIn.color = p[0].color;
				triStream.Append(pIn);

				pIn.position = UnityObjectToClipPos(v[1]);
				pIn.uv = float2(1.0f, 1.0f);
				pIn.color = p[0].color;
				triStream.Append(pIn);

				pIn.position = UnityObjectToClipPos(v[2]);
				pIn.uv = float2(0.0f, 0.0f);
				pIn.color = p[0].color;
				triStream.Append(pIn);

				pIn.position = UnityObjectToClipPos(v[3]);
				pIn.uv = float2(0.0f, 1.0f);
				pIn.color = p[0].color;
				triStream.Append(pIn);
			}

			half4 frag(g2f i) : SV_Target
			{
				return tex2D(_MainTex, float2(i.uv)) * i.color;
			}
		
		ENDCG
		}
	}
}