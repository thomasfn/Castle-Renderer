
Texture2D texImage;
sampler smpTexture;

float2 imagesize;

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
	float4 Colour : SV_Target0;
};

VShaderOutput VShader( VShaderInput input )
{
    VShaderOutput output;

    output.Position = float4( input.Position, 1.0 );
	output.TexCoord = input.TexCoord;

    return output;
}

// http://forum.unity3d.com/threads/95723-Fxaa3

inline float Luminance( float3 colour )
{
	return dot( colour, float3( 0.3, 0.59, 0.11 ) );
}

inline float TexLuminance( float2 uv )
{
	float3 pix = texImage.Sample( smpTexture, uv ).xyz;
	return Luminance( pix );
}

float4 CombineColours( float4 col1, float4 col2 )
{
	return lerp( col1, col2, 0.5 );
}

float4 FXAA3( float2 pos, float4 extents, float4 dimensionFrame )
{
	float4 retColour;
	
	float4 dir;
	dir.y = 0.0f;
	
	// North-East
	float lumaNE = TexLuminance( extents.zy );
	lumaNE += 1.0f / 384.0f;
	dir.x = -lumaNE;
	dir.z = -lumaNE;
	
	// South-West
	float lumaSW = TexLuminance( extents.xw );
	dir.x += lumaSW;
	dir.z += lumaSW;
	
	// North-West
	float lumaNW = TexLuminance( extents.xy );
	dir.x -= lumaNW;
	dir.z += lumaNW;

	// South-East
	float lumaSE = TexLuminance( extents.zw );
	dir.x += lumaSE;
	dir.z -= lumaSE;			
	
	// early exit
	
	// calc mins and maxes
	float lumaMin = min( min( lumaNW , lumaSW ) , min( lumaNE , lumaSE ) );
	float lumaMax = max( max( lumaNW , lumaSW ) , max( lumaNE , lumaSE ) );
	
	retColour = texImage.Sample( smpTexture, pos );
	float centrePixelLuminance = Luminance( retColour );
	float lumaMinM = min( lumaMin , centrePixelLuminance );
	float lumaMaxM = max( lumaMin , centrePixelLuminance );
	
	// and see if they're within the threshold
	
	if( ( lumaMaxM - lumaMinM ) < max( 0.05f , lumaMax * 0.125f ) )
	{
		return retColour;
	}
	
	// (my comment breaks do not align with those of the original shader)
	float4 dir1pos;
	dir1pos.xy = normalize( dir.xyz ).xz;
	float dirAbsMinTimesC = min( abs( dir1pos.x ) , abs( dir1pos.y ) ) * 4.0f;
	//
	float4 dir2pos;
	dir2pos.xy = clamp( dir1pos.xy / dirAbsMinTimesC , -2.0f , 2.0f );
	dir1pos.zw = pos.xy;
	dir2pos.zw = pos.xy;
	//
	float2 texCoords;
	texCoords = dir1pos.zw - dir1pos.xy * dimensionFrame.zw;
	float4 temp1N = texImage.Sample( smpTexture, texCoords );
	//
	texCoords = dir1pos.zw + dir1pos.xy * dimensionFrame.zw;
	float4 col1 = texImage.Sample( smpTexture, texCoords );
	col1 = CombineColours( temp1N , col1 );
	//
	texCoords = dir2pos.zw - dir2pos.xy * dimensionFrame.xy;
	float4 temp2N = texImage.Sample( smpTexture, texCoords );
	//
	texCoords = dir2pos.zw + dir2pos.xy * dimensionFrame.xy;
	float4 col2 = texImage.Sample( smpTexture, texCoords );
	col2 = CombineColours( temp2N , col2 );
	
	// combine the colours
	col2 = CombineColours( col1 , col2 );
	float col2luminance = Luminance( col2.rgb );
	
	// decide which to return
	if( col2luminance < lumaMin || col2luminance > lumaMax )
	{
		retColour = col1;
	}else{
		retColour = col2;
	}
	
	return retColour;
	
}

PShaderOutput PShader( VShaderOutput input )
{
	PShaderOutput output;
	
	//float3 originalcolour = texImage.Sample( smpTexture, input.TexCoord ).xyz;
	
	// calculate extents
	float4 extents;
	float2 offset = float2( 0.5, 0.5 ) / imagesize.xy;
	extents.xy = input.TexCoord - offset;
	extents.zw = input.TexCoord + offset;

	// and the dimension frame
	float4 dimensionFrame;
	dimensionFrame.xy = offset * 4.0;
	dimensionFrame.zw = offset;

	output.Colour = FXAA3( input.TexCoord, extents, dimensionFrame );
	
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