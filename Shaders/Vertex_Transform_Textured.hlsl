#include "Common.hlsli"

CBUFFER_CAMERA_TRANSFORM(b0);
CBUFFER_OBJECT_TRANSFORM(b1);

#include "CommonTransform.hlsli"

TexturedOutputVertex main(TexturedInputVertex vertex)
{
	TexturedOutputVertex output = (TexturedOutputVertex)0;

	float4 worldpos = mul(float4(vertex.Position, 1.0), ModelMatrix);

	output.Position = PROJECT(worldpos, output.ClipDepth);
	output.TexCoord = vertex.TexCoord;

	return output;
}