Shader "Pincushion/PincushionReplacement"
{
	Properties
	{
		_DistanceTex ("Distance", 2D) = "white" {}
	}
	SubShader
	{
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
			#pragma multi_compile _ _CONSTANT_SCALING

			sampler2D _DistanceTex;

			#include "UnityCG.cginc"

			struct appdata
			{
			    float4 vertex : POSITION;
				// The distance texture UV.
				float2 uv: TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2g
			{
				float4 vertex : SV_POSITION;
				// The distance texture UV.
				float2 uv : TEXCOORD0;
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
				// Store the distance texture UV.
				o.uv = v.uv;
                return o;
            }

			[maxvertexcount(4)]
			void geom(point v2g p[1], inout TriangleStream<g2f> triStream)
			{
				float distanceToCamera;
				float4 v[4];
				g2f pIn = start_g2f(p[0], distanceToCamera, v);
				
				pIn.vertex = UnityObjectToClipPos(v[0]);
				pIn.uv = float2(1.0f, 0.0f);
				pIn.distanceUv = p[0].uv;
				pIn.distance = distanceToCamera;
				triStream.Append(pIn);

				pIn.vertex = UnityObjectToClipPos(v[1]);
				pIn.uv = float2(1.0f, 1.0f);
				pIn.distanceUv = p[0].uv;
				pIn.distance = distanceToCamera;
				triStream.Append(pIn);

				pIn.vertex = UnityObjectToClipPos(v[2]);
				pIn.uv = float2(0.0f, 0.0f);
				pIn.distanceUv = p[0].uv;
				pIn.distance = distanceToCamera;
				triStream.Append(pIn);

				pIn.vertex = UnityObjectToClipPos(v[3]);
				pIn.uv = float2(0.0f, 1.0f);
				pIn.distanceUv = p[0].uv;
				pIn.distance = distanceToCamera;
				triStream.Append(pIn);
			}

			half4 frag(g2f i) : SV_Target
			{
				// Sample the distance texture and compare to the vertex's distance.
				if (i.distance >= tex2D(_DistanceTex, float2(i.distanceUv)).r + 0.01)
				{
					return float4(0, 0, 0, 0);
				}
				return tex2D(_MainTex, float2(i.uv)) * _Color;
			}
			
			ENDCG
		}
	}
}