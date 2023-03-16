#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

float blendOverlay(float base, float blend) {
	return base < 0.5 ? (2.0 * base * blend) : (1.0 - 2.0 * (1.0 - base) * (1.0 - blend));
}

float3 blendOverlay(float3 base, float3 blend) {
	return float3(blendOverlay(base.r, blend.r), blendOverlay(base.g, blend.g), blendOverlay(base.b, blend.b));
}

sampler2D TextureSampler : register(s0);

matrix World;
matrix View;
matrix Projection;

texture OverlayTex;
sampler2D OverlaySampler = sampler_state {
	Texture = <OverlayTex>;
};

bool HardLight;

texture DepthTex;
sampler2D DepthSampler = sampler_state {
	Texture = <DepthTex>;
};

float DepthCutoff;

struct VsInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 TexureCoordinateA : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION0;
	float4 Color : COLOR0;
	float2 Tex : TEXCOORD0;
};

VertexShaderOutput MainVS(VsInput input)
{
	VertexShaderOutput output;
	float4x4 wvp = mul(World, mul(View, Projection));
	output.Position = mul(input.Position, wvp); // Transform by WorldViewProjection
	output.Color = input.Color;
	output.Tex = input.TexureCoordinateA;
	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 color = tex2D(TextureSampler, input.Tex);
	if (DepthCutoff > 0.f && tex2D(DepthSampler, input.Tex).x <= DepthCutoff)
		return color;
	float4 blend = tex2D(OverlaySampler, input.Tex);
	if (blend.a == 0)
		return color;
	blend.rgb /= blend.a;
	blend.rgb = HardLight ? blendOverlay(blend.rgb, color.rgb) : blendOverlay(color.rgb, blend.rgb);
	color.rgb = blend.rgb * blend.a + color.rgb * (1 - blend.a);
	return color;
}

technique Blend
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};