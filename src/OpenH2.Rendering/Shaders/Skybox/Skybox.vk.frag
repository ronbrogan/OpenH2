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

layout(std140, binding = 2) uniform SkyboxUniform
{
    bool UseDiffuseMap;
    float DiffuseAmount;
    ivec2 DiffuseMap;
    vec4 DiffuseColor;
} Data;

layout(set = 1, binding = 3) uniform sampler2D Textures[];
layout(binding = 16) uniform sampler2DArray shadowMap;

layout(location = 0) in vec2 texcoord;
layout(location = 1) in vec3 vertex_color;
layout(location = 2) in vec3 world_pos;
layout(location = 3) in vec3 world_normal;

layout(location = 0) out vec4 out_color;

// TODO move lighting to global uniform
vec3 light_pos = vec3(50, 50, 500);
vec3 light_color = vec3(1,1,1);

void main() {

	vec4 diffuseTex = texture(Textures[Data.DiffuseMap.x], texcoord);

	vec4 diffuseColor = Data.UseDiffuseMap ? diffuseTex : Data.DiffuseColor;

	diffuseColor = vec4(diffuseColor.rgb, 1);

    out_color = diffuseColor;
}
