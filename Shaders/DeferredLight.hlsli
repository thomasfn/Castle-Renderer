
#include "Common.hlsli"

// The GBuffer
Texture2D PositionTexture : register(t0);
Texture2D NormalTexture : register(t1);
Texture2D MaterialTexture : register(t2);

SamplerState GBufferSampler : register(s0);

// Output struct
struct DeferredLightOutputPixel
{
	float4 Diffuse : SV_TARGET0;
	float4 Specular : SV_TARGET1;
};