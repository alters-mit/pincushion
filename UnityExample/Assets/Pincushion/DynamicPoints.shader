// Source: https://bronsonzgeb.com/index.php/2022/01/15/drawing-with-sdfs-in-unity/
Shader "Pincushion/DynamicPoints" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_PointSize ("Point Size", Float) = 0.015
		_NumPoints("Number of Points", Int) = 64
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		Cull Off
		
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 3.5
		#include "UnityCG.cginc"

		uint _NumPoints;
		float _PointSize;
		fixed4 _Color;
		float4 _Positions[_NumPoints];

		struct appdata
		{
		    float4 vertex : POSITION;
		    float2 uv : TEXCOORD0;
		};

		struct v2f
		{
		    float4 vertex : SV_POSITION;
		    float2 uv : TEXCOORD0;
			uint id;
		};

		v2f vert(appdata v, uint id: SV_VertexID)
		{
		    v2f o;
		    o.vertex = UnityObjectToClipPos(v.vertex);
		    o.uv = v.uv * 2.0f - 1.0f;
			o.id = id;
		    return o;
		}

		// Source: https://iquilezles.org/www/articles/distfunctions2d/distfunctions2d.htm
		float smin(float a, float b, float k)
		{
		    const float h = max(k - abs(a - b), 0.0) / k;
		    return min(a, b) - h * h * k * (1.0 / 4.0);
		}

		// Source: https://iquilezles.org/www/articles/distfunctions2d/distfunctions2d.htm
		float sdCircle(float2 p, float r)
		{
		    return length(p) - r;
		}

		fixed4 frag(v2f i) : COLOR
		{
		    float d = 10000000;
			const float3 position = _Positions[i.id].xyz;
			d = smin(d, sdCircle(i.uv - position, _PointSize), 0.1);
		    d = smoothstep(0.02, 0.03, d);
		    d = saturate(1 - d);
		    return d * _Color;
		}
		
		ENDCG
	}
	FallBack "Diffuse"
}