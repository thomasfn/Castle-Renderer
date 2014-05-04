
Texture2D texImage;
sampler smpTexture;

float2 blursize;
float2 imagesize;

float threshold;
float originalintensity, bloomintensity;
float originalsaturation, bloomsaturation;

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

float3 Contribution( float2 uv )
{
	float3 sample = texImage.Sample( smpTexture, uv ).xyz;
	return saturate( (sample - threshold) / (1 - threshold) );
}

// http://digitalerr0r.wordpress.com/2009/10/04/xna-shader-programming-tutorial-24-bloom/

float3 AdjustSaturation( float3 colour, float saturation )
{
    float grey = dot( colour, float3(0.3, 0.59, 0.11) );
    return lerp( grey, colour, saturation );
}

PShaderOutput PShader( VShaderOutput input )
{
	PShaderOutput output;
	
	float3 originalcolour = texImage.Sample( smpTexture, input.TexCoord ).xyz;
	
	float2 size = blursize / imagesize;
	
	float3 bloomcolour = float3( 0.0, 0.0, 0.0 );
	bloomcolour += Contribution( input.TexCoord - 4.0 * size ) * 0.05;
	bloomcolour += Contribution( input.TexCoord - 3.0 * size ) * 0.09;
	bloomcolour += Contribution( input.TexCoord - 2.0 * size ) * 0.12;
	bloomcolour += Contribution( input.TexCoord - size ) * 0.15;
	bloomcolour += saturate( (originalcolour - threshold) / (1 - threshold) ) * 0.16;
	bloomcolour += Contribution( input.TexCoord + size ) * 0.15;
	bloomcolour += Contribution( input.TexCoord + 2.0 * size ) * 0.12;
	bloomcolour += Contribution( input.TexCoord + 3.0 * size ) * 0.09;
	bloomcolour += Contribution( input.TexCoord + 4.0 * size ) * 0.05;
	
	bloomcolour = AdjustSaturation( bloomcolour, bloomsaturation) * bloomintensity;
	originalcolour = AdjustSaturation( originalcolour, originalsaturation) * originalintensity;
	
	originalcolour *= (1.0 - saturate( bloomcolour ));
	output.Colour = float4( originalcolour + bloomcolour, 1.0 );
	
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