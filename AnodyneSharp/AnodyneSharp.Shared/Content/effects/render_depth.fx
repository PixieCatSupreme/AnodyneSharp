#if OPENGL
#define SV_POSITION POSITION
#define PS_SHADERMODEL ps_3_0
#define VS_SHADERMODEL vs_3_0
#else
#define PS_SHADERMODEL ps_4_0
#define VS_SHADERMODEL vs_4_0
#endif

sampler2D TextureSampler : register(s0);

matrix World;
matrix View;
matrix Projection;

//_______________________________________________________________
// techniques 
// Quad Draw  Position Color Texture
//_______________________________________________________________
struct VsInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexureCoordinateA : TEXCOORD0;
};

struct Vs2Ps
{
    float4 Position : SV_Position;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
    float2 Z : TEXCOORD1; //Hack to let the pixel shader read the Z value of Position in shadermodel 3.
};

// ____________________________
Vs2Ps MainVS(VsInput input)
{
    Vs2Ps output;
    float4x4 wvp = mul(World, mul(View, Projection));
    output.Position = mul(input.Position, wvp); // Transform by WorldViewProjection
    output.Color = input.Color;
    output.TexCoord = input.TexureCoordinateA;
    output.Z = output.Position.z;
    return output;
}

struct PSOutput {
    float4 c0 : COLOR0;
    float4 c1 : COLOR1;
};

PSOutput MainPS(Vs2Ps input)
{
    PSOutput o;
    o.c0 = tex2D(TextureSampler, input.TexCoord) * input.Color;
    o.c1 = float4(input.Z.x,0,0,1);
    clip(o.c0.a - 0.2);
    return o;
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};