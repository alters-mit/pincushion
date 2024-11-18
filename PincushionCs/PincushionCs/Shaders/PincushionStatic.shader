// Render pincushions from MeshRenderers.
Shader "Pincushion/PincushionStatic" {
	SubShader {
			Tags{ "Queue" = "Overlay" "IgnoreProjector" = "True" "RenderType" = "Transparent" "DisableBatching" = "True" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Off
		
		Pass 
		{
			CGPROGRAM

			#include "PincushionStructs.cginc"

			inline float4 get_vertex(in appdata v, in uint vid)
			{
				return v.vertex;
			}

			#if _PINCUSHION_OCCLUDE_BACKFACING

			inline float3 get_normal(in appdata v, in uint vid)
			{
				return v.normal;
			}

			#endif

			#include "Pincushion.cginc"
			
			ENDCG
			
		}
	}
}