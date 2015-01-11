
float4x4 transform;
float4x4 projectionview;

Texture2D up, down, left, right, forward, back;
sampler texturesampler;

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
};

VShaderOutput VShader( VShaderInput input )
{
    VShaderOutput output;

    output.WorldPosition = mul( float4( input.Position, 1.0 ), transform );
    output.Position = mul( output.WorldPosition, projectionview );
	output.Normal = mul( input.Normal, (float3x3)transform );
	output.TexCoord = input.TexCoord;

    return output;
}

PShaderOutput PShader( VShaderOutput input )
{
	PShaderOutput output;
	
	float3 upsample = up.Sample( texturesampler, input.TexCoord ).xyz;
	float3 downsample = down.Sample( texturesampler, input.TexCoord ).xyz;
	float3 leftsample = left.Sample( texturesampler, input.TexCoord ).xyz;
	float3 rightsample = right.Sample( texturesampler, input.TexCoord ).xyz;
	float3 forwardsample = forward.Sample( texturesampler, input.TexCoord ).xyz;
	float3 backsample = back.Sample( texturesampler, input.TexCoord ).xyz;
	
	float3 colour =
		input.Normal.x > 0.5 ? rightsample : input.Normal.x < -0.5 ? leftsample :
		input.Normal.y > 0.5 ? upsample : input.Normal.y < -0.5 ? downsample :
		input.Normal.z > 0.5 ? forwardsample : backsample;
	
	//output.Colour = float4( colour, 1.0 );
	output.Colour = float4( input.TexCoord.xy, 0.0, 1.0 );
	
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