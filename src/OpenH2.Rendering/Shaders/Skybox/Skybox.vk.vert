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

layout(std140, binding = 2) uniform SkyboxUniform
{
	bool UseDiffuseMap;
	float DiffuseAmount;
	ivec2 DiffuseMap;
	vec4 DiffuseColor;
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
    mat3 modelmatrix = mat3(1.0);

    mat4 vm = mat4(mat3(Globals.ViewMatrix));

    vec4 pos = Globals.ProjectionMatrix * vm * Transform.ModelMatrix * vec4(local_position, 1);
    world_pos = local_position;

    world_normal = local_normal;

    vertex_color = vec3(0.5f,1,0.5f);
    texcoord = in_texture;
    gl_Position = pos;
}
