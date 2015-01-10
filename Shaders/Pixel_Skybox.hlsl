#include "Common.hlsli"

BasicOutputPixel main(FSQuadOutputVertex vertex) : SV_TARGET
{
	BasicOutputPixel output = (BasicOutputPixel)0;

	// TODO: Output texture somehow
	output.Colour = float4(1.0, 0.0, 1.0, 1.0);

	return output;
}