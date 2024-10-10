// Source: https://raw.githubusercontent.com/keijiro/Pcx/refs/heads/master/Packages/jp.keijiro.pcx/Runtime/Shaders/Point.shader

Shader "Pincushion/PointsShaderStatic"
{
    Properties
    {
        _Color("Color", Color) = (0.9, 0.9, 0.9, 1)
        _PointSize("Point Size", Float) = 0.02
        [Toggle] _Distance("Apply Distance", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM

            #pragma vertex Vertex
            #pragma fragment Fragment

            #pragma multi_compile_fog
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #pragma multi_compile _ _DISTANCE_ON
            #pragma multi_compile _ _COMPUTE_BUFFER

            #include "UnityCG.cginc"
            #include "Common.cginc"

            struct Attributes
            {
                float4 position : POSITION;
            };

            struct Varyings
            {
                float4 position : SV_Position;
                half psize : PSIZE;
                UNITY_FOG_COORDS(0)
            };

            float4x4 _Transform;
            half _PointSize;

        #if _COMPUTE_BUFFER
            StructuredBuffer<float3> _PointsBuffer;
        #endif

        #if _COMPUTE_BUFFER
            Varyings Vertex(uint vid : SV_VertexID)
        #else
            Varyings Vertex(Attributes input)
        #endif
            {
            #if _COMPUTE_BUFFER
                float3 pt = _PointsBuffer[vid];
                float4 pos = mul(_Transform, float4(pt, 1));
            #else
                float4 pos = input.position;
            #endif

                Varyings o;
                o.position = UnityObjectToClipPos(pos);
            #ifdef _DISTANCE_ON
                o.psize = _PointSize / o.position.w * _ScreenParams.y;
            #else
                o.psize = _PointSize;
            #endif
                UNITY_TRANSFER_FOG(o, o.position);
                return o;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_APPLY_FOG(input.fogCoord, _Color);
                return _Color;
            }

            ENDCG
        }
    }
}
