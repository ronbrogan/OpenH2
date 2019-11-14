#version 450

in vec3 worldPosition;

out vec4 color;

void main(){ color.rgb = worldPosition; }