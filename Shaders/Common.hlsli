
#include "CommonCBuffers.hlsli"

// BasicVertex

struct BasicInputVertex
{
	float3 Position : POSITION;
};

struct BasicOutputVertex
{
	float4 Position : SV_POSITION;
};

// TexturedVertex

struct TexturedInputVertex
{
	float3 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
};

struct TexturedOutputVertex
{
	float4 Position : SV_POSITION;
	float2 TexCoord : TEXCOORD0;
};

// TexturedNormalVertex

struct TexturedNormalInputVertex
{
	float3 Position : POSITION;

	float3 Normal : NORMAL;

	float2 TexCoord : TEXCOORD0;
	
};

struct TexturedNormalOutputVertex
{
	float4 Position : SV_POSITION;

	float3 WorldPosition : POSITION;
	float3 WorldNormal : NORMAL;

	float2 TexCoord : TEXCOORD0;
};

// FullVertex

struct FullInputVertex
{
	float3 Position : POSITION;

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

// BasicPixel

struct BasicOutputPixel
{
	float4 Colour : SV_TARGET0;
};