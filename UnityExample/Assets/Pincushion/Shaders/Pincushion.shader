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

			struct appdata
			{
			    float4 vertex : POSITION;
				float4 normal: NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2g
			{
			    float4 vertex : SV_POSITION;
				float4 color: COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct g2f
			{
			    float4 vertex : POSITION;
				float4 color: COLOR;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			#include "Pincushion.cginc"

			v2g vert (appdata v)
            {
                v2g o;
				start_v2g(v, o);

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
				float distanceToCamera;
				float4 v[4];
				g2f pIn = start_g2f(p[0], distanceToCamera, v);
				
				pIn.vertex = UnityObjectToClipPos(v[0]);
				pIn.uv = float2(1.0f, 0.0f);;
				pIn.color = p[0].color;
				triStream.Append(pIn);

				pIn.vertex = UnityObjectToClipPos(v[1]);
				pIn.uv = float2(1.0f, 1.0f);
				pIn.color = p[0].color;
				triStream.Append(pIn);

				pIn.vertex = UnityObjectToClipPos(v[2]);
				pIn.uv = float2(0.0f, 0.0f);
				pIn.color = p[0].color;
				triStream.Append(pIn);

				pIn.vertex = UnityObjectToClipPos(v[3]);
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