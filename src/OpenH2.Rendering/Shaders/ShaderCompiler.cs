using System;
using System.IO;
using OpenTK.Graphics.OpenGL;

namespace OpenH2.Rendering.Shaders
{
    public static class ShaderCompiler
    {
        public static int CreateShader(Shader shader)
        {
            var shaderName = shader.ToString();
            string vertSrc = null;
            string fragSrc = null;
            string geomSrc = null;

            var path = Path.Combine(Directory.GetCurrentDirectory(), "Shaders", shaderName);

            if(Directory.Exists(path) == false)
            {
                throw new Exception("Couldn't find shader folder: " + path);
            }

            vertSrc = File.ReadAllText(Path.Combine(path, shaderName + ".vert"));
            fragSrc = File.ReadAllText(Path.Combine(path, shaderName + ".frag"));

            var geomPath = Path.Combine(path, shaderName + ".geom");
            if(File.Exists(geomPath))
                geomSrc = File.ReadAllText(geomPath);

            return CreateShader(shaderName, vertSrc, fragSrc, geomSrc);
        }

        public static int CreateComputeShader(Shader shader)
        {
            return 0;
        }

        public static int CreateShader(string shaderName, string vertexSource, string fragmentSource, string geomSource = null)
        {
            var vertexShader = 0;
            var fragmentShader = 0;
            var geometryShader = 0;

            if (string.IsNullOrWhiteSpace(vertexSource) == false)
                vertexShader = CompileShader(OpenTK.Graphics.OpenGL.ShaderType.VertexShader, vertexSource, "vertex::" + shaderName);

            if (string.IsNullOrWhiteSpace(fragmentSource) == false)
                fragmentShader = CompileShader(OpenTK.Graphics.OpenGL.ShaderType.FragmentShader, fragmentSource, "fragment::" + shaderName);

            if (string.IsNullOrWhiteSpace(geomSource) == false)
                geometryShader = CompileShader(OpenTK.Graphics.OpenGL.ShaderType.GeometryShader, geomSource, "geom::" + shaderName);

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

        private static int CompileShader(OpenTK.Graphics.OpenGL.ShaderType type, string sourceCode, string shaderName)
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
