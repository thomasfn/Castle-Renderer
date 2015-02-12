
#include "DeferredLight.hlsli"

CBUFFER_CAMERA(b0);

cbuffer Spot : register(b1)
{
	float3 Colour;
	float3 Direction;
	float3 Position;
	float Range;
	float Angle;
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

	float diffuseterm, specularterm;

	float3 between = Position - position.xyz;
	float dist = length(between);
	float3 lightvec = between / dist;
	float f = dist / Range;
	float falloff = 1.0 - saturate(f * f);

	float shadowterm;
	if (UseShadowMapping > 0.5)
		shadowterm = SampleShadowMap(position.xyz);
	else
		shadowterm = 1.0f;

	cook_torrance(normal.xyz, position.xyz, lightvec, material.x, material.y, material.z, diffuseterm, specularterm);

	const float lip = 0.1;

	float spotfactor = -dot(lightvec, Direction);
	//if (spotfactor < Angle)
		//spotfactor = 0.0;
	//else
		//spotfactor = 1.0;
	float edgediff = spotfactor - Angle;
	float angfalloff = saturate(edgediff / lip);
	//float angfalloff = saturate(spotfactor);

	// NOTE: We're halfing the colour here and compensating by doubling it in the light blit shader. This is to allow "overlighting".
	output.Diffuse = float4(Colour * diffuseterm * 0.5 * falloff * angfalloff * shadowterm, 1.0);
	output.Specular = float4(Colour * specularterm * falloff * angfalloff * shadowterm, 1.0);

	return output;
}