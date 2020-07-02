#if OPENGL
#define SV_POSITION POSITION
#define PS_SHADERMODEL ps_3_0
#define VS_SHADERMODEL vs_3_0
#else
#define PS_SHADERMODEL ps_4_0
#define VS_SHADERMODEL vs_4_0
#endif

matrix World;
matrix View;
matrix Projection;

float OverlayZ;

sampler s0;

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

float4 MainPS(Vs2Ps input) : COLOR
{
    float4 color = tex2D(s0,input.TexCoord);

    if (input.Z.x < OverlayZ && input.Position.y > 80) {
        color.rgb = color.a - color.rgb;
    }

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