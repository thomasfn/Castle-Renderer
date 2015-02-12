
#include "DeferredLight.hlsli"

CBUFFER_CAMERA(b0);

cbuffer Directional : register(b1)
{
	float3 Colour;
	float3 Position;
	float3 Direction;
	row_major float4x4 ShadowMatrix;
	float UseShadowMapping;
};

#include "LightingModels.hlsli"
#include "VarianceShadowMapping.hlsli"

DeferredLightOutputPixel main(TexturedOutputVertex vertex)
{
	DeferredLightOutputPixel output = (DeferredLightOutputPixel)0;

	float4 position = PositionTexture.Sample(GBufferSampler, vertex.TexCoord);
	float4 normal = NormalTexture.Sample(GBufferSampler, vertex.TexCoord);
	float4 material = MaterialTexture.Sample(GBufferSampler, vertex.TexCoord);

	float shadowterm;
	if (UseShadowMapping > 0.5)
		shadowterm = SampleShadowMap(position.xyz);
	else
		shadowterm = 1.0f;

	float diffuseterm = 0.0, specularterm = 0.0;

	if (shadowterm > 0.0)
		cook_torrance(normal.xyz, position.xyz, Direction * -1.0, material.x, material.y, material.z, diffuseterm, specularterm);

	// NOTE: We're halfing the colour here and compensating by doubling it in the light blit shader. This is to allow "overlighting".
	output.Diffuse = float4(Colour * diffuseterm * shadowterm * 0.5, 1.0);
	output.Specular = float4(Colour * specularterm * shadowterm, 1.0);

	return output;
}