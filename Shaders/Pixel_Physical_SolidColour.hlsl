#include "Common.hlsli"

CBUFFER_PHYSICAL_PROPERTIES(b0);

cbuffer SolidColour : register(b1)
{
	float4 Colour;
};

GBufferOutputPixel main(FullOutputVertex vertex) : SV_TARGET
{
	GBufferOutputPixel output = (GBufferOutputPixel)0;

	clip(vertex.ClipDepth);

	output.Colour = Colour;
	output.Position = float4( vertex.WorldPosition, 1.0);
	output.Normal = float4(vertex.WorldNormal, 1.0);
	output.Material = float4(Roughness, Reflectivity, IndexOfRefraction, 1.0);

	return output;
}