
Texture2D texNormal;
Texture2D texPosition;
Texture2D texMaterial;
sampler smpTexture;

float3 colour;

float3 camera_position;
float3 camera_forward;

struct VShaderInput
{
    float3 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VShaderOutput
{
    float4 Position : SV_POSITION;
	float2 TexCoord : TEXCOORD0;
};

struct PShaderOutput
{
	float4 Diffuse : SV_Target0;
	float4 Specular : SV_Target1;
};

VShaderOutput VShader( VShaderInput input )
{
    VShaderOutput output;

    output.Position = float4( input.Position, 1.0 );
	output.TexCoord = input.TexCoord;

    return output;
}

PShaderOutput PShader( VShaderOutput input )
{
	float4 normal = texNormal.Sample( smpTexture, input.TexCoord );

	PShaderOutput output;
	
	// NOTE: We're halfing the colour here and compensating by doubling it in the light blit shader. This is to allow "overlighting".
	output.Diffuse = float4( lerp( float3( 1.0, 1.0, 1.0 ), colour * 0.5, normal.w ), 1.0 );
	output.Specular = float4( 0.0, 0.0, 0.0, 1.0 );
	
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