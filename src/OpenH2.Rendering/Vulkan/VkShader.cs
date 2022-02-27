using OpenH2.Core.Extensions;
using OpenH2.Rendering.Shaders;
using Silk.NET.Vulkan;
using System;


namespace OpenH2.Rendering.Vulkan
{
    internal unsafe sealed class VkShader : IDisposable
    {
        private readonly VkDevice device;
        public ShaderModule module;
        public PipelineShaderStageCreateInfo stageInfo;

        public VkShader(VkDevice device, string shaderName, ShaderType type, string entryPoint = "main")
        {
            this.device = device;

            this.module = VulkanShaderCompiler.LoadSpirvShader(device, shaderName, type);

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
            this.device.vk.DestroyShaderModule(this.device, this.module, null);
        }
    }
}
