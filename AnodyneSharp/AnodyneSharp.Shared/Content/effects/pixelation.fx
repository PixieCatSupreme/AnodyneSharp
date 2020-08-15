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

sampler s0;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION0;
	float4 Color : COLOR0;
	float2 Tex : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	//Position in tile coordinates of current pixel
	int2 TilePos = input.Tex * ScreenSize / StrideSize;

	//Back to texture coordinates of the top-left corner of the tile
	float2 texCoord = (TilePos * StrideSize + float2(0.5,0.5)) / ScreenSize;
	
	float4 game = tex2D(s0, texCoord);
	
	return game;
}

technique Pixelate
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};