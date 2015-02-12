// Shadowmap sampling
static const float g_MinVariance = 0.01;

float ReduceLightBleeding(float p_max, float Amount)
{
	// Remove the [0, Amount] tail and linearly rescale (Amount, 1].
	return smoothstep(Amount, 1, p_max);
}
float ChebyshevUpperBound(float2 Moments, float t)
{
	// One-tailed inequality valid if t > Moments.x  
	float p = (t <= Moments.x) ? 1.0f : 0.0f;
	// Compute variance.  
	float Variance = Moments.y - (Moments.x * Moments.x);
	Variance = max(Variance, g_MinVariance);
	// Compute probabilistic upper bound.  
	float d = t - Moments.x;
	float p_max = ReduceLightBleeding(Variance / (Variance + d*d), 0.1);
	return max(p, p_max);
}

float SampleShadowMap(float3 position)
{
	// Calculate device coords
	float4 shadowdevice = mul(float4(position, 1.0), ShadowMatrix);
	shadowdevice /= shadowdevice.w;

	// Clamp to bounds
	float shadowterm = 1.0;
	if (shadowdevice.x < -1.0 || shadowdevice.x > 1.0 || shadowdevice.y < -1.0 || shadowdevice.y > 1.0 || shadowdevice.z < -1.0 || shadowdevice.z > 1.0)
		shadowterm = 1.0;
	else
	{
		float2 uv = shadowdevice.xy * 0.5 + 0.5;
		uv.y = 1.0 - uv.y;
		float2 smp = ShadowMapTexture.Sample(ShadowMapSampler, uv).xy;
		float dist = length(position - Position);
		shadowterm = ChebyshevUpperBound(smp, dist);
		//float diff = dist - smp.x;
		//shadowterm = diff < 0.01 ? 1.0 : 0.0;
	}

	// Return
	return shadowterm;
}