
// Requires "CameraTransform" CBuffer

#include "Paraboloid.hlsli"

#define PROJECT(_worldpos_, _clipdepth_) (Paraboloid > 0.5 ? ProjectParaboloid(_worldpos_, _clipdepth_) : ProjectStandard(_worldpos_, _clipdepth_))

float4 ProjectStandard(float4 worldpos, out float clipdepth)
{
	float4 projected = mul(worldpos, mul(ViewMatrix, ProjectionMatrix));
	clipdepth = projected.z;
	return projected;
}

float4 ProjectParaboloid(float4 worldpos, out float clipdepth)
{
	float4 viewpos = mul(worldpos, ViewMatrix);
	return CalculateParaboloid(viewpos, clipdepth);
}