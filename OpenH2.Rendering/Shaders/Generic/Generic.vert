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

layout(location = 0) in vec3 local_position;
layout(location = 1) in vec2 in_texture;
layout(location = 2) in vec3 local_normal;
layout(location = 3) in vec3 tangent;
layout(location = 4) in vec3 bitangent;

out vec2 texcoord;
out vec3 vertex_color;
out vec3 world_pos;
out vec3 world_normal;


void main() {
    mat3 modelmatrix = mat3(1.0);

    vec4 pos = Globals.ProjectionMatrix * Globals.ViewMatrix * Data.ModelMatrix * vec4(local_position, 1);
    world_pos = local_position;

    world_normal = local_normal;

    vertex_color = vec3(0.5f,1,0.5f);
	texcoord = in_texture;
    gl_Position = pos;
}