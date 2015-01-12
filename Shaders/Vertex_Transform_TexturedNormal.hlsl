#include "Common.hlsli"

CBUFFER_CAMERA_TRANSFORM(b0);
CBUFFER_OBJECT_TRANSFORM(b1);

#include "CommonTransform.hlsli"

TexturedNormalOutputVertex main(TexturedNormalInputVertex vertex)
{
	TexturedNormalOutputVertex output = (TexturedNormalOutputVertex)0;

	float4 worldpos = mul(float4(vertex.Position, 1.0), ModelMatrix);
	float3x3 modelmatrixrot = (float3x3)ModelMatrix;

	output.WorldPosition = worldpos.xyz;
	output.WorldNormal = normalize(mul(float4(vertex.Normal, 1.0), modelmatrixrot).xyz);

	output.Position = PROJECT(worldpos, output.ClipDepth);
	output.TexCoord = vertex.TexCoord;

	return output;
}