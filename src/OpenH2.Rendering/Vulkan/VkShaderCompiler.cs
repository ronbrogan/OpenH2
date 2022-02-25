using OpenH2.Core.Extensions;
using OpenH2.Rendering.Shaders;
using Silk.NET.Vulkan;
using System;
using System.IO;


namespace OpenH2.Rendering.Vulkan
{
    internal unsafe sealed class VkShader : IDisposable
    {
        private readonly Vk vk;
        private readonly Device device;
        public ShaderModule module;
        public PipelineShaderStageCreateInfo stageInfo;

        public VkShader(Vk vk, Device device, string shaderName, ShaderType type, string entryPoint = "main")
        {
            this.vk = vk;
            this.device = device;

            this.module = VkShaderCompiler.LoadSpirvShader(vk, device, shaderName, type);

            var stage = type switch
            {
                ShaderType.Vertex => ShaderStageFlags.ShaderStageVertexBit,
                ShaderType.Fragment => ShaderStageFlags.ShaderStageFragmentBit,
                _ => throw new NotSupportedException($"Shader type {type} is not yet supported"),
            };

            stageInfo = new PipelineShaderStageCreateInfo
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = stage,
                Module = module,
                PName = PinnedUtf8.Get(entryPoint)
            };
        }

        public void Dispose()
        {
            vk.DestroyShaderModule(this.device, this.module, null);
        }
    }

    internal class VkShaderCompiler
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

        public unsafe static ShaderModule LoadSpirvShader(Vk vk, Device device, string shaderName, ShaderType type)
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

                if (vk.CreateShaderModule(device, in createInfo, null, out var shader) != Result.Success)
                    throw new Exception("Unable to compile shader");

                return shader;
            }
        }
    }
}
