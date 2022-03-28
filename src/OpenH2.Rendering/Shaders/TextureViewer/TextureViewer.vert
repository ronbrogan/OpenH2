#version 450


layout(std140, binding = 0) uniform MatrixUniform
{
    mat4 ViewMatrix;
    mat4 ProjectionMatrix;
    mat4 SunLightMatrix[4];
    vec4 SunLightDistances;
    vec4 SunLightDirection;
    vec4 ViewPosition;
} Matricies;


layout(location = 0) in vec3 local_position;
layout(location = 1) in vec2 in_texture;
layout(location = 2) in vec3 local_normal;
layout(location = 3) in vec3 tangent;
layout(location = 4) in vec3 bitangent;

out vec2 texcoord;
out vec3 world_pos;
out vec3 world_normal;


void main() {
    mat3 modelmatrix = mat3(1.0);

    vec4 pos = Matricies.ProjectionMatrix * Matricies.ViewMatrix * mat4(modelmatrix) * vec4(local_position, 1);
    world_pos = local_position;

    world_normal = local_normal;

    texcoord = in_texture;
    gl_Position = pos;
}
