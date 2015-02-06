
#include "DeferredLight.hlsli"

Texture2D AOTexture : register(t5);

cbuffer Ambient : register(b0)
{
	float3 Colour;
};

DeferredLightOutputPixel main(TexturedOutputVertex vertex)
{
	DeferredLightOutputPixel output = (DeferredLightOutputPixel)0;

	float4 material = MaterialTexture.Sample(GBufferSampler, vertex.TexCoord);
	float occlusion = AOTexture.Sample(GBufferSampler, vertex.TexCoord).r;

	// NOTE: We're halfing the colour here and compensating by doubling it in the light blit shader. This is to allow "overlighting".
	output.Diffuse = float4(Colour * material.a * 0.5 * (1.0 - occlusion), 1.0);
	output.Specular = float4(0.0, 0.0, 0.0, 1.0);

	return output;
}