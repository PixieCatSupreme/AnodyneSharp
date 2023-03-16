#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

int step;

sampler s0;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION0;
	float4 Color : COLOR0;
	float2 Tex : TEXCOORD0;
};

matrix Projection;

struct VsInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexureCoordinateA : TEXCOORD0;
};

VertexShaderOutput MainVS(VsInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, Projection); // Transform by WorldViewProjection
    output.Color = input.Color;
    output.Tex = input.TexureCoordinateA;
    return output;
}

float rand(float2 uv) {
	//Constants based on irrational numbers for better distribution
	const float2 k = float2(23.1406926327792690, 2.6651441426902251);
	return frac(cos(dot(uv + step, k)) * 12345.6789);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 color = tex2D(s0, input.Tex);

	float alpha = rand(input.Tex) * 0.31;
	float gray = rand(input.Tex + float2(1, 1)) * 0.31;

	color.rgb = color.rgb * (1 - alpha) + gray * alpha;

	return color;
}

technique AddStatic
{
	pass P0
	{
        VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};