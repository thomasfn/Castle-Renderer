#include "Common.hlsli"

CBUFFER_CAMERA_TRANSFORM(b0);
CBUFFER_OBJECT_TRANSFORM(b1);

TexturedNormalOutputVertex main(TexturedNormalInputVertex vertex)
{
	TexturedNormalOutputVertex output = (TexturedNormalOutputVertex)0;

	float4x4 projview = mul(ViewMatrix, ProjectionMatrix);

	float4 worldpos = mul(float4(vertex.Position, 1.0), ModelMatrix);

	output.WorldPosition = worldpos.xyz;
	output.WorldNormal = vertex.Normal; // TODO: Transform normal

	output.Position = mul(worldpos, projview);
	output.TexCoord = vertex.TexCoord;

	return output;
}