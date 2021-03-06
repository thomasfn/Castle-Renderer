#include "Common.hlsli"

CBUFFER_PHYSICAL_PROPERTIES(b0);

Texture2D DiffuseTexture : register(t0);

SamplerState TextureSampler : register(s0);

GBufferOutputPixel main(FullOutputVertex vertex) : SV_TARGET
{
	GBufferOutputPixel output = (GBufferOutputPixel)0;

	clip(vertex.ClipDepth);

	output.Colour = DiffuseTexture.Sample(TextureSampler, vertex.TexCoord);
	//output.Colour = float4(vertex.WorldNormal, 1.0);
	output.Position = float4(vertex.WorldPosition, vertex.ClipDepth);
	output.Normal = float4(vertex.WorldNormal, 1.0);
	output.Material = float4(Roughness, Reflectivity, IndexOfRefraction, 1.0);

	return output;
}