Shader "Pincushion/DynamicPoints" {
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

			struct appdata
			{
			    float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2g
			{
			    float4 position : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct g2f
			{
			    float4 position : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2g vert (appdata v)
            {
                v2g o;
				// set all values in the v2g o to 0.0
				UNITY_INITIALIZE_OUTPUT(v2g, o);
				// setup the instanced id to be accessed
				UNITY_SETUP_INSTANCE_ID(v);
				// copy instance id in the appdata v to the v2g o
				UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.position = UnityObjectToClipPos(v.vertex);
                return o;
            }

			// Original Source: https://github.com/keijiro/Pcx/blob/master/Packages/jp.keijiro.pcx/Runtime/Shaders/Disk.cginc
			[maxvertexcount(36)]
			void geom(point v2g input[1], inout TriangleStream<g2f> outStream)
			{
				float4 origin = input[0].position;
				float2 extent = abs(UNITY_MATRIX_P._11_22 * _PointSize);
				// Copy the basic information.
			    g2f o;

				// set all values in the g2f o to 0.0
				UNITY_INITIALIZE_OUTPUT(g2f, o);
				// setup the instanced id to be accessed
				UNITY_SETUP_INSTANCE_ID(input[0]);
				// copy instance id in the v2f i[0] to the g2f o
				UNITY_TRANSFER_INSTANCE_ID(input[0], o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.position = input[0].position;

			    // Determine the number of slices based on the radius of the
			    // point on the screen.
			    float radius = extent.y / origin.w * _ScreenParams.y;
			    uint slices = min((radius + 1) / 5, 4) + 2;

			    // Slightly enlarge quad points to compensate area reduction.
			    // Hopefully this line would be complied without branch.
			    if (slices == 2)
			    {
				    extent *= 1.2;
			    }

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

			half4 frag(g2f i) : SV_Target
			{
				return _Color;
			}
		
		ENDCG
		}
	}
}