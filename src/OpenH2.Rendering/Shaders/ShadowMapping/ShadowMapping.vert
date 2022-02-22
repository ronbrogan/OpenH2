#version 450

layout(std140, binding = 0) uniform GlobalUniform
{
	mat4 ViewMatrix;
	mat4 ProjectionMatrix;
	mat4 SunLightMatrix;
    vec3 SunLightDirection;
    vec3 ViewPosition;
} Globals;

layout(std140, binding = 2) uniform TransformUniform
{
    mat4 ModelMatrix;
    mat4 NormalMatrix;
} Transform;

layout(location = 0) in vec3 local_position;

void main() {
    gl_Position = Globals.SunLightMatrix * Transform.ModelMatrix * vec4(local_position, 1);
}