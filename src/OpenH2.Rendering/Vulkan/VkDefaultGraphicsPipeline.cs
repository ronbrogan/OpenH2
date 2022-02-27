using OpenH2.Rendering.Shaders;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenH2.Rendering.Vulkan
{
    internal unsafe class VkDefaultGraphicsPipeline : VkObject, IDisposable
    {
        private readonly VkDevice device;
        private readonly VkSwapchain swapchain;

        private DescriptorSetLayout descriptorSetLayout;
        private PipelineLayout pipelineLayout;

        private RenderPass renderPass;
        private Pipeline graphicsPipeline;
        private DescriptorSet descriptorSet;

        public VkDefaultGraphicsPipeline(VulkanHost host, VkDevice device, VkSwapchain swapchain) : base(host.vk)
        {
            this.device = device;
            this.swapchain = swapchain;

            this.CreateResources();
        }

        internal void BeginPass(in CommandBuffer commandBuffer, uint imageIndex)
        {
            var clearColor = new ClearValue(new ClearColorValue(0f, 0f, 0f, 1f));
            var renderBegin = new RenderPassBeginInfo
            {
                SType = StructureType.RenderPassBeginInfo,
                RenderPass = renderPass,
                Framebuffer = swapchain.Framebuffers[imageIndex],
                RenderArea = new Rect2D(new Offset2D(0, 0), swapchain.Extent),
                ClearValueCount = 1,
                PClearValues = &clearColor
            };

            vk.CmdBeginRenderPass(commandBuffer, in renderBegin, SubpassContents.Inline);
        }

        public void Bind(in CommandBuffer commandBuffer)
        {
            vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Graphics, graphicsPipeline);

            vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Graphics, pipelineLayout, 0, 1, in descriptorSet, 0, null);
        }

        public void CreateResources()
        {
            // =======================
            // descriptor set setup
            // =======================

            // TODO: auto generate descriptors/bindings
            var uboBinding = new DescriptorSetLayoutBinding
            {
                Binding = 0,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ShaderStageVertexBit,
                PImmutableSamplers = null
            };

            var texBinding = new DescriptorSetLayoutBinding
            { 
                Binding =1,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.CombinedImageSampler,
                PImmutableSamplers= null,
                StageFlags = ShaderStageFlags.ShaderStageFragmentBit
            };

            var bindings = stackalloc DescriptorSetLayoutBinding[] { uboBinding, texBinding };

            var descCreate = new DescriptorSetLayoutCreateInfo
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = 2,
                PBindings = bindings
            };

            SUCCESS(vk.CreateDescriptorSetLayout(device, in descCreate, null, out descriptorSetLayout), "Descriptor set layout create failed");

            // TODO: auto generate binding/attribute descriptions
            var binding = new VertexInputBindingDescription()
            {
                Binding = 0,
                InputRate = VertexInputRate.Vertex,
                Stride = (uint)sizeof(VulkanTestVertex)
            };

            var posAttr = new VertexInputAttributeDescription
            {
                Binding = 0,
                Location = 0,
                Format = Format.R32G32Sfloat,
                Offset = 0,
            };

            var colorAttr = new VertexInputAttributeDescription
            {
                Binding = 0,
                Location = 1,
                Format = Format.R32G32B32Sfloat,
                Offset = (uint)sizeof(Vector2)
            };

            var texAttr = new VertexInputAttributeDescription
            {
                Binding = 0,
                Location = 2,
                Format = Format.R32G32Sfloat,
                Offset = (uint)sizeof(Vector2) + (uint)sizeof(Vector3),
            };

            var attrs = stackalloc VertexInputAttributeDescription[] { posAttr, colorAttr, texAttr };

            var vertInput = new PipelineVertexInputStateCreateInfo
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexBindingDescriptionCount = 1,
                PVertexBindingDescriptions = &binding,
                VertexAttributeDescriptionCount = 3,
                PVertexAttributeDescriptions = attrs
            };

            var inputAssembly = new PipelineInputAssemblyStateCreateInfo
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = PrimitiveTopology.TriangleList,
                PrimitiveRestartEnable = false
            };

            var viewport = new Viewport(0, 0, swapchain.Extent.Width, swapchain.Extent.Height, 0, 1f);
            var scissor = new Rect2D(new Offset2D(0, 0), swapchain.Extent);

            var viewportState = new PipelineViewportStateCreateInfo
            {
                SType = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = 1,
                PViewports = &viewport,
                ScissorCount = 1,
                PScissors = &scissor,
            };

            var rasterizer = new PipelineRasterizationStateCreateInfo
            {
                SType = StructureType.PipelineRasterizationStateCreateInfo,
                DepthClampEnable = false,
                RasterizerDiscardEnable = false,
                PolygonMode = PolygonMode.Fill,
                LineWidth = 1,
                CullMode = CullModeFlags.CullModeBackBit,
                FrontFace = FrontFace.CounterClockwise,
                DepthBiasEnable = false,
                DepthBiasConstantFactor = 0,
                DepthBiasClamp = 0,
                DepthBiasSlopeFactor = 0
            };

            var msaa = new PipelineMultisampleStateCreateInfo
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,
                SampleShadingEnable = false,
                RasterizationSamples = SampleCountFlags.SampleCount1Bit,
                MinSampleShading = 1
            };

            var colorBlend = new PipelineColorBlendAttachmentState
            {
                ColorWriteMask = ColorComponentFlags.ColorComponentRBit | ColorComponentFlags.ColorComponentGBit | ColorComponentFlags.ColorComponentBBit | ColorComponentFlags.ColorComponentABit,
                BlendEnable = true,
                SrcColorBlendFactor = BlendFactor.SrcAlpha,
                DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
                ColorBlendOp = BlendOp.Add,
                SrcAlphaBlendFactor = BlendFactor.One,
                DstAlphaBlendFactor = BlendFactor.Zero,
                AlphaBlendOp = BlendOp.Add
            };

            var colorBlendState = new PipelineColorBlendStateCreateInfo()
            {
                SType = StructureType.PipelineColorBlendStateCreateInfo,
                LogicOpEnable = false,
                LogicOp = LogicOp.Copy,
                AttachmentCount = 1,
                PAttachments = &colorBlend
            };

            var dynamicStates = stackalloc DynamicState[] { DynamicState.Viewport, DynamicState.LineWidth };

            var dynamicState = new PipelineDynamicStateCreateInfo
            {
                SType = StructureType.PipelineDynamicStateCreateInfo,
                DynamicStateCount = 2,
                PDynamicStates = dynamicStates,
            };

            var descriptors = stackalloc DescriptorSetLayout[] { descriptorSetLayout };
            var layoutCreate = new PipelineLayoutCreateInfo
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = 1,
                PSetLayouts = descriptors
            };

            SUCCESS(vk.CreatePipelineLayout(device, in layoutCreate, null, out pipelineLayout), "Pipeline layout create failed");

            // =======================
            // start render pass setup
            // =======================

            var colorAttach = new AttachmentDescription
            {
                Format = device.SurfaceFormat.Format,
                Samples = SampleCountFlags.SampleCount1Bit,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.PresentSrcKhr
            };

            var colorAttachRef = new AttachmentReference(0, ImageLayout.ColorAttachmentOptimal);

            var subpass = new SubpassDescription
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachmentCount = 1,
                PColorAttachments = &colorAttachRef,
            };

            var dependency = new SubpassDependency
            {
                SrcSubpass = Vk.SubpassExternal,
                DstSubpass = 0,
                SrcStageMask = PipelineStageFlags.PipelineStageColorAttachmentOutputBit,
                SrcAccessMask = 0,
                DstStageMask = PipelineStageFlags.PipelineStageColorAttachmentOutputBit,
                DstAccessMask = AccessFlags.AccessColorAttachmentWriteBit,
            };

            var renderPassCreate = new RenderPassCreateInfo
            {
                SType = StructureType.RenderPassCreateInfo,
                AttachmentCount = 1,
                PAttachments = &colorAttach,
                SubpassCount = 1,
                PSubpasses = &subpass,
                DependencyCount = 1,
                PDependencies = &dependency
            };

            SUCCESS(vk.CreateRenderPass(device, in renderPassCreate, null, out renderPass), "Render pass create failed");

            using var vertShader = new VkShader(device, "VulkanTest", ShaderType.Vertex);
            using var fragShader = new VkShader(device, "VulkanTest", ShaderType.Fragment);

            var shaderStages = stackalloc PipelineShaderStageCreateInfo[] { vertShader.stageInfo, fragShader.stageInfo };

            var pipelineCreate = new GraphicsPipelineCreateInfo
            {
                SType = StructureType.GraphicsPipelineCreateInfo,
                StageCount = 2,
                PStages = shaderStages,
                PVertexInputState = &vertInput,
                PInputAssemblyState = &inputAssembly,
                PViewportState = &viewportState,
                PRasterizationState = &rasterizer,
                PMultisampleState = &msaa,
                PDepthStencilState = null,
                PColorBlendState = &colorBlendState,
                //PDynamicState = &dynamicState,
                Layout = pipelineLayout,
                RenderPass = renderPass,
                Subpass = 0,
                BasePipelineHandle = default,
                BasePipelineIndex = -1
            };

            SUCCESS(vk.CreateGraphicsPipelines(device, default, 1, &pipelineCreate, null, out graphicsPipeline), "Pipeline create failed");

            swapchain.InitializeFramebuffers(renderPass);
        }

        public void CreateDescriptors(VkBuffer<UBO> ubo, VkImage image, VkSampler sampler)
        {
            var layouts = stackalloc DescriptorSetLayout[] { descriptorSetLayout };
            var alloc = new DescriptorSetAllocateInfo
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = device.DescriptorPool,
                DescriptorSetCount = 1,
                PSetLayouts = layouts
            };

            SUCCESS(vk.AllocateDescriptorSets(device, in alloc, out descriptorSet), "DescriptorSet allocate failed");

            var uboInfo = new DescriptorBufferInfo(ubo, 0, Vk.WholeSize);

            var texInfo = new DescriptorImageInfo(sampler, image.View, ImageLayout.ShaderReadOnlyOptimal);

            var writes = stackalloc WriteDescriptorSet[]
            {
                new WriteDescriptorSet
                {
                    SType = StructureType.WriteDescriptorSet,
                    DstSet = descriptorSet,
                    DstBinding = 0,
                    DstArrayElement = 0,
                    DescriptorType = DescriptorType.UniformBuffer,
                    DescriptorCount = 1,
                    PBufferInfo = &uboInfo,
                    PImageInfo = null,
                    PTexelBufferView = null
                },
                new WriteDescriptorSet
                {
                    SType = StructureType.WriteDescriptorSet,
                    DstSet = descriptorSet,
                    DstBinding = 1,
                    DstArrayElement = 0,
                    DescriptorType = DescriptorType.CombinedImageSampler,
                    DescriptorCount = 1,
                    PImageInfo = &texInfo
                }
            };

            vk.UpdateDescriptorSets(device, 2, writes, 0, null);
        }

        public void DestroyResources()
        {
            vk.DestroyDescriptorSetLayout(device, descriptorSetLayout, null);
            vk.DestroyPipeline(device, graphicsPipeline, null);
            vk.DestroyPipelineLayout(device, pipelineLayout, null);
            vk.DestroyRenderPass(device, renderPass, null);
        }

        public void Dispose()
        {
            this.DestroyResources();
        }
    }
}
