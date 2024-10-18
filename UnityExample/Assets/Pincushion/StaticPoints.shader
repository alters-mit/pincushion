Shader "Pincushion/StaticPoints" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "DisableBatching" = "True" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 200
		
		Pass 
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct appdata
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};
	        
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			const float3 vect3Zero = float3(0.0, 0.0, 0.0);

			// Original source: https://gist.github.com/kaiware007/8ebad2d28638ff83b6b74970a4f70c9a
			v2f vert (appdata v)
			{
				v2f o;

				float4 camPos = float4(UnityObjectToViewPos(vect3Zero).xyz, 1.0);

                float4 viewDir = float4(v.pos.x, v.pos.y, 0.0, 0.0);
                float4 outPos = mul(UNITY_MATRIX_P, camPos + viewDir);

                o.pos = outPos;
                o.uv = v.uv;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				 return tex2D(_MainTex, i.uv);
			}
			
			ENDCG	
		}
	}
}