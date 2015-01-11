void cook_torrance(in float3 normal, in float3 position, in float3 direction, in float roughnessvalue, in float reflectivity, in float indexofrefraction, out float diffuseterm, out float specularterm)
{
	// Calculate reflection and half vectors
	float3 eyedir = normalize(CameraPosition - position);
	float3 halfvec = normalize(direction + eyedir);


	float NdotL = saturate(dot(normal, direction));
	float NdotH = saturate(dot(normal, halfvec));
	float NdotV = saturate(dot(normal, eyedir));
	float VdotH = saturate(dot(eyedir, halfvec));
	float r_sq = roughnessvalue * roughnessvalue;

	float geo_num = 2.0 * NdotH;
	float geo_b = (geo_num * NdotV) / VdotH;
	float geo_c = (geo_num * NdotL) / VdotH;
	float geo = min(1.0, min(geo_b, geo_c));

	float roughness_a = 1.0 / (4.0 * r_sq * pow(NdotH, 4.0));
	float roughness_b = (NdotH * NdotH - 1.0) / (r_sq * NdotH * NdotH);
	float roughness = roughness_a * exp(roughness_b);

	float fresnel = pow(1.0 - VdotH, 5.0);
	fresnel *= (1.0 - indexofrefraction);
	fresnel += indexofrefraction;

	float rs_num = geo * roughness * fresnel;
	float rs_denom = NdotV * NdotL * 3.14;
	float rs = rs_num / rs_denom;

	float factor = saturate(NdotL);
	diffuseterm = factor * (1.0 - reflectivity);
	specularterm = saturate(factor * rs * reflectivity);
}

void blinn_phong(in float3 normal, in float3 position, in float3 direction, in float roughnessvalue, in float reflectivity, in float indexofrefraction, out float diffuseterm, out float specularterm)
{
	// Calculate reflection and half vectors
	float3 reflection = reflect(direction, normal);
		float3 halfvec = normalize(CameraPosition - position);

	// Calculate reflect and diffuse terms
	float NdotL = saturate(dot(normal, direction));
	float RdotV = saturate(-dot(reflection, halfvec));

	diffuseterm = NdotL;
	specularterm = reflectivity * pow(RdotV, roughnessvalue * 100.0);
}