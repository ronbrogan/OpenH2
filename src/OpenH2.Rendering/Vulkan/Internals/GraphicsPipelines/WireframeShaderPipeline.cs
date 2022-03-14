using OpenH2.Foundation;
using OpenH2.Rendering.Shaders;
using Silk.NET.Vulkan;

namespace OpenH2.Rendering.Vulkan.Internals.GraphicsPipelines
{
    internal class WireframeTriListPipeline : WireframeShaderPipeline<WireframeTriListPipeline>
    {
        public WireframeTriListPipeline(VkDevice device, VkSwapchain swapchain, VkRenderPass renderPass, ShadowMapPass shadowPass)
            : base(device, swapchain, renderPass, shadowPass, MeshElementType.TriangleList)
        {
        }
    }

    internal class WireframeTriStripPipeline : WireframeShaderPipeline<WireframeTriStripPipeline>
    {
        public WireframeTriStripPipeline(VkDevice device, VkSwapchain swapchain, VkRenderPass renderPass, ShadowMapPass shadowPass)
            : base(device, swapchain, renderPass, shadowPass, MeshElementType.TriangleStrip)
        {
        }
    }

    internal unsafe class WireframeShaderPipeline<T> : BaseGraphicsPipeline<T>
    {
        public WireframeShaderPipeline(VkDevice device, VkSwapchain swapchain, VkRenderPass renderPass, ShadowMapPass shadowPass, MeshElementType primitiveType) 
            : base(device, swapchain, renderPass, shadowPass, Shader.Wireframe, primitiveType, depthTestEnable: true, PolygonMode.Line, CullModeFlags.CullModeNone)
        {
        }

        protected override (DescriptorSetLayout, PipelineLayout) CreateLayouts()
        {
            

            var globalsBinding = new DescriptorSetLayoutBinding
            {
                Binding = 0,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ShaderStageAllGraphics,
                PImmutableSamplers = null
            };

            var transformBinding = new DescriptorSetLayoutBinding
            {
                Binding = 1,
                DescriptorType = DescriptorType.UniformBufferDynamic,
                DescriptorCount = 1,
                PImmutableSamplers = null,
                StageFlags = ShaderStageFlags.ShaderStageAllGraphics
            };

            var shaderUniformBinding = new DescriptorSetLayoutBinding
            {
                Binding = 2,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ShaderStageAllGraphics,
                PImmutableSamplers = null
            };

            var texBinding = new DescriptorSetLayoutBinding
            {
                Binding = 3,
                DescriptorType = DescriptorType.CombinedImageSampler,
                DescriptorCount = 8,
                StageFlags = ShaderStageFlags.ShaderStageAllGraphics,
                PImmutableSamplers = null
            };

            var shadowMapBinding = new DescriptorSetLayoutBinding
            {
                Binding = 16,
                DescriptorType = DescriptorType.CombinedImageSampler,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ShaderStageAllGraphics,
                PImmutableSamplers = null
            };

            var bindings = stackalloc[] { globalsBinding, transformBinding, shaderUniformBinding, texBinding, shadowMapBinding };

            var noneBindFlag = (DescriptorBindingFlags)0;
            var bindingFlagValues = stackalloc[] { noneBindFlag, noneBindFlag, noneBindFlag, DescriptorBindingFlags.DescriptorBindingPartiallyBoundBit, noneBindFlag };

            var flags = new DescriptorSetLayoutBindingFlagsCreateInfo
            {
                SType = StructureType.DescriptorSetLayoutBindingFlagsCreateInfo,
                BindingCount = 5,
                PBindingFlags = bindingFlagValues
            };

            var descCreate = new DescriptorSetLayoutCreateInfo
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = 5,
                PBindings = bindings,
                PNext = &flags
            };

            SUCCESS(vk.CreateDescriptorSetLayout(device, in descCreate, null, out var descriptorSetLayout), "Descriptor set layout create failed");

            var descriptors = stackalloc[] { descriptorSetLayout };
            var layoutCreate = new PipelineLayoutCreateInfo
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = 1,
                PSetLayouts = descriptors
            };

            SUCCESS(vk.CreatePipelineLayout(device, in layoutCreate, null, out var pipelineLayout), "Pipeline layout create failed");

            return (descriptorSetLayout, pipelineLayout);
        }

        protected override DescriptorPool CreateDescriptorPool()
        {
            var count = 16u;

            var uboPoolSize = new DescriptorPoolSize(DescriptorType.UniformBuffer, 4 * count);
            var texPoolSize = new DescriptorPoolSize(DescriptorType.CombinedImageSampler, 1 * count);
            var sizes = stackalloc DescriptorPoolSize[] { uboPoolSize, texPoolSize };
            var createInfo =  new DescriptorPoolCreateInfo
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = 2,
                PPoolSizes = sizes,
                MaxSets = 1 * count
            };

            SUCCESS(vk.CreateDescriptorPool(device, in createInfo, null, out var descriptorPool));
            return descriptorPool;
        }


    }
}
