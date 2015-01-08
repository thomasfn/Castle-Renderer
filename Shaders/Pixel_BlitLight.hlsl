#include "Common.hlsli"

BasicOutputPixel main(FSQuadOutputVertex vertex) : SV_TARGET
{
	BasicOutputPixel output = (BasicOutputPixel)0;

	//float4 colour = texColour.Sample(smpTexture, input.TexCoord);
	// NOTE: We're doubling the colour here to compensate for halfing it in the light shaders. This is to allow "overlighting".
	//float4 diffuse = texDiffuseLight.Sample(smpTexture, input.TexCoord) * 2.0;
	//float4 specular = texSpecularLight.Sample(smpTexture, input.TexCoord);

	//float3 add = saturate( specular.xyz );

	//output.Colour = float4(colour.xyz * diffuse.xyz + specular.xyz, colour.w);

	// TODO: Output texture somehow
	output.Colour = float4(1.0, 0.0, 1.0, 1.0);

	return output;
}