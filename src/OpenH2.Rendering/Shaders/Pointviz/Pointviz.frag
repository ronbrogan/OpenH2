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

in Vertex
{
    vec2 texcoord;
    vec3 vertex_color;
    vec3 world_pos;
    vec3 world_normal;
} frag_in;

layout(location = 0) out vec4 out_color;

void main() {
    vec4 diffuse = Data.DiffuseColor;

	out_color = vec4(diffuse.x, diffuse.y, diffuse.z, min(diffuse.a, Data.AlphaAmount));
}