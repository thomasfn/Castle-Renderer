
float4x4 transform;
float4x4 view;
float4x4 projection;

Texture2D diffuse;
sampler texturesampler;

float2 textureoffset, texturescale;

struct VShaderInput
{
    float3 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 MiscData : TEXCOORD0;
};

struct VShaderOutput
{
    float4 Position : SV_POSITION;
	float3 Colour : COLOR0;
	float2 MiscData : TEXCOORD0;
};

struct GShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Colour : COLOR0;
	float2 MiscData : TEXCOORD0;
	float2 TexCoord : TEXCOORD1;
};

struct PShaderOutput
{
	float4 Colour : SV_Target0;
};

VShaderOutput VShader( VShaderInput input )
{
    VShaderOutput output;

    output.Position = mul( float4( input.Position, 1.0 ), view );
	output.Colour = input.Normal;
	output.MiscData = input.MiscData;

    return output;
}

const float2 halfvec2 = float2( 0.5, 0.5 );
float2 RotateUV(float2 uv, float2x2 rotmtx)
{
	/*float2 a = uv - half2;
	a = mul( a, rotmtx );
	a = a + half2;
	return a;*/
	
	return halfvec2 + mul( uv - halfvec2, rotmtx );
}

[maxvertexcount(6)]
void GShader( point VShaderOutput input[1], inout TriangleStream<GShaderOutput> OutputStream )
{
	GShaderOutput output;
	output.Colour = input[0].Colour;
	output.MiscData = input[0].MiscData;
	
	float4 halfoffsetX = float4( output.MiscData.x, 0.0, 0.0, 0.0 );
	float4 halfoffsetY = float4( 0.0, output.MiscData.x, 0.0, 0.0 );
	
	float c = cos( output.MiscData.y );
	float s = sin( output.MiscData.y );
	float2x2 rotmtx = { c, -s, s, c };
	
	// TRIANGLE 1
	
	output.Position = mul( input[0].Position - halfoffsetX - halfoffsetY, projection );
	output.TexCoord = RotateUV( float2( 0.0, 0.0 ), rotmtx );
	OutputStream.Append( output );
	
	output.Position = mul( input[0].Position - halfoffsetX + halfoffsetY, projection );
	output.TexCoord = RotateUV( float2( 0.0, 1.0 ), rotmtx );
	OutputStream.Append( output );
	
	output.Position = mul( input[0].Position + halfoffsetX + halfoffsetY, projection );
	output.TexCoord = RotateUV( float2( 1.0, 1.0 ), rotmtx );
	OutputStream.Append( output );
	
	// TRIANGLE 2
	
	// Reuse previous structure data for this one
	//output.Position = mul( input[0].Position + halfoffsetX + halfoffsetY, projection );
	//output.TexCoord = RotateUV( float2( 1.0, 1.0 ), rotmtx );
	OutputStream.Append( output );
	
	output.Position = mul( input[0].Position + halfoffsetX - halfoffsetY, projection );
	output.TexCoord = RotateUV( float2( 1.0, 0.0 ), rotmtx );
	OutputStream.Append( output );
	
	output.Position = mul( input[0].Position - halfoffsetX - halfoffsetY, projection );
	output.TexCoord = RotateUV( float2( 0.0, 0.0 ), rotmtx );
	OutputStream.Append( output );
	
	OutputStream.RestartStrip();
}

PShaderOutput PShader( GShaderOutput input )
{
	PShaderOutput output;
	
	float4 colour = diffuse.Sample( texturesampler, input.TexCoord );
	
	output.Colour = float4( input.Colour * colour.xyz, colour.w );
	
	return output;
}

technique10 Render
{
    pass P0
    {
		SetVertexShader( CompileShader( vs_4_0, VShader() ) );
		SetGeometryShader( CompileShader( gs_4_0, GShader() ) );
		SetPixelShader( CompileShader( ps_4_0, PShader() ) );
    }
}