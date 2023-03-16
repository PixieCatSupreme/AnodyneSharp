#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

int phase_offset;

sampler2D Screen : register(s0) {
	AddressU = CLAMP;
	AddressV = CLAMP;
	Filter = Point;
};

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
	int yloc = ((int)input.Position.y - phase_offset + 180) % 180;
	
	int phase = (yloc / 15) % 12 - 6;
	input.Position.x += 3 - abs(phase);
	return tex2D(Screen, input.Position.xy / float2(160,180));
}

technique Wave
{
	pass P0
	{
        VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};