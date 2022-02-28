#version 450

layout(location = 0) in vec2 texPos;

layout(location = 0) out vec4 outColor;

layout(binding = 2) uniform sampler2D Textures[1];

void main() {
    outColor = texture(Textures[0], texPos);
}