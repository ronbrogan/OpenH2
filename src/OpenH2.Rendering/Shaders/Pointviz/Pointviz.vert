#version 450

layout(std140, binding = 0) uniform GlobalUniform
{
	mat4 ViewMatrix;
	mat4 ProjectionMatrix;
	mat4 SunLightMatrix;
	vec4 ViewPosition;
} Globals;

layout(std140, binding = 1) uniform PointvizUniform
{
	mat4 ModelMatrix;
	mat4 NormalMatrix;
	vec4 DiffuseColor;
    float AlphaAmount;
} Data;

layout(location = 0) in vec3 local_position;
layout(location = 1) in vec2 in_texture;
layout(location = 2) in vec3 local_normal;
layout(location = 3) in vec3 tangent;
layout(location = 4) in vec3 bitangent;

out Vertex
{
	vec2 texcoord;
	vec3 vertex_color;
	vec3 world_pos;
	vec3 world_normal;
} vert_out;

void main() {

    mat4 modelView = Globals.ViewMatrix * Data.ModelMatrix;
	mat3 mat3nm = mat3(Data.NormalMatrix);

	vert_out.texcoord = in_texture;
	vert_out.world_normal = local_normal;
	vert_out.world_pos = local_position;

	gl_Position =  vec4(local_position, 1);
}