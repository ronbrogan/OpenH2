using System;
using System.IO;
using Silk.NET.OpenGL;
using Shader = OpenH2.Rendering.Shaders.Shader;

namespace OpenH2.Rendering.OpenGL
{
    public static class OpenGLShaderCompiler
    {
        private static OpenGLHost host;
        private static GL gl => host.gl;

        public static void UseHost(OpenGLHost host) { OpenGLShaderCompiler.host = host; }

        public static uint CreateShader(Shader shader)
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

        public static uint CreateShader(string shaderName, string vertexSource, string fragmentSource, string geomSource = null)
        {
            var vertexShader = 0u;
            var fragmentShader = 0u;
            var geometryShader = 0u;

            if (string.IsNullOrWhiteSpace(vertexSource) == false)
                vertexShader = CompileShader(Silk.NET.OpenGL.ShaderType.VertexShader, vertexSource, "vertex::" + shaderName);

            if (string.IsNullOrWhiteSpace(fragmentSource) == false)
                fragmentShader = CompileShader(Silk.NET.OpenGL.ShaderType.FragmentShader, fragmentSource, "fragment::" + shaderName);

            if (string.IsNullOrWhiteSpace(geomSource) == false)
                geometryShader = CompileShader(Silk.NET.OpenGL.ShaderType.GeometryShader, geomSource, "geom::" + shaderName);

            var program = gl.CreateProgram();

            if (vertexShader != 0)
                gl.AttachShader(program, vertexShader);

            if (fragmentShader != 0)
                gl.AttachShader(program, fragmentShader);

            if (geometryShader != 0)
                gl.AttachShader(program, geometryShader);

            gl.LinkProgram(program);

            var linkResult = 0;
            gl.GetProgram(program, GLEnum.LinkStatus, out linkResult);
            if (linkResult == 0)
            {
                string linkLog;

                gl.GetProgramInfoLog(program, out linkLog);

                Console.WriteLine("CREATE PROGRAM FAILED");
                Console.WriteLine(linkLog);
                return 0;
            }

            return program;
        }

        private static uint CompileShader(Silk.NET.OpenGL.ShaderType type, string sourceCode, string shaderName)
        {
            var statusCode = 0;
            var shader = gl.CreateShader(type);

            gl.ShaderSource(shader, sourceCode);

            gl.CompileShader(shader);

            var shaderStatus = gl.GetShaderInfoLog(shader);

            gl.GetShader(shader, GLEnum.CompileStatus, out statusCode);

            if (statusCode != 0)
                return shader;

            Console.WriteLine("-- Shader Error --");
            Console.WriteLine("-- Could not create shader: {0}", shaderName);
            Console.WriteLine(shaderStatus);

            return 0;
        }
    }
}
