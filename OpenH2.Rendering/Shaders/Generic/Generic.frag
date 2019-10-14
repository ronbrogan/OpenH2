#version 450

#extension GL_ARB_bindless_texture : require

layout(std140, binding = 0) uniform GlobalUniform
{
	mat4 ViewMatrix;
	mat4 ProjectionMatrix;
	vec3 ViewPosition;
} Globals;

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

in vec3 world_pos;
in vec2 texcoord;
in vec3 world_normal;
in vec3 vertex_color;
out vec4 out_color;

// TODO move lighting to global uniform
vec3 light_pos = vec3(50, 50, 500);
vec3 light_color = vec3(1,1,1);

void main() {

	vec4 detail1Tex = texture(Data.DetailMap1, texcoord * Data.DetailMap1Scale.xy);
	vec4 detail2Tex = texture(Data.DetailMap2, texcoord * Data.DetailMap2Scale.xy);
	vec4 diffuseTex = texture(Data.DiffuseMap, texcoord);

	vec4 diffuseColor = Data.UseDiffuseMap ? diffuseTex : Data.DiffuseColor;
	vec4 detailColor;

	if(Data.UseDetailMap1 && Data.UseDetailMap2)
	{
		detailColor = mix(detail1Tex, detail2Tex, diffuseColor.a);
	}
	else if(!Data.UseDetailMap1 && !Data.UseDetailMap2)
	{
		// Set to nop for later multiply
		detailColor = vec4(0.4);
	}
	else
	{
		// If one is empty (vec4(0)), detailColor will be set to the other
		detailColor = detail1Tex + detail2Tex;
	}

	diffuseColor = vec4((diffuseColor * detailColor * 2.5).rgb, 1);
	
    float ambientStrength = 0.4;
    vec4 ambient = ambientStrength * diffuseColor;

    vec3 norm = normalize(world_normal);
    vec3 lightDir = normalize(light_pos - world_pos);  

    float diffStrength = max(dot(norm, lightDir), 0.0);
    vec4 diffuse = diffStrength * diffuseColor;

    vec4 result = ambient + diffuse;

	float alpha = 1f;

	if(Data.UseAlpha)
	{
		alpha = texture(Data.AlphaHandle, texcoord).a;
		
		if(alpha < 0.5)
		{
			discard;
		}
	}

    out_color = vec4(result.xyz, alpha);
}