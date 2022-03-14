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

layout(std140, binding = 2) uniform GenericUniform
{
    bool UseDiffuseMap;
    float DiffuseAmount;
    ivec2 DiffuseMap;
    vec4 DiffuseColor;
    
    bool UseAlpha;
    float AlphaAmount;
    ivec2 AlphaHandle;
    vec4 AlphaChannel;

    bool UseSpecularMap;
    float SpecularAmount;
    ivec2 SpecularMap;
    vec4 SpecularColor;
    
    bool UseNormalMap;
    float NormalMapAmount;
    ivec2 NormalMap;
    vec4 NormalMapScale;
    
    bool UseEmissiveMap;
    int EmissiveType;
    ivec2 EmissiveMap;
    vec4 EmissiveArguments;
    
    bool UseDetailMap1;
    float DetailMap1Amount;
    ivec2 DetailMap1;
    vec4 DetailMap1Scale;

    bool UseDetailMap2;
    float DetailMap2Amount;
    ivec2 DetailMap2;
    vec4 DetailMap2Scale;

    bool ChangeColor;
    float ColorChangeAmount;
    ivec2 ColorChangeMaskMap;
    vec4 ColorChangeColor;
} Data;

layout(binding = 3) uniform sampler2D Textures[8];

layout(binding = 16) uniform sampler2DArray shadowMap;

layout(location = 0) in vec3 local_position;
layout(location = 1) in vec2 in_texture;
layout(location = 2) in vec3 local_normal;
layout(location = 3) in vec3 tangent;
layout(location = 4) in vec3 bitangent;

layout(location = 0) out vec3 frag_pos;
layout(location = 1) out vec2 texcoord;
layout(location = 2) out vec3 color;
layout(location = 3) out vec3 world_pos;
layout(location = 4) out vec3 world_normal;
layout(location = 5) out mat3 TBN;

void main() {

    frag_pos = vec3(Transform.ModelMatrix * vec4(local_position, 1.0));

    mat4 modelView = Globals.ViewMatrix * Transform.ModelMatrix;
    mat3 mat3nm = mat3(Transform.NormalMatrix);

    texcoord = in_texture;
    world_normal = normalize(mat3nm * local_normal);
    world_pos = (modelView * vec4(local_position, 1)).xyz;

    if (Data.UseNormalMap) {
        vec3 world_tangent = normalize(mat3nm * tangent);
        vec3 world_bitangent = normalize(mat3nm * bitangent);

        TBN = transpose(mat3(
            world_tangent,
            world_bitangent,
            world_normal
        ));
    }
    else {
        TBN = mat3(1);
    }

    gl_Position = Globals.ProjectionMatrix * modelView * vec4(local_position, 1);
}
