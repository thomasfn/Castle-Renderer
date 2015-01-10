#include "Common.hlsli"

CBUFFER_CAMERA_TRANSFORM(b0);
CBUFFER_OBJECT_TRANSFORM(b1);

TexturedOutputVertex main(TexturedInputVertex vertex)
{
	TexturedOutputVertex output = (TexturedOutputVertex)0;

	float4x4 projviewmodel = mul(mul(ProjectionMatrix, ViewMatrix), ModelMatrix);

	output.Position = mul(vertex.Position, projviewmodel);
	output.TexCoord = vertex.TexCoord;

	return output;
}