#include "UnityCG.cginc"

struct appdata
{
    float4 vertex : POSITION;

    #if _OCCLUDE_BACKFACING

    // This is used to determine if a point is backfacing.
    float4 normal: NORMAL;

    #endif
							
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2g
{
    float4 vertex : SV_POSITION;

    #if _OCCLUDE_BACKFACING || _SKIP_EVERY_NTH
				
    // To hide a backfacing point, set its color to (0, 0, 0, 0).
    // Otherwise, this will be the _PincushionColor
    float4 color: COLOR;

    #endif
							
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct g2f
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;

    #if _OCCLUDE_BACKFACING

    // The color from v2g.
    float4 color: COLOR;
							
    #endif

    #if _OCCLUDE_BEHIND
							
    // The distance texture UV.
    float2 distanceUv : TEXCOORD1;
    // The actual distance.
    float distance: TEXCOORD2;

    #endif
							
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};
