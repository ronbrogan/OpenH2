#version 450

#extension GL_ARB_bindless_texture : require

layout(std140, binding = 0) uniform GlobalUniform
{
	mat4 ViewMatrix;
	mat4 ProjectionMatrix;
	vec3 ViewPosition;
} Globals;

layout(std140, binding = 1) uniform GenericUniform
{
	mat4 ModelMatrix;
	mat4 NormalMatrix;
	
	bool UseDiffuseMap;
	float DiffuseAmount;
	sampler2D DiffuseMap;
	vec4 DiffuseColor;
		
	bool UseAlpha;
    float AlphaAmount;
    sampler2D AlphaHandle;

	bool UseSpecularMap;
	float SpecularAmount;
	sampler2D SpecularMap;
	vec4 SpecularColor;
	
	bool UseNormalMap;
	float NormalMapAmount;
	sampler2D NormalMap;
	
	bool UseEmissiveMap;
	float EmissiveMapAmount;
	sampler2D EmissiveMap;
	
	bool UseDetailMap1;
	float DetailMap1Scale;
	sampler2D DetailMap1;

	bool UseDetailMap2;
	float DetailMap2Scale;
	sampler2D DetailMap2;

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

	vec4 diffuse_color = Data.DiffuseColor;

	if(Data.UseDiffuseMap)
	{
		diffuse_color = texture(Data.DiffuseMap, texcoord);
	}

	vec4 detailColor = diffuse_color;

	if(Data.UseDetailMap1 && Data.UseDetailMap2)
	{
		vec4 det1_color = texture(Data.DetailMap1, texcoord * Data.DetailMap1Scale);
		vec4 det2_color = texture(Data.DetailMap2, texcoord * Data.DetailMap2Scale);

		detailColor = mix(det1_color, det2_color, diffuse_color.a);
	}

	diffuse_color = vec4((diffuse_color * detailColor * 2.5).rgb, 1);
	
    float ambientStrength = 0.4;
    vec4 ambient = ambientStrength * diffuse_color;

    vec3 norm = normalize(world_normal);
    vec3 lightDir = normalize(light_pos - world_pos);  

    float diffStrength = max(dot(norm, lightDir), 0.0);
    vec4 diffuse = diffStrength * diffuse_color;

    vec4 result = ambient + diffuse;

	float alpha = 1f;

	if(Data.UseAlpha)
	{
		alpha = texture(Data.AlphaHandle, texcoord).a;
		
		if(alpha < 0.5)
		{
			discard;
		}
	}

    out_color = vec4(result.xyz, alpha);
}