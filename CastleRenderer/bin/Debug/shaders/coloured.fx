
float4x4 transform;
float4x4 projectionview;
float3 colour;

float specular_hardness, specular_power;

struct VShaderInput
{
    float3 Position : POSITION0;
	float3 Normal : NORMAL0;
	//float2 TexCoord : TEXCOORD0;
	//float3 Tangent : TANGENT0;
};

struct VShaderOutput
{
    float4 Position : SV_POSITION;
	float4 WorldPosition : POSITION0;
	float3 Normal : NORMAL0;
	//float2 TexCoord : TEXCOORD0;
	//float3 Tangent : TANGENT0;
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
	//output.TexCoord = input.TexCoord;
	//output.Tangent = input.Tangent;

    return output;
}

PShaderOutput PShader( VShaderOutput input )
{
	PShaderOutput output;
	
	output.Colour = float4( colour, 1.0 );
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