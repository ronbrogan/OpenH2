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

struct PointLight {
	vec4 Position;
    vec4 ColorAndRange;
};

layout(std140, binding = 2) uniform LightingUniform
{
	PointLight[10] pointLights;
} Lighting;

layout(location = 0) in vec3 local_position;
layout(location = 1) in vec2 in_texture;
layout(location = 2) in vec3 local_normal;
layout(location = 3) in vec3 tangent;
layout(location = 4) in vec3 bitangent;

out vec2 texcoord;
out vec3 vertex_color;
out vec3 world_pos;
out vec3 world_normal;
out mat3 TBN;

void main() {

    mat4 modelView = Globals.ViewMatrix * Data.ModelMatrix;
	mat3 mat3nm = mat3(Data.NormalMatrix);

	texcoord = in_texture;
	world_normal = normalize(mat3nm * local_normal);
	world_pos = (Data.ModelMatrix * vec4(local_position, 1)).xyz;

	if (Data.UseNormalMap) {
		vec3 world_tangent = normalize(mat3nm * tangent);
		vec3 world_bitangent = normalize(mat3nm * bitangent);

		TBN = transpose(mat3(
			world_tangent,
			world_bitangent,
			world_normal
		));
	}
	else {
		TBN = mat3(1);
	}

	gl_Position = Globals.ProjectionMatrix * modelView * vec4(local_position, 1);
}