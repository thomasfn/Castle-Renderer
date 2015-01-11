#include "Common.hlsli"

CBUFFER_CAMERA_TRANSFORM(b0);
CBUFFER_OBJECT_TRANSFORM(b1);

FullOutputVertex main(FullInputVertex vertex)
{
	FullOutputVertex output = (FullOutputVertex)0;

	float4x4 projview = mul(ViewMatrix, ProjectionMatrix);

	float4 worldpos = mul(float4(vertex.Position, 1.0), ModelMatrix);
	float3x3 modelmatrixrot = (float3x3)ModelMatrix;

	output.WorldPosition = worldpos.xyz;
	output.WorldNormal = normalize(mul(float4(vertex.Normal, 1.0), modelmatrixrot).xyz);
	output.WorldTangent = normalize(mul(float4(vertex.Tangent, 1.0), modelmatrixrot).xyz);
	output.WorldBinormal = normalize(mul(float4(vertex.Binormal, 1.0), modelmatrixrot).xyz);

	output.Position = mul(worldpos, projview);
	output.TexCoord = vertex.TexCoord;

	return output;
}