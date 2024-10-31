Shader "Pincushion/PincushionReplacement"
{
	SubShader
	{
		Tags { "Queue" = "Overlay" "IgnoreProjector" = "True" "RenderType" = "Transparent" "DisableBatching" = "True" }
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
			#pragma multi_compile _ _CONSTANT_SCALING

			uniform sampler2D _PincushionDistanceTex;

			#include "UnityCG.cginc"

			struct appdata
			{
			    float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2g
			{
				float4 vertex : SV_POSITION;
			};

			struct g2f
			{
			    float4 vertex : POSITION;
				// The point texture UV.
				float2 uv : TEXCOORD0;
				// The distance texture UV.
				float2 distanceUv : TEXCOORD1;
				// The actual distance.
				float distance: TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			#include "Pincushion.cginc"

			v2g vert (appdata v)
            {
                v2g o;
				start_v2g(v, o);
                return o;
            }

			[maxvertexcount(4)]
			void geom(point v2g p[1], inout TriangleStream<g2f> triStream)
			{
				float distanceToCamera;
				float4 v[4];
				g2f pIn = start_g2f(p[0], distanceToCamera, v);

				float4 screenPositionFull = ComputeScreenPos(UnityObjectToClipPos(p[0].vertex));
				const float2 distanceUv = screenPositionFull.xy / screenPositionFull.w;
				
				pIn.vertex = UnityObjectToClipPos(v[0]);
				pIn.uv = float2(1.0f, 0.0f);
				pIn.distanceUv = distanceUv;
				pIn.distance = distanceToCamera;
				triStream.Append(pIn);

				pIn.vertex = UnityObjectToClipPos(v[1]);
				pIn.uv = float2(1.0f, 1.0f);
				pIn.distanceUv = distanceUv;
				pIn.distance = distanceToCamera;
				triStream.Append(pIn);

				pIn.vertex = UnityObjectToClipPos(v[2]);
				pIn.uv = float2(0.0f, 0.0f);
				pIn.distanceUv = distanceUv;
				pIn.distance = distanceToCamera;
				triStream.Append(pIn);

				pIn.vertex = UnityObjectToClipPos(v[3]);
				pIn.uv = float2(0.0f, 1.0f);
				pIn.distanceUv = distanceUv;
				pIn.distance = distanceToCamera;
				triStream.Append(pIn);
			}

			half4 frag(g2f i) : SV_Target
			{
				// Sample the distance texture and compare to the vertex's distance.
				if (i.distance + 0.01 < tex2D(_PincushionDistanceTex, i.distanceUv).r * 100.3)
				{
					return tex2D(_PincushionMainTex, i.uv) * _PincushionColor;
				}
				return float4(1, 0, 0, 1);
			}
			
			ENDCG
		}
	}
}