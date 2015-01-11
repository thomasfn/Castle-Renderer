
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

// GBufferPixel

struct GBufferOutputPixel
{
	float4 Colour : SV_TARGET0;
	float4 Position : SV_TARGET1;
	float4 Normal : SV_TARGET2;
	float4 Material : SV_TARGET3;
};

// Helper methods

float3 UnpackNormal(in float3 norm)
{
	return normalize( (norm.xzy * 2.0) - 1.0 );
}

float3x3 CreateNormalMatrix(in float3 normal, in float3 tangent, in float3 binormal)
{
	return float3x3(binormal, normal, tangent * -1.0);
}