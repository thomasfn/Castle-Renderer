#include "Common.hlsli"

Texture2D SourceTexture : register(t0);
SamplerState BlitSampler : register(s0);

BasicOutputPixel main(TexturedOutputVertex vertex) : SV_TARGET
{
	BasicOutputPixel output = (BasicOutputPixel)0;

	output.Colour = SourceTexture.Sample(BlitSampler, vertex.TexCoord);

	return output;
}