﻿#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

float4 FadeColor;
float2 ScreenSize;
int StrideSize;
sampler s0;
float Fade;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION0;
	float4 Color : COLOR0;
	float2 Tex : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float2 texCoord = input.Tex;
	if (StrideSize > 1) {
		//Position in tile coordinates of current pixel
		int2 TilePos = input.Tex * ScreenSize / StrideSize;

		//Back to texture coordinates of the top-left corner of the tile
		texCoord = (TilePos * StrideSize) / ScreenSize;
	}
	
	float4 game = tex2D(s0, texCoord);

	game.rgb = game.rgb * (1 - Fade) + FadeColor.rgb * Fade;
	
	return game;
}

technique Fade
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};