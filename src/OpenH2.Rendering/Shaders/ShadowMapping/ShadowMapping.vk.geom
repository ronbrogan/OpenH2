#version 450

layout(triangles, invocations = 4) in;
layout(triangle_strip, max_vertices = 3) out;

layout(std140, binding = 0) uniform GlobalUniform
{
	mat4 ViewMatrix;
	mat4 ProjectionMatrix;
	mat4 SunLightMatrix[4];
    vec4 SunLightDistances;
    vec3 SunLightDirection;
    vec3 ViewPosition;
} Globals;

void main() {
    for (int i = 0; i < 3; ++i)
    {
        gl_Position = 
            Globals.SunLightMatrix[gl_InvocationID] * gl_in[i].gl_Position;
        gl_Layer = gl_InvocationID;
        EmitVertex();
    }
    EndPrimitive();
}
