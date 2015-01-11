#include "Common.hlsli"

CBUFFER_CAMERA_TRANSFORM(b0);

TexturedNormalOutputVertex main(TexturedNormalInputVertex vertex)
{
	TexturedNormalOutputVertex output = (TexturedNormalOutputVertex)0;

	//float4x4 tmpview = (float4x4)(float3x3)ViewMatrix;
	//tmpview._m03_m13_m23 = float3(0.0, 0.0, 0.0);
	//tmpview._m30_m31_m32 = float3(0.0, 0.0, 0.0);
	//tmpview._m33 = 1.0;
	float4x4 projview = mul(ViewMatrix, ProjectionMatrix);

		output.Position = mul(float4(vertex.Position, 1.0), projview);
	//output.Position = float4(vertex.Position, 1.0);
	output.WorldNormal = mul(float4(vertex.Normal, 1.0), ViewMatrixInvTrans).xyz; // TODO: Transform normal

	output.WorldPosition = output.Position.xyz;
	output.TexCoord = vertex.TexCoord;

	return output;
}