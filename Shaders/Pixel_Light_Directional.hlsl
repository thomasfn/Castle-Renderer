
#include "DeferredLight.hlsli"

CBUFFER_CAMERA(b0);

#include "LightingModels.hlsli"

cbuffer Directional : register(b1)
{
	float3 Colour;
	float3 Position;
	float3 Direction;
};

DeferredLightOutputPixel main(TexturedOutputVertex vertex) : SV_TARGET
{
	DeferredLightOutputPixel output = (DeferredLightOutputPixel)0;

		float4 position = PositionTexture.Sample(GBufferSampler, vertex.TexCoord);
		float4 normal = NormalTexture.Sample(GBufferSampler, vertex.TexCoord);
		float4 material = MaterialTexture.Sample(GBufferSampler, vertex.TexCoord);

		float diffuseterm, specularterm;

	cook_torrance(normal.xyz, position.xyz, Direction * -1.0, material.x, material.y, 0.8, diffuseterm, specularterm);

		// NOTE: We're halfing the colour here and compensating by doubling it in the light blit shader. This is to allow "overlighting".
	output.Diffuse = float4( Colour * diffuseterm * 0.5, 1.0 );
	output.Specular = float4(Colour * specularterm, 1.0);

	return output;
}