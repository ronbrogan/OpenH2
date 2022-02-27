using OpenH2.Rendering.Shaders;
using Silk.NET.Vulkan;
using System;
using System.IO;


namespace OpenH2.Rendering.Vulkan
{

    internal class VulkanShaderCompiler
    {
        public static byte[] LoadSpirvBytes(string shaderName, ShaderType type)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Shaders", shaderName);

            if (Directory.Exists(path) == false)
            {
                throw new Exception("Couldn't find shader folder: " + path);
            }

            var stub = type switch
            {
                ShaderType.Vertex => "vert",
                ShaderType.Fragment => "frag",
                _ => throw new NotSupportedException($"Shader type {type} is not yet supported")
            };

            return File.ReadAllBytes(Path.Combine(path, stub + ".spv"));
        }

        public unsafe static ShaderModule LoadSpirvShader(VkDevice device, string shaderName, ShaderType type)
        {
            var bytes = LoadSpirvBytes(shaderName, type);

            fixed (byte* ptr = bytes)
            {
                var createInfo = new ShaderModuleCreateInfo
                {
                    SType = StructureType.ShaderModuleCreateInfo,
                    CodeSize = (uint)bytes.Length,
                    PCode = (uint*)ptr
                };

                if (device.vk.CreateShaderModule(device, in createInfo, null, out var shader) != Result.Success)
                    throw new Exception("Unable to compile shader");

                return shader;
            }
        }
    }
}
