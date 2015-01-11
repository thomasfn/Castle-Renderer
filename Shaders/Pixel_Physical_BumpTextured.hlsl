#include "Common.hlsli"

CBUFFER_PHYSICAL_PROPERTIES(b0);

Texture2D DiffuseTexture : register(t0);
Texture2D NormalTexture : register(t1);

SamplerState TextureSampler : register(s0);

cbuffer BumpmapProperties : register(b1)
{
	float BumpHeight;
};

GBufferOutputPixel main(FullOutputVertex vertex) : SV_TARGET
{
	GBufferOutputPixel output = (GBufferOutputPixel)0;

	float3 texturenormal = UnpackNormal(NormalTexture.Sample(TextureSampler, vertex.TexCoord).xyz);
	//float3 texturenormal = float3(0.0, 1.0, 0.0);
		float3x3 normalmatrix = CreateNormalMatrix(vertex.WorldNormal, vertex.WorldTangent, vertex.WorldBinormal);
		float3 normal = normalize(mul(texturenormal * float3(BumpHeight, 1.0, BumpHeight), normalmatrix));

	output.Colour = DiffuseTexture.Sample(TextureSampler, vertex.TexCoord);
	//output.Colour = float4(normal, 1.0);
	output.Position = float4(vertex.WorldPosition, 1.0);
	//output.Normal = float4(mul(texturenormal, normalmatrix), 1.0);
	output.Normal = float4(normal, 1.0);
	output.Material = float4(Roughness, Reflectivity, 0.0, 0.0);

	return output;
}