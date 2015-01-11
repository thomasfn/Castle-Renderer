#include "Common.hlsli"

Texture2D UpTexture : register(t0);
Texture2D DownTexture : register(t1);
Texture2D LeftTexture : register(t2);
Texture2D RightTexture : register(t3);
Texture2D ForwardTexture : register(t4);
Texture2D BackTexture : register(t5);

SamplerState SkyboxSampler : register(s0);

BasicOutputPixel main(TexturedNormalOutputVertex vertex) : SV_TARGET
{
	BasicOutputPixel output = (BasicOutputPixel)0;

	float3 upsample = UpTexture.Sample(SkyboxSampler, vertex.TexCoord).xyz;
	float3 downsample = DownTexture.Sample(SkyboxSampler, vertex.TexCoord).xyz;
	float3 leftsample = LeftTexture.Sample(SkyboxSampler, vertex.TexCoord).xyz;
	float3 rightsample = RightTexture.Sample(SkyboxSampler, vertex.TexCoord).xyz;
	float3 forwardsample = ForwardTexture.Sample(SkyboxSampler, vertex.TexCoord).xyz;
	float3 backsample = BackTexture.Sample(SkyboxSampler, vertex.TexCoord).xyz;

	output.Colour = float4(
		vertex.WorldNormal.x > 0.5 ? rightsample : vertex.WorldNormal.x < -0.5 ? leftsample :
		vertex.WorldNormal.y > 0.5 ? upsample : vertex.WorldNormal.y < -0.5 ? downsample :
		vertex.WorldNormal.z > 0.5 ? forwardsample : backsample,
		//1.0,
		//1.0,
		//1.0,
		1.0);

	return output;
}