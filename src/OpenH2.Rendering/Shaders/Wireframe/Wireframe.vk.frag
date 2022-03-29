#version 450

#extension GL_EXT_nonuniform_qualifier : enable

layout(std140, binding = 0) uniform GlobalUniform
{
    mat4 ViewMatrix;
    mat4 ProjectionMatrix;
    mat4 SunLightMatrix[4];
    vec4 SunLightDistances;
    vec4 SunLightDirection;
    vec4 ViewPosition;
} Globals;

layout(std140, binding = 1) uniform TransformUniform
{
    mat4 ModelMatrix;
    mat4 NormalMatrix;
} Transform;

layout(std140, binding = 2) uniform WireframeUniform
{
	vec4 DiffuseColor;
    float AlphaAmount;
} Data;

layout(set = 1, binding = 3) uniform sampler2D Textures[];

layout(location = 0) in vec2 texcoord;
layout(location = 1) in vec3 vertex_color;
layout(location = 2) in vec3 world_pos;
layout(location = 3) in vec3 world_normal;

layout(location = 0) out vec4 out_color;

void main() {
    vec4 diffuse = Data.DiffuseColor;

    out_color = vec4(diffuse.x, diffuse.y, diffuse.z, min(diffuse.a, Data.AlphaAmount));
}
