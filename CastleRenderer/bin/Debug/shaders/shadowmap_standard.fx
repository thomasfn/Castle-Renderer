
float3 light_position;

float4x4 transform;
float4x4 projectionview;

struct VShaderInput
{
    float3 Position : POSITION0;
};

struct VShaderOutput
{
    float4 Position : SV_POSITION;
    float4 WorldPosition : POSITION0;
};

struct PShaderOutput
{
	float4 DistanceData : SV_Target0;
};

VShaderOutput VShader( VShaderInput input )
{
    VShaderOutput output;

    output.WorldPosition = mul( float4( input.Position, 1.0 ), transform );
    output.Position = mul( output.WorldPosition, projectionview );

    return output;
}

PShaderOutput PShader( VShaderOutput input )
{
	PShaderOutput output;
	
	float3 between = input.WorldPosition.xyz - light_position;
	float distance2 = between.x * between.x + between.y * between.y + between.z * between.z;
	float distance = sqrt( distance2 );
	
	output.DistanceData = float4( distance, distance2, 0.0, 0.0 );
	
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