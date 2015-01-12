#include "Common.hlsli"

CBUFFER_CAMERA_TRANSFORM(b0);

#include "Paraboloid.hlsli"

TexturedNormalOutputVertex main(TexturedNormalInputVertex vertex)
{
	TexturedNormalOutputVertex output = (TexturedNormalOutputVertex)0;

	if (Paraboloid > 0.5)
	{
		output.Position = CalculateParaboloid(mul(float4(vertex.Position, 1.0), ViewMatrixRotOnly), output.ClipDepth);
	}
	else
	{
		float4x4 projview = mul(ViewMatrixRotOnly, ProjectionMatrix);
		output.Position = mul(float4(vertex.Position, 1.0), projview);
		output.ClipDepth = output.Position.z;
	}

	output.WorldPosition = vertex.Position;
	output.WorldNormal = vertex.Normal;
	output.TexCoord = vertex.TexCoord;

	return output;
}