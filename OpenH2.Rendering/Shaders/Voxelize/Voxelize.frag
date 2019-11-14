#version 450

#extension GL_ARB_bindless_texture : require
// Lighting settings.
#define POINT_LIGHT_INTENSITY 1
#define MAX_LIGHTS 1

// Lighting attenuation factors.
#define DIST_FACTOR 1.1f /* Distance is multiplied by this when calculating attenuation. */
#define CONSTANT 1
#define LINEAR 0
#define QUADRATIC 1

// Returns an attenuation factor given a distance.
float attenuate(float dist){ dist *= DIST_FACTOR; return 1.0f / (CONSTANT + LINEAR * dist + QUADRATIC * dist * dist); }

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
	float DetailMap1Amount;
	sampler2D DetailMap1;
	vec4 DetailMap1Scale;

	bool UseDetailMap2;
	float DetailMap2Amount;
	sampler2D DetailMap2;
	vec4 DetailMap2Scale;

} Data;

struct PointLight {
	vec4 Position;
    vec4 ColorAndRange;
};

layout(std430, binding = 2) buffer LightingUniform
{
	PointLight[] pointLights;
} Lighting;

layout(binding = 0, RGBA8) uniform image3D texture3D;

in vec2 texcoord;
in vec3 worldPositionFrag;
in vec3 normalFrag;

vec3 calculatePointLight(const PointLight light){
	const vec3 direction = normalize(light.Position.xyz - worldPositionFrag);
	const float distanceToLight = distance(light.Position.xyz, worldPositionFrag);
	const float attenuation = attenuate(distanceToLight);
	const float d = max(dot(normalize(normalFrag), direction), 0.0f);
	return d * POINT_LIGHT_INTENSITY * attenuation * light.ColorAndRange.xyz;
};

vec3 scaleAndBias(vec3 p) { return 0.5f * p + vec3(0.5f); }

bool isInsideCube(const vec3 p, float e) { return abs(p.x) < 1 + e && abs(p.y) < 1 + e && abs(p.z) < 1 + e; }

void main(){
	vec3 color = vec3(0.0f);
	if(!isInsideCube(worldPositionFrag, 0)) return;

	// Calculate diffuse lighting fragment contribution.
	const uint maxLights = min(Lighting.pointLights.length(), 10);
	for(uint i = 0; i < maxLights; ++i) color += calculatePointLight(Lighting.pointLights[i]);
	vec3 spec = Data.SpecularAmount * Data.SpecularColor.rgb;
	vec3 diffuseColor = Data.DiffuseColor.rgb;

	if(Data.UseDiffuseMap)
	{
		diffuseColor = Data.DiffuseAmount * texture(Data.DiffuseMap, texcoord).rgb;
	}

	vec3 diff = Data.DiffuseAmount * diffuseColor;

	color = (diff + spec) * color + clamp(Data.EmissiveMapAmount, 0, 1) * diffuseColor;

	// Output lighting to 3D texture.
	vec3 voxel = scaleAndBias(worldPositionFrag);
	ivec3 dim = imageSize(texture3D);
	float alpha = pow(1 - 0, 4); // For soft shadows to work better with transparent materials., replace 0 with alpha
	alpha = 1;
	vec4 res = alpha * vec4(vec3(1), 1);
    imageStore(texture3D, ivec3(dim * voxel), res);
}