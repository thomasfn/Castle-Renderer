#include "Common.hlsli"

CBUFFER_CAMERA(b0);

BasicOutputPixel main(FullOutputVertex vertex) : SV_TARGET
{
	BasicOutputPixel output = (BasicOutputPixel)0;

	clip(vertex.ClipDepth);

	float3 between = vertex.WorldPosition.xyz - CameraPosition;
	float distance2 = between.x * between.x + between.y * between.y + between.z * between.z;
	float distance = sqrt(distance2);

	output.Colour = float4(distance, distance2, 0.0, 0.0);
	//output.Colour = float4(0.0, 0.0, 0.0, 0.0);

	return output;
}