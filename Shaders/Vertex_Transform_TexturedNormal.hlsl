#include "Common.hlsli"

CBUFFER_CAMERA_TRANSFORM(b0);
CBUFFER_OBJECT_TRANSFORM(b1);

TexturedNormalOutputVertex main(TexturedNormalInputVertex vertex)
{
	TexturedNormalOutputVertex output = (TexturedNormalOutputVertex)0;

	float4x4 projview = mul(ProjectionMatrix, ViewMatrix);

	output.WorldPosition = mul(float4(vertex.Position, 1.0), ModelMatrix);
	output.WorldNormal = vertex.Normal; // TODO: Transform normal

	output.Position = mul(output.WorldPosition, projview);
	output.TexCoord = vertex.TexCoord;

	return output;
}