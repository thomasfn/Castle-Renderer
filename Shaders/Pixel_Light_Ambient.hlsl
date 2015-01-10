
#include "DeferredLight.hlsli"

cbuffer Ambient : register(b1)
{
	float3 Colour;
};

DeferredLightOutputPixel main(TexturedOutputVertex vertex) : SV_TARGET
{
	DeferredLightOutputPixel output = (DeferredLightOutputPixel)0;

	float4 normal = NormalTexture.Sample(GBufferSampler, vertex.TexCoord);

	// NOTE: We're halfing the colour here and compensating by doubling it in the light blit shader. This is to allow "overlighting".
	output.Diffuse = float4(lerp(float3(1.0, 1.0, 1.0), Colour * 0.5, normal.w), 1.0);
	output.Specular = float4(0.0, 0.0, 0.0, 1.0);

	return output;
}