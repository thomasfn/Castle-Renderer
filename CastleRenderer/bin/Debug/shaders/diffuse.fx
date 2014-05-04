
float4x4 transform;
float4x4 projectionview;

Texture2D diffuse;
sampler texturesampler;

float alphatest = 0.0;

float2 textureoffset, texturescale;

float specular_hardness, specular_power;

float useclip;
float4 clip;

struct VShaderInput
{
    float3 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
};

struct VShaderOutput
{
    float4 Position : SV_POSITION;
	float4 WorldPosition : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
};

struct PShaderOutput
{
	float4 Colour : SV_Target0;
	float4 Normal : SV_Target1;
	float4 Position : SV_Target2;
	float4 Material : SV_Target3;
};

VShaderOutput VShader( VShaderInput input )
{
    VShaderOutput output;

    output.WorldPosition = mul( float4( input.Position, 1.0 ), transform );
    output.Position = mul( output.WorldPosition, projectionview );
	output.Normal = normalize( mul( input.Normal, (float3x3)transform ) );
	output.TexCoord = textureoffset + input.TexCoord * texturescale;

    return output;
}

PShaderOutput PShader( VShaderOutput input )
{
	PShaderOutput output;
	
	if (useclip > 0.5 && dot( input.WorldPosition.xyz, clip.xyz ) < -clip.w) discard;
	
	float4 diffusesample = diffuse.Sample( texturesampler, input.TexCoord );
	if (diffusesample.w < alphatest) discard;
	
	output.Colour = float4( diffusesample.xyz, 1.0 );
	output.Normal = float4( input.Normal, 1.0 );
	output.Position = input.WorldPosition;
	output.Material = float4( specular_hardness, specular_power, 0.0, 0.0 );
	
	return output;
}

technique10 Render
{
    pass P0
    {
		SetVertexShader( CompileShader( vs_4_0, VShader() ) );
		SetPixelShader( CompileShader( ps_4_0, PShader() ) );
    }
}