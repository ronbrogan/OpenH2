layout(std140, binding = 1) uniform GenericUniform
{
	mat4 ModelMatrix;
	mat4 NormalMatrix;
	
	bool UseDiffuseMap;
	float DiffuseAmount;
	sampler2D DiffuseMap;
	vec4 DiffuseColor;
		
	bool UseAlpha;
    float AlphaAmount;
    sampler2D AlphaHandle;

	bool UseSpecularMap;
	float SpecularAmount;
	sampler2D SpecularMap;
	vec4 SpecularColor;
	
	bool UseNormalMap;
	float NormalMapAmount;
	sampler2D NormalMap;
	
	bool UseEmissiveMap;
	float EmissiveMapAmount;
	sampler2D EmissiveMap;
	
	bool UseDetailMap1;
	float DetailMap1Amount;
	sampler2D DetailMap1;
	vec4 DetailMap1Scale;

	bool UseDetailMap2;
	float DetailMap2Amount;
	sampler2D DetailMap2;
	vec4 DetailMap2Scale;

} Data;