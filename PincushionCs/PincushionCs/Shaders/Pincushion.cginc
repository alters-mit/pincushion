uniform half4 _PincushionColor;
uniform half _PincushionPointSize;
uniform sampler2D _PincushionMainTex;
static float2 pointUvs[4] = { float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 1) };

#if _OCCLUDE_BEHIND

uniform sampler2D _PincushionDistanceTex;
						
#endif

#if _SHOW_EVERY_NTH

Buffer<uint> _PincushionNthMask;

#endif

v2g vert (appdata v, uint vid : SV_VertexID)
{
	v2g o;
	// set all values in the v2g o to 0.0
	UNITY_INITIALIZE_OUTPUT(v2g, o);
	// setup the instanced id to be accessed
	UNITY_SETUP_INSTANCE_ID(v);
	// copy instance id in the appdata v to the v2g o
	UNITY_TRANSFER_INSTANCE_ID(v, o);
										
	o.vertex = get_vertex(v, vid);
				
	#if _OCCLUDE_BACKFACING

	// Hide points facing away from the camera.
	// Source: https://discussions.unity.com/t/camera-forward-vector-in-shader/32664/4
	half3 worldVert = mul(unity_ObjectToWorld, v.vertex);
	half3 viewDir = _WorldSpaceCameraPos - worldVert;
	if (dot(viewDir, get_normal(v, vid)) > 0) {
		o.color = _PincushionColor;
	}
	else
	{
		o.color = float4(0, 0, 0, 0);
	}

	#elif _SHOW_EVERY_NTH

	o.color = _PincushionColor;

	#endif

	#if _SHOW_EVERY_NTH

	// Only show every nth vertex.
	if (!_PincushionNthMask[vid])
	{
		o.color = float4(0, 0, 0, 0);
	}
	
	#endif
							
	return o;
}

[maxvertexcount(4)]
void geom(point v2g p[1], inout TriangleStream<g2f> triStream)
{
	float3 right = normalize(UNITY_MATRIX_IT_MV[0].xyz);
	float3 up = normalize(UNITY_MATRIX_IT_MV[1].xyz);
	float distanceToCamera = distance(mul(unity_ObjectToWorld, p[0].vertex), _WorldSpaceCameraPos);

	#if _OCCLUDE_BEHIND

	// To occlude behind, we need the point's coordinates on the pre-rendered distance texture.
	// We assume that the size of the distance texture matches that of the screen.
	float4 screenPositionFull = ComputeScreenPos(UnityObjectToClipPos(p[0].vertex));
	const float2 distanceUv = screenPositionFull.xy / screenPositionFull.w;

	#endif

	#if _CONSTANT_SCALING
							
	// Keep a constant scale regardless of distance.
	float scale = distanceToCamera * 0.1;

	#else

	float scale = 1;

	#endif

	// Handle eventual rescaling of the renderer by computing average scale.
	float3 objectScale = float3(length( unity_ObjectToWorld[0].xyz),
		length(unity_ObjectToWorld[1].xyz),
		length(unity_ObjectToWorld[2].xyz));
	float averageScale = (objectScale.x + objectScale.y + objectScale.z) / 3.0;

	scale /= averageScale;
	right *= _PincushionPointSize * scale;
	up *= _PincushionPointSize * scale;

	// Define the four vertices for the billboard in world space.
	float4 v[4];
	v[0] = float4(p[0].vertex + right - up, 1.0f);
	v[1] = float4(p[0].vertex + right + up, 1.0f);
	v[2] = float4(p[0].vertex - right - up, 1.0f);
	v[3] = float4(p[0].vertex - right + up, 1.0f);

	g2f o;
	UNITY_INITIALIZE_OUTPUT(g2f, o);
	UNITY_SETUP_INSTANCE_ID(i);
	UNITY_TRANSFER_INSTANCE_ID(i, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	for (int j = 0; j < 4; j++)
	{
		o.vertex = UnityObjectToClipPos(v[j]);
		// The UVs never change.
		o.uv = pointUvs[j];
		
		#if _OCCLUDE_BACKFACING || _SHOW_EVERY_NTH

		o.color = p[0].color;

		#endif
					
		#if _OCCLUDE_BEHIND

		o.distanceUv = distanceUv;
		o.distance = distanceToCamera;

		#endif

		// Add the vertex.
		triStream.Append(o);
	}
}

half4 frag(g2f i) : SV_Target
{

	#if _OCCLUDE_BACKFACING || _SHOW_EVERY_NTH

	float4 color = i.color;

	#else

	float4 color = _PincushionColor;
	
	#endif
				
	#if _OCCLUDE_BACKFACING

	// The color was set via calculating the normal.
	return tex2D(_PincushionMainTex, i.uv) * color;

	#elif _OCCLUDE_BEHIND

	// Sample the distance texture and compare to the vertex's distance.
	if (i.distance < tex2D(_PincushionDistanceTex, i.distanceUv).r + 0.01)
	{
		return tex2D(_PincushionMainTex, i.uv) * color;
	}
	return float4(0, 0, 0, 0);

	#else

	return tex2D(_PincushionMainTex, i.uv) * color;

	#endif
							
}