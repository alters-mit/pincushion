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

			#include "Pincushion.cginc"
			
			v2g vert (appdata v)
			{
				v2g o;
				// set all values in the v2g o to 0.0
				UNITY_INITIALIZE_OUTPUT(v2g, o);
				// setup the instanced id to be accessed
				UNITY_SETUP_INSTANCE_ID(v);
				// copy instance id in the appdata v to the v2g o
				UNITY_TRANSFER_INSTANCE_ID(v, o);
										
				o.vertex = v.vertex;
				
				#if _OCCLUDE_BACKFACING
				
				occlude_backfacing(UnityObjectToWorldNormal(v.normal), v, o);

				#endif
							
				return o;
			}
			
			ENDCG
			
		}
	}
}