#include "Common.hlsli"

TexturedOutputVertex main(TexturedInputVertex vertex)
{
	FSQuadOutputVertex output = (FSQuadOutputVertex)0;

	output.Position = vertex.Position;
	output.TexCoord = vertex.TexCoord;

	return output;
}