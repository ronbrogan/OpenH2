#version 450

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

layout(binding = 2) uniform sampler2D Textures[1];

layout(location = 0) in vec3 local_position;
layout(location = 1) in vec2 in_texture;
layout(location = 2) in vec3 local_normal;
layout(location = 3) in vec3 tangent;
layout(location = 4) in vec3 bitangent;

layout(location = 0) out vec2 texPos;


void main() {
    mat4 modelView = Globals.ViewMatrix * Transform.ModelMatrix;
    gl_Position = Globals.ProjectionMatrix * modelView * vec4(local_position, 1.0);
    texPos = in_texture;
}