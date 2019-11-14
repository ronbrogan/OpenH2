#version 450

layout(location = 0) in vec3 position;

layout(std140, binding = 0) uniform GlobalUniform
{
	mat4 ViewMatrix;
	mat4 ProjectionMatrix;
	vec3 ViewPosition;
} Globals;


out vec3 worldPosition;

void main(){
	worldPosition = vec3(mat4(1) * vec4(position, 1));
	gl_Position = Globals.ProjectionMatrix * Globals.ViewMatrix * vec4(worldPosition, 1);
}