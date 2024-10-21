// Original sources:
// https://gist.github.com/josephbk117/2227128370097500d07c4dd894931429
// https://gist.github.com/kaiware007/8ebad2d28638ff83b6b74970a4f70c9a
Shader "Pincushion/StaticPoints" {
	Properties {
		_Color ("Color", Color) = (1, 1, 1, 1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[Toggle] _KeepConstantScaling("Keep Constant Scaling", Int) = 0
	}
	SubShader {
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "DisableBatching" = "True" }
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		Pass 
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float4 _MainTex_ST;
			int _KeepConstantScaling;
			fixed4 _Color;
			fixed4 noColor = fixed4(0, 0, 0, 0);
			fixed4 noPosition = fixed4(0, 0, 0, 0);
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 normal: NORMAL;
			};
	        
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color: COLOR;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.uv = v.uv;
				
				float3 viewDir = UNITY_MATRIX_IT_MV[2].xyz;
				// Show only front-facing points.
				if (dot(viewDir, v.normal) > 0)
				{
					o.color = _Color;
					// Set the position.
					float relativeScaler = _KeepConstantScaling ? distance(mul(unity_ObjectToWorld, v.vertex), _WorldSpaceCameraPos) : 1;
					o.vertex = UnityViewToClipPos(UnityObjectToViewPos(float4(0.0, 0.0, 0.0, 1.0)) + float4(v.vertex.x, v.vertex.y, 0.0, 0.0) * relativeScaler);
				}
				else
				{
					o.color = noColor;
					o.vertex = noPosition;
				}
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				return tex2D(_MainTex, float2(i.uv)) * i.color;
			}
			
			ENDCG	
		}
	}
}