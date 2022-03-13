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

layout(std140, binding = 2) uniform WireframeUniform
{
	vec4 DiffuseColor;
    float AlphaAmount;
} Data;

layout(binding = 3) uniform sampler2D Textures[8];

layout(location = 0) in vec3 local_position;
layout(location = 1) in vec2 in_texture;
layout(location = 2) in vec3 local_normal;
layout(location = 3) in vec3 tangent;
layout(location = 4) in vec3 bitangent;

layout(location = 0) out vec2 texcoord;
layout(location = 1) out vec3 vertex_color;
layout(location = 2) out vec3 world_pos;
layout(location = 3) out vec3 world_normal;

void main() {

    mat4 modelView = Globals.ViewMatrix * Transform.ModelMatrix;
	mat3 mat3nm = mat3(Transform.NormalMatrix);

	texcoord = in_texture;
	world_normal = normalize(mat3nm * local_normal);
	world_pos = (modelView * vec4(local_position, 1)).xyz;

	gl_Position = Globals.ProjectionMatrix * modelView * vec4(local_position, 1);

	// Nudge closer to camera as this is typically used to overlay a mesh and the lines z-fight
	gl_Position.z -= 0.0001;
}
