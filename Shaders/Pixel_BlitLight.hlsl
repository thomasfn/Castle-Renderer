#include "Common.hlsli"

Texture2D ColourTexture : register(t0);
Texture2D NormalTexture : register(t1);
Texture2D ReflectionTexture : register(t2);
Texture2D DiffuseTexture : register(t3);
Texture2D SpecularTexture : register(t4);

SamplerState BlitSampler : register(s0);

BasicOutputPixel main(TexturedOutputVertex vertex) : SV_TARGET
{
	BasicOutputPixel output = (BasicOutputPixel)0;

	float4 colour = pow( ColourTexture.Sample(BlitSampler, vertex.TexCoord), 2.2 );
	float4 normal = NormalTexture.Sample(BlitSampler, vertex.TexCoord);
	// NOTE: We're doubling the colour here to compensate for halfing it in the light shaders. This is to allow "overlighting".
	float4 diffuse = lerp(float4(1.0, 1.0, 1.0, 1.0), DiffuseTexture.Sample(BlitSampler, vertex.TexCoord) * 2.0, normal.a);
	float4 specular = float4(SpecularTexture.Sample(BlitSampler, vertex.TexCoord).xyz + ReflectionTexture.Sample(BlitSampler, vertex.TexCoord).xyz, 1.0);



	//float3 add = saturate( specular.xyz );

	output.Colour = pow( float4(colour.xyz * diffuse.xyz + specular.xyz, colour.w), 1.0 / 2.2 );

	return output;
}