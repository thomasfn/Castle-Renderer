#include "Common.hlsli"

TexturedOutputVertex main(TexturedInputVertex vertex)
{
	TexturedOutputVertex output = (TexturedOutputVertex)0;

	output.Position = float4(vertex.Position, 1.0);
	output.TexCoord = vertex.TexCoord;

	return output;
}