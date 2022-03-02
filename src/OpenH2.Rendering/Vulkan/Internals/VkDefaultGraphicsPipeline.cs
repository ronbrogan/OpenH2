using OpenH2.Foundation;
using OpenH2.Rendering.Shaders;
using Silk.NET.Vulkan;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace OpenH2.Rendering.Vulkan.Internals
{
    internal unsafe class VkDefaultGraphicsPipeline : VkObject, IDisposable
    {
        private readonly VkDevice device;
        private readonly VkSwapchain swapchain;
        private readonly VkRenderPass renderPass;
        private readonly MeshElementType primitiveType;

        private DescriptorSetLayout descriptorSetLayout;
        private PipelineLayout pipelineLayout;

        private Pipeline graphicsPipeline;
        private DescriptorSet descriptorSet;

        public VkDefaultGraphicsPipeline(VulkanHost host, VkDevice device, VkSwapchain swapchain, VkRenderPass renderPass, MeshElementType primitiveType) : base(host.vk)
        {
            this.device = device;
            this.swapchain = swapchain;
            this.renderPass = renderPass;
            this.primitiveType = primitiveType;
            CreateResources();
        }

        public void Bind(in CommandBuffer commandBuffer)
        {
            vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Graphics, graphicsPipeline);

        }

        public void BindDescriptors(in CommandBuffer commandBuffer)
        {
            vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Graphics, pipelineLayout, 0, 1, in descriptorSet, 0, null);
        }

        public void BindDescriptors(in CommandBuffer commandBuffer, Span<uint> dynamicOffsets)
        {
            Debug.Assert(dynamicOffsets.Length > 0);
            var ptr = (uint*)Unsafe.AsPointer(ref dynamicOffsets[0]);
            vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Graphics, pipelineLayout, 0, 1, in descriptorSet, (uint)dynamicOffsets.Length, ptr);
        }

        public void CreateResources()
        {
            // =======================
            // descriptor set setup
            // =======================

            // TODO: auto generate descriptors/bindings
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

            SUCCESS(vk.CreateDescriptorSetLayout(device, in descCreate, null, out descriptorSetLayout), "Descriptor set layout create failed");

            // TODO: auto generate binding/attribute descriptions
            var binding = new VertexInputBindingDescription()
            {
                Binding = 0,
                InputRate = VertexInputRate.Vertex,
                Stride = (uint)sizeof(VertexFormat)
            };

            var posAttr = new VertexInputAttributeDescription
            {
                Binding = 0,
                Location = 0,
                Format = Format.R32G32B32Sfloat,
                Offset = (uint)VertexFormat.PositionOffset,
            };

            var texAttr = new VertexInputAttributeDescription
            {
                Binding = 0,
                Location = 1,
                Format = Format.R32G32Sfloat,
                Offset = (uint)VertexFormat.TexCoordsOffset,
            };

            var normalAttr = new VertexInputAttributeDescription
            {
                Binding = 0,
                Location = 2,
                Format = Format.R32G32B32Sfloat,
                Offset = (uint)VertexFormat.NormalOffset
            };

            var tanAttr = new VertexInputAttributeDescription
            {
                Binding = 0,
                Location = 3,
                Format = Format.R32G32B32Sfloat,
                Offset = (uint)VertexFormat.TangentOffset
            };

            var bitanAttr = new VertexInputAttributeDescription
            {
                Binding = 0,
                Location = 4,
                Format = Format.R32G32B32Sfloat,
                Offset = (uint)VertexFormat.BitangentOffset
            };

            var attrs = stackalloc[] { posAttr, texAttr, normalAttr, tanAttr, bitanAttr };

            var vertInput = new PipelineVertexInputStateCreateInfo
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexBindingDescriptionCount = 1,
                PVertexBindingDescriptions = &binding,
                VertexAttributeDescriptionCount = 5,
                PVertexAttributeDescriptions = attrs
            };

            var (topology, restart) = this.primitiveType switch
            {
                MeshElementType.TriangleList => (PrimitiveTopology.TriangleList, false),
                MeshElementType.TriangleStrip => (PrimitiveTopology.TriangleStrip, true),
                MeshElementType.TriangleStripDecal => (PrimitiveTopology.TriangleStrip, true),
                MeshElementType.Point => (PrimitiveTopology.PointList, false),
                _ => (PrimitiveTopology.TriangleList, false)
            };

            var inputAssembly = new PipelineInputAssemblyStateCreateInfo
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = topology,
                PrimitiveRestartEnable = restart
            };

            var viewport = new Viewport(0, swapchain.Extent.Height, swapchain.Extent.Width, -swapchain.Extent.Height, 0, 1f);
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

            var dynamicStates = stackalloc[] { DynamicState.Viewport, DynamicState.LineWidth, DynamicState.PrimitiveTopology, DynamicState.PrimitiveRestartEnable };

            var dynamicState = new PipelineDynamicStateCreateInfo
            {
                SType = StructureType.PipelineDynamicStateCreateInfo,
                DynamicStateCount = 3,
                PDynamicStates = dynamicStates,
            };

            var descriptors = stackalloc[] { descriptorSetLayout };
            var layoutCreate = new PipelineLayoutCreateInfo
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = 1,
                PSetLayouts = descriptors,
            };

            SUCCESS(vk.CreatePipelineLayout(device, in layoutCreate, null, out pipelineLayout), "Pipeline layout create failed");

            using var vertShader = new VkShader(device, "VulkanTest", ShaderType.Vertex);
            using var fragShader = new VkShader(device, "VulkanTest", ShaderType.Fragment);

            var shaderStages = stackalloc[] { vertShader.stageInfo, fragShader.stageInfo };

            var depthStencil = new PipelineDepthStencilStateCreateInfo
            {
                SType = StructureType.PipelineDepthStencilStateCreateInfo,
                DepthTestEnable = true,
                DepthWriteEnable = true,
                DepthCompareOp = CompareOp.Less,
                DepthBoundsTestEnable = false,
                StencilTestEnable = false,
            };

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
                PDepthStencilState = &depthStencil,
                PColorBlendState = &colorBlendState,
                //PDynamicState = &dynamicState,
                Layout = pipelineLayout,
                RenderPass = renderPass,
                Subpass = 0,
                BasePipelineHandle = default,
                BasePipelineIndex = -1,
            };

            SUCCESS(vk.CreateGraphicsPipelines(device, default, 1, &pipelineCreate, null, out graphicsPipeline), "Pipeline create failed");


        }

        public void CreateDescriptors(VkBuffer<GlobalUniform> globals, VkBuffer<TransformUniform> transform, VkBuffer<VulkanTestUniform> uniform, (VkImage image, VkSampler sampler)[] textures)
        {
            var layouts = stackalloc[] { descriptorSetLayout };
            var alloc = new DescriptorSetAllocateInfo
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = device.DescriptorPool,
                DescriptorSetCount = 1,
                PSetLayouts = layouts
            };

            SUCCESS(vk.AllocateDescriptorSets(device, in alloc, out descriptorSet), "DescriptorSet allocate failed");

            var globalsInfo = new DescriptorBufferInfo(globals, 0, Vk.WholeSize);
            var uniformInfo = new DescriptorBufferInfo(uniform, 0, Vk.WholeSize);
            var transformsInfo = new DescriptorBufferInfo(transform, 0, (ulong)device.AlignUboItem<TransformUniform>(1));

            var textureInfos = stackalloc DescriptorImageInfo[8];
            for (int i = 0; i < 8; i++)
            {
                if (textures.Length > i)
                    textureInfos[i] = new DescriptorImageInfo(textures[i].sampler, textures[i].image.View, ImageLayout.ShaderReadOnlyOptimal);
                else
                    // TODO: have a fallback texture or something?
                    textureInfos[i] = new DescriptorImageInfo(textures[0].sampler, textures[0].image.View, ImageLayout.ShaderReadOnlyOptimal);

            }

            ReadOnlySpan<WriteDescriptorSet> writes = stackalloc[]
            {
                new WriteDescriptorSet
                {
                    SType = StructureType.WriteDescriptorSet,
                    DstSet = descriptorSet,
                    DstBinding = 0,
                    DstArrayElement = 0,
                    DescriptorType = DescriptorType.UniformBuffer,
                    DescriptorCount = 1,
                    PBufferInfo = &globalsInfo,
                    PImageInfo = null,
                    PTexelBufferView = null
                },
                new WriteDescriptorSet
                {
                    SType = StructureType.WriteDescriptorSet,
                    DstSet = descriptorSet,
                    DstBinding = 1,
                    DstArrayElement = 0,
                    DescriptorType = DescriptorType.UniformBufferDynamic,
                    DescriptorCount = 1,
                    PBufferInfo = &transformsInfo,
                    PImageInfo = null,
                    PTexelBufferView = null
                },
                new WriteDescriptorSet
                {
                    SType = StructureType.WriteDescriptorSet,
                    DstSet = descriptorSet,
                    DstBinding = 2,
                    DstArrayElement = 0,
                    DescriptorType = DescriptorType.CombinedImageSampler,
                    DescriptorCount = 8,
                    PBufferInfo = null,
                    PImageInfo = textureInfos,
                    PTexelBufferView = null
                }
            };

            vk.UpdateDescriptorSets(device, writes, 0, null);
        }

        public void DestroyResources()
        {
            vk.DestroyDescriptorSetLayout(device, descriptorSetLayout, null);
            vk.DestroyPipeline(device, graphicsPipeline, null);
            vk.DestroyPipelineLayout(device, pipelineLayout, null);
        }

        public void Dispose()
        {
            DestroyResources();
        }
    }
}
