#version 450

layout (triangles) in;
layout (line_strip, max_vertices=4) out;

layout(std140, binding = 0) uniform GlobalUniform
{
	mat4 ViewMatrix;
	mat4 ProjectionMatrix;
	mat4 SunLightMatrix[4];
    vec4 SunLightDistances;
    vec3 SunLightDirection;
    vec3 ViewPosition;
} Globals;

layout(std140, binding = 1) uniform WireframeUniform
{
	vec4 DiffuseColor;
    float AlphaAmount;
} Data;

layout(std140, binding = 2) uniform TransformUniform
{
	mat4 ModelMatrix;
	mat4 NormalMatrix;
} Transform;

in Vertex
{
    vec2 texcoord;
    vec3 vertex_color;
    vec3 world_pos;
    vec3 world_normal;
} vert_out[];

out Vertex
{
    vec2 texcoord;
    vec3 vertex_color;
    vec3 world_pos;
    vec3 world_normal;
} frag_in;

layout(location = 0) out vec4 out_color;


void main(void)
{
    gl_Position = gl_in[0].gl_Position;
    EmitVertex();

    gl_Position = gl_in[1].gl_Position;
    EmitVertex();

    gl_Position = gl_in[2].gl_Position;
    EmitVertex();

    gl_Position = gl_in[0].gl_Position;
    EmitVertex();

    EndPrimitive();
}