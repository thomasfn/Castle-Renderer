
float4x4 transform;
float4x4 projectionview;

Texture2D texReflection;
Texture2D texNormal;
sampler normaltexturesampler;
sampler texturesampler;

float3 colour;

float2 textureoffset, texturescale;

float specular_hardness, specular_power;

float3 camera_position;

float time;

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
	float2 ScreenTexCoord : TEXCOORD1;
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
	output.TexCoord = input.TexCoord;
	output.ScreenTexCoord = output.Position.xy;

    return output;
}

float3 UnpackNormal( float3 normal )
{
	return normal.xzy * 2.0 - 1.0;
}

PShaderOutput PShader( VShaderOutput input )
{
	PShaderOutput output;
	
	float2 texcoord = textureoffset + texturescale * input.TexCoord;
	
	float3 sample1 = texNormal.Sample( normaltexturesampler, texcoord + float2( time * 0.0517642, 0.0 ) ).xyz;
	float3 sample2 = texNormal.Sample( normaltexturesampler, texcoord - float2( 0.0, time * 0.0491784 ) ).xyz;
	
	float fresnel = -dot( input.Normal, normalize( input.WorldPosition - camera_position ) );
	
	float3 texnorm = lerp( sample1, sample2, 0.5 );
	float3x3 normalmatrix = { -cross( input.Tangent, input.Normal ), input.Normal, -input.Tangent };
	float3 localnormal = UnpackNormal( texnorm );
	
	float2 device = input.ScreenTexCoord / input.Position.w;
	float2 uv = (device + 1.0) * 0.5 + localnormal.xz * 0.01;
	
	
	
	float4 reflectedcolour = texReflection.Sample( texturesampler, uv );
	
	output.Colour = float4( lerp( colour, reflectedcolour.xyz, 1.0 - fresnel ), 1.0 );
	output.Normal = float4( lerp( float3( 0.0, 1.0, 0.0 ), localnormal, fresnel ), 1.0 );
	output.Position = input.WorldPosition + float4( localnormal * 0.25, 0.0 );
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