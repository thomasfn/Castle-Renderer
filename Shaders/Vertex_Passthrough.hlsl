#include "Common.hlsli"

BasicOutputVertex main(BasicInputVertex vertex)
{
	BasicOutputVertex output = (BasicOutputVertex)0;

	output.Position = vertex.Position;

	return output;
}