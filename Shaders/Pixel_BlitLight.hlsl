#include "Common.hlsli"

Texture2D ColourTexture : register(t0);
Texture2D DiffuseTexture : register(t1);
Texture2D SpecularTexture : register(t2);

SamplerState BlitSampler : register(s0);

BasicOutputPixel main(TexturedOutputVertex vertex) : SV_TARGET
{
	BasicOutputPixel output = (BasicOutputPixel)0;

	float4 colour = pow( ColourTexture.Sample(BlitSampler, vertex.TexCoord), 2.2 );
	// NOTE: We're doubling the colour here to compensate for halfing it in the light shaders. This is to allow "overlighting".
	float4 diffuse = DiffuseTexture.Sample(BlitSampler, vertex.TexCoord) * 2.0;
	float4 specular = SpecularTexture.Sample(BlitSampler, vertex.TexCoord);



	//float3 add = saturate( specular.xyz );

	output.Colour = pow( float4(colour.xyz * diffuse.xyz + specular.xyz, colour.w), 1.0 / 2.2 );

	return output;
}