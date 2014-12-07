
Texture2D texNormal;
Texture2D texPosition;
Texture2D texMaterial;
sampler smpTexture;

Texture2D shadowmap;
float4x4 shadowmatrix;
float useshadows;

float3 colour;
float3 position;
float3 direction;

float3 camera_position;
float3 camera_forward;

struct VShaderInput
{
    float3 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VShaderOutput
{
    float4 Position : SV_POSITION;
	float2 TexCoord : TEXCOORD0;
};

struct PShaderOutput
{
	float4 Diffuse : SV_Target0;
	float4 Specular : SV_Target1;
};

VShaderOutput VShader( VShaderInput input )
{
    VShaderOutput output;

    output.Position = float4( input.Position, 1.0 );
	output.TexCoord = input.TexCoord;

    return output;
}

const float g_MinVariance = 0.01;

float ReduceLightBleeding(float p_max, float Amount)
{
	// Remove the [0, Amount] tail and linearly rescale (Amount, 1].
	return smoothstep( Amount, 1, p_max );
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
	float p_max = ReduceLightBleeding( Variance / (Variance + d*d), 0.1 );
	return max(p, p_max);  
}

PShaderOutput PShader( VShaderOutput input )
{
	// Sample GBuffer
	float4 normal = texNormal.Sample( smpTexture, input.TexCoord );
	float4 pixelpos = texPosition.Sample( smpTexture, input.TexCoord );
	float4 material = texMaterial.Sample( smpTexture, input.TexCoord );
	
	// Transform position to shadowmap coords
	float4 shadowdevice = mul( float4( pixelpos.xyz, 1.0 ), shadowmatrix );
	
	// Clamp to bounds
	float shadowterm = 1.0;
	if (shadowdevice.x < -1.0 || shadowdevice.x > 1.0 || shadowdevice.y < -1.0 || shadowdevice.y > 1.0 || shadowdevice.z < -1.0 || shadowdevice.z > 1.0)
		shadowterm = 1.0;
	else
	{
		float2 uv = (shadowdevice.xy / shadowdevice.w) * 0.5 + 0.5;
		uv.y = 1.0 - uv.y;
		float2 sample = shadowmap.Sample( smpTexture, uv ).xy;
		float dist = length( pixelpos.xyz - position );
		shadowterm = ChebyshevUpperBound( sample, dist );
		
	}

	PShaderOutput output;
	
	// Calculate diffuse term
	float NdotL = dot( normal.xyz, direction );
	float intensity = saturate( -NdotL ) * shadowterm;
	// NOTE: We're halfing the colour here and compensating by doubling it in the light blit shader. This is to allow "overlighting".
	output.Diffuse = float4( colour * intensity * 0.5, 1.0 );
	
	// From OpenGL shader
	/*vec3 eye = normalize( camerapos - pos.xyz );
	vec3 r = reflect( -direction, normal );
	float specularcomponent = 0.3 * pow( clamp( dot( r, eye ), 0.0, 1.0 ), 5.0 );*/
	
	// Calculate specular term
	float3 eye = normalize( camera_position - pixelpos.xyz );
	float3 r = reflect( direction, normal.xyz );
	
	//float3 H = normalize( direction + (pixelpos.xyz - camera_position));
	//float NdotH = dot( normal.xyz, H );
	intensity *= pow( saturate( dot( r, eye ) ), material.x );
	output.Specular = float4( colour * intensity * material.y, 1.0 );
	
	return output;
}

technique10 Render
{
    pass P0
    {
		SetVertexShader( CompileShader( vs_4_0, VShader() ) );
		SetPixelShader( CompileShader( ps_4_0, PShader() ) );
    }
}