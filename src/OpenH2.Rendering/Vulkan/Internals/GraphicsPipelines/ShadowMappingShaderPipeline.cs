using OpenH2.Foundation;
using OpenH2.Rendering.Shaders;
using Silk.NET.Vulkan;

namespace OpenH2.Rendering.Vulkan.Internals.GraphicsPipelines
{
    internal class ShadowMappingTriListPipeline : ShadowMappingShaderPipeline<ShadowMappingTriListPipeline>
    {
        public ShadowMappingTriListPipeline(VkDevice device, VkSwapchain swapchain, VkRenderPass renderPass)
            : base(device, swapchain, renderPass, MeshElementType.TriangleList)
        {
        }
    }

    internal class ShadowMappingTriStripPipeline : ShadowMappingShaderPipeline<ShadowMappingTriStripPipeline>
    {
        public ShadowMappingTriStripPipeline(VkDevice device, VkSwapchain swapchain, VkRenderPass renderPass)
            : base(device, swapchain, renderPass, MeshElementType.TriangleStrip)
        {
        }
    }

    internal unsafe class ShadowMappingShaderPipeline<T> : BaseGraphicsPipeline<T>
    {
        public ShadowMappingShaderPipeline(VkDevice device, VkSwapchain swapchain, VkRenderPass renderPass, MeshElementType primitiveType) 
            : base(device, swapchain, renderPass, Shader.ShadowMapping, primitiveType, depthTestEnable: true, PolygonMode.Line, CullModeFlags.CullModeNone)
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

            var bindings = stackalloc[] { globalsBinding, transformBinding, shaderUniformBinding, texBinding };

            var noneBindFlag = (DescriptorBindingFlags)0;
            var bindingFlagValues = stackalloc[] { noneBindFlag, noneBindFlag, noneBindFlag, DescriptorBindingFlags.DescriptorBindingPartiallyBoundBit };

            var flags = new DescriptorSetLayoutBindingFlagsCreateInfo
            {
                SType = StructureType.DescriptorSetLayoutBindingFlagsCreateInfo,
                BindingCount = 4,
                PBindingFlags = bindingFlagValues
            };

            var descCreate = new DescriptorSetLayoutCreateInfo
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = 4,
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
