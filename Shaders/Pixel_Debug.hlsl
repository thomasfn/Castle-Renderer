#include "Common.hlsli"

BasicOutputPixel main(BasicOutputVertex vertex) : SV_TARGET
{
	BasicOutputPixel output = (BasicOutputPixel)0;

	output.Colour = float4(1.0, 1.0, 1.0, 1.0);

	return output;
}