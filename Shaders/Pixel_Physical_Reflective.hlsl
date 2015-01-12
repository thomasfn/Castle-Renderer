#include "Common.hlsli"

CBUFFER_PHYSICAL_PROPERTIES(b0);
CBUFFER_CAMERA(b1);

cbuffer SolidColour : register(b2)
{
	float4 Colour;
};

Texture2D FrontReflectionTexture0 : register(t0);
Texture2D BackReflectionTexture0 : register(t1);

SamplerState ReflectionSampler : register(s0);

GBufferOutputPixel main(FullOutputVertex vertex) : SV_TARGET
{
	GBufferOutputPixel output = (GBufferOutputPixel)0;

	clip(vertex.ClipDepth);



	output.Colour = float4(FrontReflectionTexture0.Sample(ReflectionSampler, vertex.TexCoord).xyz, 1.0);
	output.Position = float4(vertex.WorldPosition, 1.0);
	output.Normal = float4(vertex.WorldNormal, 1.0);
	output.Material = float4(Roughness, Reflectivity, 0.0, 0.0);

	return output;
}