#if OPENGL
#define SV_POSITION POSITION
#define PS_SHADERMODEL ps_3_0
#else
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler s0;

float4 MainPS(float2 input : TEXCOORD0) : COLOR
{
	float4 color = tex2D(s0, input);
	float average = (0.2126 * color.r) + (0.7152 * color.g) + (0.0722 * color.b);

	color.rgb = average;
	return color;
}

technique BasicColorDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};