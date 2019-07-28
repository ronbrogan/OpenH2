using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace OpenH2.Rendering.Shaders
{
    public static class ShaderCompiler
    {
        public static int CreateStandardShader()
        {
            var vertexSource = @"#version 450


layout(std140, binding = 0) uniform MatrixUniform
{
	mat4 ViewMatrix;
	mat4 ProjectionMatrix;
	vec3 ViewPosition;
} Matricies;


layout(location = 0) in vec3 local_position;
layout(location = 1) in vec2 in_texture;
layout(location = 2) in vec3 local_normal;
layout(location = 3) in vec3 tangent;
layout(location = 4) in vec3 bitangent;

out vec3 vertex_color;
out vec3 world_pos;
out vec3 world_normal;


void main() {
    mat3 modelmatrix = mat3(1.0);

    vec4 pos = Matricies.ProjectionMatrix * Matricies.ViewMatrix * mat4(modelmatrix) * vec4(local_position, 1);
    world_pos = local_position;

    world_normal = local_normal;

    vertex_color = vec3(0.5f,1,0.5f);
    gl_Position = pos;
}";

            var fragmentSource = @"#version 450

in vec3 world_pos;
in vec3 world_normal;
in vec3 vertex_color;
out vec4 out_color;

vec3 light_pos = vec3(50, 50, 500);
vec3 light_color = vec3(1,1,1);

void main() {
    float ambientStrength = 0.2;
    vec3 ambient = ambientStrength * light_color;

    vec3 norm = normalize(world_normal);
    vec3 lightDir = normalize(light_pos - world_pos);  

    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * light_color;

    vec3 result = (ambient + diffuse) * vertex_color;
    out_color = vec4(result, 1.0);
}";

            return CreateShader("Standard", vertexSource, fragmentSource);
        }

        public static int CreateShader(string shaderName)
        {
            string vertSrc;
            string fragSrc;

            var path = Path.Combine(Directory.GetCurrentDirectory(), "Shaders", shaderName);

            if(Directory.Exists(path) == false)
            {
                throw new Exception("Couldn't find shader folder: " + path);
            }

            vertSrc = File.ReadAllText(Path.Combine(path, shaderName + ".vert"));
            fragSrc = File.ReadAllText(Path.Combine(path, shaderName + ".frag"));

            return CreateShader(shaderName, vertSrc, fragSrc);
        }

        public static int CreateShader(string shaderName, string vertexSource, string fragmentSource)
        {
            var vertexShader = 0;
            var fragmentShader = 0;
            var geometryShader = 0;

            if (vertexSource != string.Empty)
                vertexShader = CompileShader(ShaderType.VertexShader, vertexSource, "vertex::" + shaderName);

            if (fragmentSource != string.Empty)
                fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentSource, "fragment::" + shaderName);

            var program = GL.CreateProgram();

            if (vertexShader != 0)
                GL.AttachShader(program, vertexShader);

            if (fragmentShader != 0)
                GL.AttachShader(program, fragmentShader);

            if (geometryShader != 0)
                GL.AttachShader(program, geometryShader);

            GL.LinkProgram(program);

            var linkResult = 0;
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out linkResult);
            if (linkResult == 0)
            {
                string linkLog;

                GL.GetProgramInfoLog(program, out linkLog);

                Console.WriteLine("CREATE PROGRAM FAILED");
                Console.WriteLine(linkLog);
                return 0;
            }

            return program;
        }

        private static int CompileShader(ShaderType type, string sourceCode, string shaderName)
        {
            var statusCode = 0;
            var shader = GL.CreateShader(type);

            GL.ShaderSource(shader, sourceCode);

            GL.CompileShader(shader);

            var shaderStatus = GL.GetShaderInfoLog(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out statusCode);

            if (statusCode != 0)
                return shader;

            Console.WriteLine("-- Shader Error --");
            Console.WriteLine("-- Could not create shader: {0}", shaderName);
            Console.WriteLine(shaderStatus);

            return 0;
        }
    }
}
