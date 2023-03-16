#if OPENGL
#define SV_POSITION POSITION
#define PS_SHADERMODEL ps_3_0
#define VS_SHADERMODEL vs_3_0
#else
#define PS_SHADERMODEL ps_4_0
#define VS_SHADERMODEL vs_4_0
#endif

sampler2D TextureSampler : register(s0);

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

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 color = tex2D(TextureSampler, input.Tex);
	float average = (color.r+color.g+color.b)/3;

	average = int(average * 25) / 25.0;

	color.rgb = average;
	return color;
}

technique BasicColorDrawing
{
	pass P0
	{
        VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};