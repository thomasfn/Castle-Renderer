
float4x4 transform;
float4x4 projectionview;

Texture2D diffuse;
Texture2D normalmap;
sampler texturesampler;

float height, specularfromalpha;

float2 textureoffset, texturescale;

float specular_hardness, specular_power;

float useclip;
float4 clip;

struct VShaderInput
{
    float3 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
	float3 Tangent : TANGENT0;
};

struct VShaderOutput
{
    float4 Position : SV_POSITION;
	float4 WorldPosition : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
	float3 Tangent : TANGENT0;
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
	output.Normal = mul( input.Normal, (float3x3)transform );
	output.Tangent = mul( input.Tangent, (float3x3)transform );
	output.TexCoord = textureoffset + input.TexCoord * texturescale;

    return output;
}

float3 UnpackNormal( float3 normal )
{
	return normal.xzy * 2.0 - 1.0;
}

PShaderOutput PShader( VShaderOutput input )
{
	PShaderOutput output;
	
	if (useclip > 0.5 && dot( input.WorldPosition.xyz, clip.xyz ) < -clip.w) discard;
	
	float3x3 normalmatrix = { -cross( input.Tangent, input.Normal ), input.Normal, -input.Tangent };
	float3 localnormal = UnpackNormal( normalmap.Sample( texturesampler, input.TexCoord ).xyz );
	
	float4 diffusesample = diffuse.Sample( texturesampler, input.TexCoord );
	float specular = lerp( 1.0, diffusesample.w, specularfromalpha );
	
	float3 worldnormal = normalize( mul( localnormal * float3( height, 1.0, height ), normalmatrix ) );
	
	output.Colour = float4( diffusesample.xyz, 1.0 );
	output.Normal = float4( worldnormal, 1.0 );
	output.Position = input.WorldPosition;
	output.Material = float4( specular_hardness, specular_power * specular, 0.0, 0.0 );
	
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