#version 450

layout (points) in;
layout (line_strip, max_vertices=5) out;

layout(std140, binding = 0) uniform GlobalUniform
{
	mat4 ViewMatrix;
	mat4 ProjectionMatrix;
	mat4 SunLightMatrix[4];
    vec4 SunLightDistances;
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
    mat4 modelViewProj = Globals.ProjectionMatrix * Globals.ViewMatrix * Data.ModelMatrix;

    frag_in.world_pos = vert_out[0].world_pos + vec3(0.1, 0, 0);
    gl_Position = modelViewProj * vec4(frag_in.world_pos, 1);
    EmitVertex();

    frag_in.world_pos = vert_out[0].world_pos;
    gl_Position = modelViewProj * vec4(frag_in.world_pos, 1);
    EmitVertex();

    frag_in.world_pos = vert_out[0].world_pos + vec3(0, 0.1, 0);
    gl_Position = modelViewProj * vec4(frag_in.world_pos, 1);
    EmitVertex();

    frag_in.world_pos = vert_out[0].world_pos;
    gl_Position = modelViewProj * vec4(frag_in.world_pos, 1);
    EmitVertex();
    
    frag_in.world_pos = vert_out[0].world_pos + vert_out[0].world_normal * 0.25;
    gl_Position = modelViewProj * vec4(frag_in.world_pos, 1);
    EmitVertex();
    
    EndPrimitive();
}