using OpenH2.Core.Extensions;
using OpenH2.Rendering.Shaders;
using Silk.NET.Vulkan;
using System;


namespace OpenH2.Rendering.Vulkan.Internals
{
    internal unsafe sealed class VkShader : IDisposable
    {
        private readonly VkDevice device;
        public ShaderModule module;
        public PipelineShaderStageCreateInfo stageInfo;

        public static VkShader CreateIfPresent(VkDevice device, Shader shader, ShaderType type, string entryPoint = "main")
        {
            if (!VulkanShaderCompiler.IsPresent(shader, type))
                return null;

            return new VkShader(device, shader, type, entryPoint);
        }

        public VkShader(VkDevice device, Shader shader, ShaderType type, string entryPoint = "main")
        {
            this.device = device;

            module = VulkanShaderCompiler.LoadSpirvShader(device, shader, type);

            var stage = type switch
            {
                ShaderType.Vertex => ShaderStageFlags.ShaderStageVertexBit,
                ShaderType.Fragment => ShaderStageFlags.ShaderStageFragmentBit,
                ShaderType.Geometry => ShaderStageFlags.ShaderStageGeometryBit,
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
            device.vk.DestroyShaderModule(device, module, null);
        }
    }
}
