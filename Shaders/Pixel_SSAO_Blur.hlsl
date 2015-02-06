
#include "Common.hlsli"

CBUFFER_CAMERA(b0);
CBUFFER_CAMERA_TRANSFORM(b1);

#include "CommonTransform.hlsli"

Texture2D PositionTexture : register(t0);
Texture2D AOTexture : register(t1);

SamplerState GBufferSampler : register(s0);

cbuffer SSAO : register(b2)
{
	float2 KernelSize;
	float2 NoiseScale;
	float3 Scale;
};

float4 main(TexturedOutputVertex vertex) : SV_TARGET
{
	float4 position4 = PositionTexture.Sample(GBufferSampler, vertex.TexCoord);
	clip(position4.w - 0.5);

	int blursize = (int)KernelSize.y;

	float2 texelsize = 1.0 / (KernelSize.y * NoiseScale);
	float result = 0.0;
	float fblursize = float(-blursize) * 0.5 + 0.5;
	float2 hlim = float2(fblursize, fblursize);
	for (int i = 0; i < blursize; ++i)
	{
		for (int j = 0; j < blursize; ++j)
		{
			float2 offset = (hlim + float2(float(i), float(j))) * texelsize;
			result += AOTexture.Sample(GBufferSampler, vertex.TexCoord + offset).r;
		}
	}

	float fResult = result / (KernelSize.y * KernelSize.y);
	
	return float4(fResult, 0.0, 0.0, 1.0);
}