#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

float4 FadeColor;
float2 ScreenSize;
int MaxSteps;
sampler s0;
float Progress;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION0;
	float4 Color : COLOR0;
	float2 Tex : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float2 texCoord = input.Tex;
	int tileSize = 1 + Progress * MaxSteps;
	if (tileSize > 1) {
		//Position in tile coordinates of current pixel
		int2 TilePos = input.Tex * ScreenSize / tileSize;

		//Back to texture coordinates of the top-left corner of the tile
		texCoord = (TilePos * tileSize) / ScreenSize;
	}
	
	float4 game = tex2D(s0, texCoord);

	game.rgb = game.rgb * (1 - Progress) + FadeColor.rgb * Progress;
	
	return game;
}

technique Fade
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};