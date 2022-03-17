using System;
using System.Collections.Concurrent;
using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Rendering.Shaders;
using Silk.NET.Vulkan;

namespace OpenH2.Rendering.Vulkan.Internals.GraphicsPipelines
{
    internal class PipelineStore : IDisposable
    {
        private ConcurrentDictionary<(Shader, MeshElementType), GeneralGraphicsPipeline> pipelines = new();
        private readonly VkDevice device;
        private readonly VkSwapchain swapchain;
        private readonly VkRenderPass renderPass;
        private readonly ShadowMapPass shadowPass;
        private PipelineConfig[] shaderConfigs = new PipelineConfig[(int)Shader.MAX_VALUE];
        private PipelineBinding[] defaultBindings = new PipelineBinding[]
        {
            new (0, DescriptorType.UniformBuffer),
            new (1, DescriptorType.UniformBufferDynamic),
            new (2, DescriptorType.UniformBuffer),
            new (3, DescriptorType.CombinedImageSampler, 8, DescriptorBindingFlags.DescriptorBindingPartiallyBoundBit),
            new (16, DescriptorType.CombinedImageSampler),
        };

        private PipelineBinding[] shadowMapBindings = new PipelineBinding[]
        {
            new (0, DescriptorType.UniformBuffer),
            new (1, DescriptorType.UniformBufferDynamic)
        };

        public PipelineStore(VkDevice device, VkSwapchain swapchain, VkRenderPass renderPass, ShadowMapPass shadowPass)
        {
            this.device = device;
            this.swapchain = swapchain;
            this.renderPass = renderPass;
            this.shadowPass = shadowPass;

            shaderConfigs[(int)Shader.Skybox] = new(Shader.Skybox, defaultBindings, 16, depthTest: false);
            shaderConfigs[(int)Shader.Generic] = new(Shader.Generic, defaultBindings, 4096);
            shaderConfigs[(int)Shader.Wireframe] = new(Shader.Wireframe, defaultBindings, 1024, polyMode: PolygonMode.Line);
            shaderConfigs[(int)Shader.ShadowMapping] = new(Shader.ShadowMapping, shadowMapBindings, 4096, SampleCountFlags.SampleCount1Bit, invertY: false);
        }

        public GeneralGraphicsPipeline GetOrCreate(Shader shader, MeshElementType primitiveType)
        {
            var config = shaderConfigs[(int)shader];

            if (config == null)
                throw new Exception($"No config for shader {shader}");

            RenderPass pipelinePass = shader == Shader.ShadowMapping
                ? this.shadowPass
                : this.renderPass;

            Extent2D size = shader == Shader.ShadowMapping
                ? new Extent2D(ShadowMapPass.MapSize, ShadowMapPass.MapSize)
                : this.swapchain.Extent;

            bool swapchainTarget = shader != Shader.ShadowMapping;

            return pipelines.GetOrAdd((shader, primitiveType), 
                k => new GeneralGraphicsPipeline(this.device, pipelinePass, size, config, primitiveType, swapchainTarget));
        }

        public void DestroySwapchainResources()
        {
            foreach (var (_, inst) in pipelines)
                if (inst.ShouldRecreate())
                    inst.DestroyResources();
        }

        public void CreateSwapchainResources()
        {
            foreach (var (_, inst) in pipelines)
                if(inst.ShouldRecreate())
                    inst.CreateResources(this.swapchain.Extent);
        }

        public void Dispose()
        {
            foreach (var (_, inst) in pipelines)
                inst.Dispose();
        }
    }
}
