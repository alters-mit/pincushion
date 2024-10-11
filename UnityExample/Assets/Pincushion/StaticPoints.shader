Shader "Pincushion/StaticPoints" {
	Properties {
		_Color ("Color", Color) = (0.9, 0.9, 0.9, 1)
		_PointSize("Point Size", Float) = 0.02
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		Cull Off
		
		Pass 
		{
			CGPROGRAM
		
			#pragma vertex vert
            #pragma fragment frag
			#pragma geometry geom
            #include "UnityCG.cginc"

			half4 _Color;
			half _PointSize;

			struct Point
			{
			    float4 position : SV_POSITION;
			};

			Point vert (appdata_base v)
            {
                Point o;
                o.position = UnityObjectToClipPos(v.vertex);
                return o;
            }

			// Source: https://github.com/keijiro/Pcx/blob/master/Packages/jp.keijiro.pcx/Runtime/Shaders/Disk.cginc
			[maxvertexcount(36)]
			void geom(point Point input[1], inout TriangleStream<Point> outStream)
			{
				float4 origin = input[0].position;
				float2 extent = abs(UNITY_MATRIX_P._11_22 * _PointSize);
				// Copy the basic information.
			    Point o = input[0];

			    // Determine the number of slices based on the radius of the
			    // point on the screen.
			    float radius = extent.y / origin.w * _ScreenParams.y;
			    uint slices = min((radius + 1) / 5, 4) + 2;

			    // Slightly enlarge quad points to compensate area reduction.
			    // Hopefully this line would be complied without branch.
			    if (slices == 2) extent *= 1.2;

			    // Top vertex
			    o.position.y = origin.y + extent.y;
			    o.position.xzw = origin.xzw;
			    outStream.Append(o);

				UNITY_LOOP for (uint i = 1; i < slices; i++)
			    {
			        float sn, cs;
			        sincos(UNITY_PI / slices * i, sn, cs);

			        // Right side vertex
			        o.position.xy = origin.xy + extent * float2(sn, cs);
			        outStream.Append(o);

			        // Left side vertex
			        o.position.x = origin.x - extent.x * sn;
			        outStream.Append(o);
			    }

			    // Bottom vertex
			    o.position.x = origin.x;
			    o.position.y = origin.y - extent.y;
			    outStream.Append(o);

			    outStream.RestartStrip();
			}

			half4 frag() : SV_Target
			{
				return _Color;
			}
		
		ENDCG
		}
	}
}