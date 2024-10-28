Shader "Pincushion/Occluder"
{
	Properties
	{
		_Color ("Color", Color) = (0, 0, 0, 1)
	}
	SubShader
	{
		Tags { "RenderType" = "Background" }
		Cull Front
		LOD 100

		Pass
		{
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			float4 _Color;

			struct appdata
			{
				float4 vertex : POSITION;
				float4 normal: NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normal: NORMAL;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float3 viewDir = normalize(UNITY_MATRIX_IT_MV[2].xyz);
				if (dot(viewDir, v.normal) > 0) {
					o.normal = v.normal;
				}
				else
				{
					o.normal = -v.normal;			
				}
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				return _Color;
			}
			ENDCG
		}
	}
}