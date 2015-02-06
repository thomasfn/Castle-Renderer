
#include "Common.hlsli"

CBUFFER_CAMERA(b0);
CBUFFER_CAMERA_TRANSFORM(b1);

#include "CommonTransform.hlsli"

Texture2D PositionTexture : register(t0);
Texture2D NormalTexture : register(t1);
Texture2D KernelTexture : register(t2);
Texture2D NoiseTexture : register(t3);

SamplerState GBufferSampler : register(s0);
SamplerState KernelSampler : register(s1);

cbuffer SSAO : register(b2)
{
	float2 KernelSize;
	float2 NoiseScale;
	float3 Scale;
};

float length2(float3 vec)
{
	return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z;
}

float OcclusionTerm(float testdepth2, float sampledepth2)
{
	const float bias = 0.25 * 0.25;
	float diff = testdepth2 - sampledepth2;
	//return clamp((diff / bias) + 0.5, 0.0, 1.0);
	return diff < 0.0 ? 0.0 : 1.0;
}

float4 main(TexturedOutputVertex vertex) : SV_TARGET
{
	float4 position4 = PositionTexture.Sample(GBufferSampler, vertex.TexCoord);
	//if (position4.w < 0.5) return float4(0.0, 0.0, 0.0, 0.0);
	clip(position4.w-0.0001);
	float3 normal = NormalTexture.Sample(GBufferSampler, vertex.TexCoord).xyz;
	float3 position = position4.xyz;

	//float mydepth2 = position4.w * position4.w;
	float mydepth2 = length2(position - CameraPosition);

	float3 rvec = NoiseTexture.Sample(KernelSampler, vertex.TexCoord * NoiseScale).xyz;
	float3 tangent = normalize(rvec - normal * dot(rvec, normal));
	float3 binormal = cross(normal, tangent);
	float3x3 tbn = float3x3(binormal, normal, tangent);

	float occlusion = 0.0;
	float occperit = 1.0 / KernelSize;

	float4x4 viewproj = mul(ViewMatrix, ProjectionMatrix);

		//float seed = (floor((vertex.TexCoord.x * 57.0 + vertex.TexCoord.y) * KernelSize.y) + 0.5) / KernelSize.y;
		//float seed = NoiseTexture.Sample(KernelSampler, vertex.TexCoord).r;

	float clipdepth;
	//float counter = 0.0;
	int iterations = (int)KernelSize.x;
	for (int i = 0; i < iterations; i++)
	{
		// Get the sample vector
		float3 ssao_vec = mul(KernelTexture.Sample(KernelSampler, (i + 0.5) * occperit, 0.5).xyz, tbn);
		//float3 ssao_vec = KernelTexture.Sample(KernelSampler, float2( (i + 0.5) * occperit, seed ) ).xyz;
			//ssao_vec.y = -ssao_vec.y;

		// Check that it faces the front
		//float term = dot(ssao_vec, normal.xyz);
		//if (term > 0.0)
		{
			// Calculate the point to test
			float3 testpos = position + ssao_vec * Scale.x;
			float testdepth2 = length2(testpos - CameraPosition);

			// Project the test point
			//float4 testdev = ProjectStandard(float4(testpos, 1.0), clipdepth);
			float4 testdev = mul(float4(testpos, 1.0), viewproj);
			testdev /= testdev.w;
			float2 testuv = (testdev * 0.5 + 0.5).xy;
			testuv.y = 1.0 - testuv.y;

			// Sample the test point
			float4 samplepos = PositionTexture.Sample(GBufferSampler, testuv);
			//float sampledepth2 = samplepos.w * samplepos.w;
			float sampledepth2 = length2(samplepos.xyz - CameraPosition);

			// Range check
			float rangecheck = abs(position4.w - samplepos.w) < Scale.y ? 1.0 : 0.0;

			// Accumulate occlusion
			occlusion += OcclusionTerm(testdepth2, sampledepth2) * rangecheck * Scale.z;
			//counter += 1.0f;
		}
	}
	return float4(min(1.0, occlusion / KernelSize.x), 0.0, 0.0, 1.0);
}