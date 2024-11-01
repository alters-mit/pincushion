// This shader gets the distance of each vertex from the camera and renders it to a texture.
Shader "Pincushion/Distance" {
	SubShader {
		Tags { "Queue" = "Background" "RenderType" = "Opaque" }
		
		ZWrite On
		
		Pass 
		{
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float distance : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.distance = distance(mul(unity_ObjectToWorld, v.vertex), _WorldSpaceCameraPos);
				return o;
			}
			
			float4 frag(v2f i) : SV_Target
			{
				return float4(i.distance, i.distance, i.distance, 1);
			}
			
			ENDCG
		}
	
	}
}