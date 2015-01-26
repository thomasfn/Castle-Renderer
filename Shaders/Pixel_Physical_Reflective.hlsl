#include "Common.hlsli"

CBUFFER_PHYSICAL_PROPERTIES(b0);
CBUFFER_CAMERA(b1);
CBUFFER_REFLECTION_INFO(b2);

cbuffer SolidColour : register(b3)
{
	float4 Colour;
};

Texture2D FrontReflectionTexture0 : register(t0);
Texture2D BackReflectionTexture0 : register(t1);

SamplerState ReflectionSampler : register(s0);

// http://members.gamedev.net/JasonZ/Paraboloid/DualParaboloidMappingInTheVertexShader.pdf
// http://gamedevelop.eu/en/tutorials/dual-paraboloid-shadow-mapping.htm

float4 SampleReflection(float3 position, float3 normal)
{
	float3 eye = normalize(position - CameraPosition);
	float3 refl = reflect(eye, normal);
	refl = mul(refl, (float3x3)ReflViewMatrix);

	float2 front;
	front.x = (refl.x / (4 * (1 + refl.z))) + 0.5;
	front.y = 1.0 - ((refl.y / (4 * (1 + refl.z))) + 0.5);

	float2 back;
	back.x = 1.0 - ((refl.x / (4 * (1 - refl.z))) + 0.5);
	back.y = 1.0 - ((refl.y / (4 * (1 - refl.z))) + 0.5);

	float4 forward = FrontReflectionTexture0.Sample(ReflectionSampler, front);
		float4 backward = BackReflectionTexture0.Sample(ReflectionSampler, back);

		//return max(forward, backward);
		if (refl.z > 0.0)
			return forward;
		else
			//return float4( 1.0, 0.0,0.0, 1.0);
			return backward;
}

GBufferOutputPixel main(FullOutputVertex vertex) : SV_TARGET
{
	GBufferOutputPixel output = (GBufferOutputPixel)0;

	clip(vertex.ClipDepth);

	output.Colour = float4(Colour.xyz, 1.0);
	output.Position = float4(vertex.WorldPosition, 1.0);
	output.Normal = float4(vertex.WorldNormal, 1.0);
	output.Material = float4(Roughness, Reflectivity, IndexOfRefraction, 1.0);
	output.Reflection = float4(SampleReflection(vertex.WorldPosition, vertex.WorldNormal).xyz * Reflectivity, 1.0);

	return output;
}