#version 450

in vec3 world_pos;
in vec2 texcoord;
in vec3 world_normal;
in vec3 vertex_color;
out vec4 out_color;

vec3 light_pos = vec3(50, 50, 500);
vec3 light_color = vec3(1,1,1);

layout (binding = 0) uniform sampler2D diffuse_map;

void main() {
	vec4 ambient_color = texture(diffuse_map, texcoord);
	vec4 diffuse_color = texture(diffuse_map, texcoord);

    float ambientStrength = 0.2;
    vec4 ambient = ambientStrength * ambient_color;

    vec3 norm = normalize(world_normal);
    vec3 lightDir = normalize(light_pos - world_pos);  

    float diffStrength = max(dot(norm, lightDir), 0.0);
    vec4 diffuse = diffStrength * diffuse_color;

    vec4 result = ambient + diffuse;
    out_color = result;
}