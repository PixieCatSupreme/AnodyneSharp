#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

float2 ScreenSize;
int StrideSize;

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
	//Position in tile coordinates of current pixel
	int2 TilePos = input.Tex * ScreenSize / StrideSize;

	//Back to texture coordinates of the top-left corner of the tile
	float2 texCoord = (TilePos * StrideSize + float2(0.5,0.5)) / ScreenSize;
	
	float4 game = tex2D(TextureSampler, texCoord);
	
	return game;
}

technique Pixelate
{
	pass P0
	{
        VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};