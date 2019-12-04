#version 450

in vec3 world_pos;
in vec3 world_normal;
in vec2 texcoord;
out vec4 out_color;

layout (binding = 0) uniform sampler2D diffuse_map;


void main() {
    vec4 color = texture(diffuse_map, texcoord.xy);

	out_color = color.rgba;
}