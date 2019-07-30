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
	
	bool UseDetailMap0;
	float DetailMapAmount0;
	sampler2D DetailMap0;

	bool UseDetailMap1;
	float DetailMapAmount1;
	sampler2D DetailMap1;

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

	vec4 ambient_color = Data.DiffuseColor;
	vec4 diffuse_color = Data.DiffuseColor;

	if(Data.UseDiffuseMap)
	{
		ambient_color = texture(Data.DiffuseMap, texcoord);
		diffuse_color = texture(Data.DiffuseMap, texcoord);
	}

    float ambientStrength = 0.4;
    vec4 ambient = ambientStrength * ambient_color;

    vec3 norm = normalize(world_normal);
    vec3 lightDir = normalize(light_pos - world_pos);  

    float diffStrength = max(dot(norm, lightDir), 0.0);
    vec4 diffuse = diffStrength * diffuse_color;

    vec4 result = ambient + diffuse;
    out_color = result;
}