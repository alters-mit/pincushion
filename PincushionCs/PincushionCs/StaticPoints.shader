// Original source: https://gist.github.com/josephbk117/2227128370097500d07c4dd894931429
Shader "Pincushion/StaticPoints" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[Toggle] _KeepConstantScaling("Keep Constant Scaling", Int) = 1
		[Enum(RenderOnTop, 0,RenderWithTest, 4)] _ZTest("Render on top", Int) = 1
	}
	SubShader {
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "DisableBatching" = "True" }
		ZWrite On
		ZTest [_ZTest]
		Blend SrcAlpha OneMinusSrcAlpha
		
		Pass 
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			int _KeepConstantScaling;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
	        
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.uv = v.uv;

				// billboard mesh towards camera
				float relativeScaler = _KeepConstantScaling ? distance(mul(unity_ObjectToWorld, v.vertex), _WorldSpaceCameraPos) : 1;
				o.vertex = mul(UNITY_MATRIX_P, UnityObjectToViewPos(float4(0.0, 0.0, 0.0, 1.0)) + float4(v.vertex.x, v.vertex.y, 0.0, 0.0) * relativeScaler);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				return tex2D(_MainTex, float2(i.uv));
			}
			
			ENDCG	
		}
	}
}