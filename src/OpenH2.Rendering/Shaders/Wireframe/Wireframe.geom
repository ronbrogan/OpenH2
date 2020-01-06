#version 450

layout (triangles) in;
layout (line_strip, max_vertices=3) out;

layout(std140, binding = 0) uniform GlobalUniform
{
	mat4 ViewMatrix;
	mat4 ProjectionMatrix;
	vec4 ViewPosition;
} Globals;

layout(std140, binding = 1) uniform WireframeUniform
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
    int i;
    for (i = 0; i < gl_in.length(); i++)
    {
        frag_in.texcoord = vert_out[i].texcoord;
        frag_in.world_pos = vert_out[i].world_pos;
        frag_in.world_normal = vert_out[i].world_normal;
        gl_Position = gl_in[i].gl_Position;
        EmitVertex();
    }
    EndPrimitive();
}