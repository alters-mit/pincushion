Shader "Pincushion/SampledPointsStatic" {
    Properties 
    {
        _Color ("Color", Color) = (0.9, 0.9, 0.9, 1.0)
    }


    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="TransparentCutout"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull back 
        LOD 100

        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;

            struct v2f
            {
                float4 position: SV_POSITION;
                float2 screenPosition: TEXCOORD0;
                fixed4 color: COLOR;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.screenPosition = ComputeScreenPos(o.position);
                o.color = _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.color; 
            }

            ENDCG
        }
    }
}