#version 450

#extension GL_ARB_bindless_texture : require

layout(std140, binding = 0) uniform GlobalUniform
{
	mat4 ViewMatrix;
	mat4 ProjectionMatrix;
	vec3 ViewPosition;
} Globals;

layout(std140, binding = 1) uniform SkyboxUniform
{
	mat4 ModelMatrix;
	mat4 NormalMatrix;
	
	bool UseDiffuseMap;
	float DiffuseAmount;
	sampler2D DiffuseMap;
	vec4 DiffuseColor;

} Data;

in vec3 world_pos;
in vec2 texcoord;
in vec3 world_normal;
in vec3 vertex_color;
out vec4 out_color;

// TODO move lighting to global uniform
vec3 light_pos = vec3(50, 50, 500);
vec3 light_color = vec3(1,1,1);

void main() {

	vec4 diffuseTex = texture(Data.DiffuseMap, texcoord);

	vec4 diffuseColor = Data.UseDiffuseMap ? diffuseTex : Data.DiffuseColor;

	diffuseColor = vec4(diffuseColor.rgb, 1);

    out_color = diffuseColor;
}