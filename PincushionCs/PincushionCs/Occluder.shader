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
				// Source: https://discussions.unity.com/t/camera-forward-vector-in-shader/32664/4
				half3 normal = UnityObjectToWorldNormal(v.normal);
				half3 worldVert = mul(unity_ObjectToWorld, v.vertex);
				half3 viewDir = _WorldSpaceCameraPos - worldVert;
				if (dot(viewDir, normal) > 0) {
					o.normal = v.normal;
				}
				else
				{
					o.normal = -v.normal;			
				}
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
				return _Color;
			}
			ENDCG
		}
	}
}