#include "Common.hlsli"

FSQuadOutputVertex main(FSQuadInputVertex vertex)
{
	FSQuadOutputVertex output = (FSQuadOutputVertex)0;

	output.Position = vertex.Position;
	output.TexCoord = vertex.TexCoord;

	return output;
}