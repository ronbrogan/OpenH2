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

layout(std140, binding = 2) uniform TransformUniform
{
    mat4 ModelMatrix;
    mat4 NormalMatrix;
} Transform;

layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec3 inColor;
layout(location = 2) in vec2 inTex;

layout(location = 0) out vec3 fragColor;
layout(location = 1) out vec2 texPos;


void main() {
    gl_Position = Globals.ProjectionMatrix * Globals.ViewMatrix * Transform.ModelMatrix * vec4(inPosition, 1.0);
    fragColor = inColor;
    texPos = inTex;
}