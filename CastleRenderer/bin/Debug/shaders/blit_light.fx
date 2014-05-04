
Texture2D texColour;
Texture2D texDiffuseLight;
Texture2D texSpecularLight;
sampler smpTexture;

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
	float4 Colour : SV_Target0;
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
	PShaderOutput output;
	
	float4 colour = texColour.Sample( smpTexture, input.TexCoord );
	// NOTE: We're doubling the colour here to compensate for halfing it in the light shaders. This is to allow "overlighting".
	float4 diffuse = texDiffuseLight.Sample( smpTexture, input.TexCoord ) * 2.0;
	float4 specular = texSpecularLight.Sample( smpTexture, input.TexCoord );
	
	//float3 add = saturate( specular.xyz );
	
	output.Colour = float4( colour.xyz * diffuse.xyz + specular.xyz, colour.w );
	
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