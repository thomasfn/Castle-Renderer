#include "Common.hlsli"

CBUFFER_CAMERA_TRANSFORM(b0);

TexturedNormalOutputVertex main(TexturedNormalInputVertex vertex)
{
	TexturedNormalOutputVertex output = (TexturedNormalOutputVertex)0;
	float4x4 projview = mul(ViewMatrixRotOnly, ProjectionMatrix);

	output.Position = mul(float4(vertex.Position, 1.0), projview);
	output.WorldNormal = vertex.Normal;

	output.WorldPosition = output.Position.xyz;
	output.TexCoord = vertex.TexCoord;

	return output;
}