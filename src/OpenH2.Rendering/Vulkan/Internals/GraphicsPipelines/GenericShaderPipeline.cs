using OpenH2.Foundation;
using OpenH2.Rendering.Shaders;
using Silk.NET.Vulkan;

namespace OpenH2.Rendering.Vulkan.Internals.GraphicsPipelines
{
    internal class GenericTriListPipeline : GenericShaderPipeline<GenericTriListPipeline>
    {
        public GenericTriListPipeline(VkDevice device, VkSwapchain swapchain, VkRenderPass renderPass)
            : base(device, swapchain, renderPass, MeshElementType.TriangleList)
        {
        }
    }

    internal class GenericTriStripPipeline : GenericShaderPipeline<GenericTriStripPipeline>
    {
        public GenericTriStripPipeline(VkDevice device, VkSwapchain swapchain, VkRenderPass renderPass)
            : base(device, swapchain, renderPass, MeshElementType.TriangleStrip)
        {
        }
    }

    internal unsafe class GenericShaderPipeline<T> : BaseGraphicsPipeline<T>
    {
        // TODO: change to Shader.Generic
        public GenericShaderPipeline(VkDevice device, VkSwapchain swapchain, VkRenderPass renderPass, MeshElementType primitiveType) 
            : base(device, swapchain, renderPass, Shader.VulkanTest, primitiveType)
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

            var texBinding = new DescriptorSetLayoutBinding
            {
                Binding = 2,
                DescriptorType = DescriptorType.CombinedImageSampler,
                DescriptorCount = 8,
                StageFlags = ShaderStageFlags.ShaderStageAllGraphics,
                PImmutableSamplers = null
            };

            var bindings = stackalloc[] { globalsBinding, transformBinding, texBinding };

            var descCreate = new DescriptorSetLayoutCreateInfo
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = 3,
                PBindings = bindings
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
            var uboPoolSize = new DescriptorPoolSize(DescriptorType.UniformBuffer, 1);
            var texPoolSize = new DescriptorPoolSize(DescriptorType.CombinedImageSampler, 1);
            var sizes = stackalloc DescriptorPoolSize[] { uboPoolSize, texPoolSize };
            var createInfo =  new DescriptorPoolCreateInfo
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = 2,
                PPoolSizes = sizes,
                MaxSets = 2
            };

            SUCCESS(vk.CreateDescriptorPool(device, in createInfo, null, out var descriptorPool));
            return descriptorPool;
        }


    }
}
