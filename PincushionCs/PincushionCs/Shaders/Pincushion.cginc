uniform half4 _PincushionColor;
uniform half _PincushionPointSize;
uniform sampler2D _PincushionMainTex;

// Vertex-to-geometry initialization code used by both Pincushion and PincushionReplacement
inline void start_v2g(in appdata v, out v2g o)
{
	// set all values in the v2g o to 0.0
	UNITY_INITIALIZE_OUTPUT(v2g, o);
	// setup the instanced id to be accessed
	UNITY_SETUP_INSTANCE_ID(v);
	// copy instance id in the appdata v to the v2g o
	UNITY_TRANSFER_INSTANCE_ID(v, o);
				
	o.vertex = v.vertex;
}

// Geometry-to-fragment initialization code used by both Pincushion and PincushionReplacement
inline g2f start_g2f(in v2g i, out float distanceToCamera, out float4 v[4])
{
	float3 right = normalize(UNITY_MATRIX_IT_MV[0].xyz);
	float3 up = normalize(UNITY_MATRIX_IT_MV[1].xyz);
	
	distanceToCamera = distance(mul(unity_ObjectToWorld, i.vertex), _WorldSpaceCameraPos);

	#if _CONSTANT_SCALING
	
	// Keep a constant scale regardless of distance.
	float scale = distanceToCamera * 0.1;

	#else

	float scale = 1;

	#endif

	// Handle eventual rescaling of the renderer by computing average scale.
	float3 scaleX = unity_ObjectToWorld[0].xyz;
	float3 scaleY = unity_ObjectToWorld[1].xyz;
	float3 scaleZ = unity_ObjectToWorld[2].xyz;
	float3 objectScale = float3(length(scaleX), length(scaleY), length(scaleZ));
	float averageScale = (objectScale.x + objectScale.y + objectScale.z) / 3.0;

	scale /= averageScale;
	right *= _PincushionPointSize * scale;
	up *= _PincushionPointSize * scale;

	// Define the four vertices for the billboard in world space.
	v[0] = float4(i.vertex + right - up, 1.0f);
	v[1] = float4(i.vertex + right + up, 1.0f);
	v[2] = float4(i.vertex - right - up, 1.0f);
	v[3] = float4(i.vertex - right + up, 1.0f);

	g2f o;
	UNITY_INITIALIZE_OUTPUT(g2f, o);
	UNITY_SETUP_INSTANCE_ID(i);
	UNITY_TRANSFER_INSTANCE_ID(i, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	return o;
}