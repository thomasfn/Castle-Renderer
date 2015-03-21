#include "Common.hlsli"

CBUFFER_CAMERA_TRANSFORM(b0);
//CBUFFER_OBJECT_TRANSFORM(b1);
CBUFFER_TEXTURE_TRANSFORM(b1);

cbuffer RopeInfo : register(b2)
{
	float4 StartPos;
	float4 EndPos;
};

#include "CommonTransform.hlsli"

FullOutputVertex main(FullInputVertex vertex)
{
	FullOutputVertex output = (FullOutputVertex)0;

	float dy = vertex.Position.y;

	float quadratic = -4.0 * (dy * dy - dy);


	//float4 worldpos = mul(float4(vertex.Position, 1.0), ModelMatrix);
	float4 worldpos = float4(lerp(StartPos.xyz, EndPos.xyz, dy) + float3(vertex.Position.x, quadratic * -1.0f * StartPos.w, 0.0), 1.0);
		//float4 worldpos = float4(vertex.Position + lerp(StartPos, EndPos, 0.5) + float3(0.0, 0.0, 5.0), 1.0);
		//float3x3 modelmatrixrot = (float3x3)ModelMatrix;

		// (x-0)(x-1)
		// -4(x^2 - x)

	

	output.WorldPosition = worldpos.xyz;
	output.WorldNormal = float4(vertex.Normal, 1.0);
	output.WorldTangent = float4(vertex.Tangent, 1.0);
	output.WorldBinormal = float4(vertex.Binormal, 1.0);

	output.Position = PROJECT(worldpos, output.ClipDepth);
	output.TexCoord = TextureOffset + TextureScale * vertex.TexCoord;

	return output;
}