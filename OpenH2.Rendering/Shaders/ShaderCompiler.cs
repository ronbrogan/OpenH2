using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL;

namespace OpenH2.Rendering.Shaders
{
    public static class ShaderCompiler
    {
        private static Dictionary<ShaderType, string> ShaderExtensions = new Dictionary<ShaderType, string>
        {
            { ShaderType.VertexShader, "vert" },
            { ShaderType.GeometryShader, "geom" },
            { ShaderType.FragmentShader, "frag" },
            { ShaderType.ComputeShader, "comp" }
        };

        public static int CreateShader(Shader shader)
        {
            var shaderName = shader.ToString();

            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "Shaders", shaderName);

            if(Directory.Exists(basePath) == false)
            {
                throw new Exception("Couldn't find shader folder: " + basePath);
            }

            var sources = new Dictionary<ShaderType, string>();

            foreach(var type in ShaderExtensions.Keys)
            {
                var path = Path.Combine(basePath, shaderName + "." + ShaderExtensions[type]);
                if (File.Exists(path) == false)
                    continue;

                sources[type] = File.ReadAllText(path);
            }

            return CreateShader(shaderName, sources);
        }

        public static int CreateComputeShader(Shader shader)
        {
            return 0;
        }

        public static int CreateShader(string shaderName, Dictionary<ShaderType, string> sources)
        {
            var program = GL.CreateProgram();

            foreach (var source in sources)
            {
                var compiledShader = CompileShader(source.Key, source.Value, source.Key+"::"+shaderName);

                if(compiledShader > 0)
                    GL.AttachShader(program, compiledShader);
            }

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
