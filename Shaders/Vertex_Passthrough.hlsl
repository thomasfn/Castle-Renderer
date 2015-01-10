#include "Common.hlsli"

BasicOutputVertex main(BasicInputVertex vertex)
{
	BasicOutputVertex output = (BasicOutputVertex)0;

	output.Position = float4( vertex.Position, 1.0 );

	return output;
}