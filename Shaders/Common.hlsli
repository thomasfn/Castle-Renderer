
struct BasicInputVertex
{
	float4 Position : POSITION;
};

struct BasicOutputVertex
{
	float4 Position : SV_POSITION;
};

struct FullInputVertex
{
	float4 Position : POSITION;

	float3 Normal : NORMAL;
	float3 Tangent : TANGENT;
	float3 Binormal : BINORMAL;

	float2 TexCoord : TEXCOORD0;
};

struct FullOutputVertex
{
	float4 Position : SV_POSITION;

	float3 WorldPosition : POSITION;
	float3 WorldNormal : NORMAL;
	float3 WorldTangent : TANGENT;
	float3 WorldBinormal : BINORMAL;

	float2 TexCoord : TEXCOORD0;
};

struct BasicOutputPixel
{
	float4 Colour : SV_TARGET0;
};