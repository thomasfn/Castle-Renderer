
#include "DeferredLight.hlsli"

CBUFFER_CAMERA(b0);

#include "LightingModels.hlsli"

cbuffer Point : register(b1)
{
	float3 Colour;
	float3 Position;
	float Range;
};

DeferredLightOutputPixel main(TexturedOutputVertex vertex)
{
	DeferredLightOutputPixel output = (DeferredLightOutputPixel)0;

	float4 position = PositionTexture.Sample(GBufferSampler, vertex.TexCoord);
	float4 normal = NormalTexture.Sample(GBufferSampler, vertex.TexCoord);
	float4 material = MaterialTexture.Sample(GBufferSampler, vertex.TexCoord);

	float diffuseterm, specularterm;

	float3 between = Position - position.xyz;
	float dist = length(between);
	float3 lightvec = between / dist;
	float f = dist / Range;
	float falloff = 1.0 - saturate(f * f);

	cook_torrance(normal.xyz, position.xyz, lightvec, material.x, material.y, material.z, diffuseterm, specularterm);

	// NOTE: We're halfing the colour here and compensating by doubling it in the light blit shader. This is to allow "overlighting".
	output.Diffuse = float4(Colour * diffuseterm * 0.5 * falloff, 1.0);
	output.Specular = float4(Colour * specularterm * falloff, 1.0);

	return output;
}