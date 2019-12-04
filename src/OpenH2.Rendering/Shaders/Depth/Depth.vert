#version 450

layout(std140, binding = 0) uniform GlobalUniform
{
	mat4 ViewMatrix;
	mat4 ProjectionMatrix;
	vec3 ViewPosition;
} Globals;

layout(std140, binding = 1) uniform DepthUniform
{
	mat4 ModelMatrix;
	mat4 NormalMatrix;
} Data;

layout(location = 0) in vec3 local_position;

void main() {
    gl_Position = Globals.ProjectionMatrix * Globals.ViewMatrix * Data.ModelMatrix * vec4(local_position, 1);;
}