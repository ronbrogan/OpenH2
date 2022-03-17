using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenH2.Foundation;
using OpenH2.Rendering.Shaders;
using Silk.NET.Vulkan;

namespace OpenH2.Rendering.Vulkan.Internals.GraphicsPipelines
{
    internal record PipelineConfig(Shader shader,
            PipelineBinding[] bindings,
            uint descriptorSetCount,
            SampleCountFlags msaaSamples = SampleCountFlags.SampleCount8Bit,
            bool invertY = true,
            PolygonMode polyMode = PolygonMode.Fill,
            CullModeFlags cullMode = CullModeFlags.CullModeBackBit,
            bool depthTest = true);

    internal record PipelineBinding(uint location, DescriptorType type, uint count = 1, DescriptorBindingFlags flags = 0);

    internal unsafe class GeneralGraphicsPipeline : VkObject, IDisposable
    {
        protected DescriptorPool DescriptorPool;
        protected DescriptorSetLayout DescriptorSetLayout;
        protected PipelineLayout PipelineLayout;
        protected Pipeline Pipeline;

        private readonly VkDevice device;
        private readonly RenderPass renderPass;

        private readonly PipelineConfig config;
        private readonly MeshElementType primitiveType;
        private readonly bool swapchainTarget;

        public GeneralGraphicsPipeline(VkDevice device,
            RenderPass renderPass,
            Extent2D viewport,
            PipelineConfig config,
            MeshElementType primitiveType,
            bool swapchainTarget)
            : base(device.vk)
        {
            this.device = device;
            this.renderPass = renderPass;
            this.config = config;
            this.primitiveType = primitiveType;
            this.swapchainTarget = swapchainTarget;

            DescriptorPool = this.CreateDescriptorPool(config.descriptorSetCount, config.bindings);

            var (descSet, pipeline) = CreateLayouts(config.bindings);
            DescriptorSetLayout = descSet;
            PipelineLayout = pipeline;

            Pipeline = CreateGraphicsPipeline(viewport);
        }

        public bool ShouldRecreate() => this.swapchainTarget;

        protected (DescriptorSetLayout, PipelineLayout) CreateLayouts(PipelineBinding[] bindings)
        {
            var pBindings = stackalloc DescriptorSetLayoutBinding[bindings.Length];
            var bindingFlags = stackalloc DescriptorBindingFlags[bindings.Length];

            for (var i = 0; i < bindings.Length; i++)
            {
                var binding = bindings[i];
                bindingFlags[i] = binding.flags;
                pBindings[i] = new DescriptorSetLayoutBinding
                {
                    Binding = binding.location,
                    DescriptorCount = binding.count,
                    DescriptorType = binding.type,
                    StageFlags = ShaderStageFlags.ShaderStageAllGraphics
                };
            }

            var flags = new DescriptorSetLayoutBindingFlagsCreateInfo
            {
                SType = StructureType.DescriptorSetLayoutBindingFlagsCreateInfo,
                BindingCount = (uint)bindings.Length,
                PBindingFlags = bindingFlags
            };

            var descCreate = new DescriptorSetLayoutCreateInfo
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = (uint)bindings.Length,
                PBindings = pBindings,
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

        protected DescriptorPool CreateDescriptorPool(uint setCount, PipelineBinding[] bindings)
        {
            var sizes = bindings.GroupBy(g => g.type).Select(g => new DescriptorPoolSize(g.Key, (uint)g.Count() * setCount)).ToArray();

            fixed (DescriptorPoolSize* pSizes = sizes)
            {
                var createInfo = new DescriptorPoolCreateInfo
                {
                    SType = StructureType.DescriptorPoolCreateInfo,
                    PoolSizeCount = 2,
                    PPoolSizes = pSizes,
                    MaxSets = 1 * setCount
                };

                SUCCESS(vk.CreateDescriptorPool(device, in createInfo, null, out var descriptorPool));
                return descriptorPool;
            }
        }

        public void Bind(in CommandBuffer commandBuffer)
        {
            vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Graphics, Pipeline);
        }

        public void BindDescriptors(in CommandBuffer commandBuffer, in DescriptorSet descriptorSet)
        {
            vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Graphics, PipelineLayout, 0, 1, in descriptorSet, 0, null);
        }

        public void BindDescriptors(in CommandBuffer commandBuffer, in DescriptorSet descriptorSet, Span<uint> dynamicOffsets)
        {
            Debug.Assert(dynamicOffsets.Length > 0);
            var ptr = (uint*)Unsafe.AsPointer(ref dynamicOffsets[0]);
            vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Graphics, PipelineLayout, 0, 1, in descriptorSet, (uint)dynamicOffsets.Length, ptr);
        }

        public DescriptorSet CreateDescriptors(VkBuffer<GlobalUniform> globals, VkBuffer<TransformUniform> transform, VkBufferSlice uniform, (VkImage image, VkSampler sampler)[] textures, ShadowMapPass shadowPass)
        {
            var layouts = stackalloc[] { DescriptorSetLayout };
            var alloc = new DescriptorSetAllocateInfo
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = DescriptorPool,
                DescriptorSetCount = 1,
                PSetLayouts = layouts
            };

            SUCCESS(vk.AllocateDescriptorSets(device, in alloc, out var descriptorSet), "DescriptorSet allocate failed");

            var globalsInfo = new DescriptorBufferInfo(globals, 0, Vk.WholeSize);
            var uniformInfo = new DescriptorBufferInfo(uniform.Buffer, uniform.Start, uniform.Length);
            var transformsInfo = new DescriptorBufferInfo(transform, 0, device.AlignUboItem<TransformUniform>(1));

            var textureInfos = stackalloc DescriptorImageInfo[8];
            for (int i = 0; i < 8; i++)
            {
                if (textures.Length > i && textures[i] != default)
                {
                    textureInfos[i] = new DescriptorImageInfo(textures[i].sampler, textures[i].image.View, ImageLayout.ShaderReadOnlyOptimal);
                }
                else
                {
                    var fallback = device.UnboundTexture;
                    textureInfos[i] = new DescriptorImageInfo(fallback.Item2, fallback.Item1.View, ImageLayout.ShaderReadOnlyOptimal);
                }
            }


            var shadowInfo = new DescriptorImageInfo(shadowPass.Texture.sampler, shadowPass.Texture.view, ImageLayout.DepthStencilReadOnlyOptimal);

            Span<WriteDescriptorSet> writes = stackalloc WriteDescriptorSet[config.bindings.Length];

            for (int i = 0; i < config.bindings.Length; i++)
            {
                var binding = config.bindings[i];

                var write = new WriteDescriptorSet
                {
                    SType = StructureType.WriteDescriptorSet,
                    DstSet = descriptorSet,
                    DstBinding = binding.location,
                    DstArrayElement = 0,
                    DescriptorType = binding.type,
                    DescriptorCount = binding.count,
                    PBufferInfo = null,
                    PImageInfo = null,
                    PTexelBufferView = null
                };

                // TODO: better mapping for binding data
                switch(binding.location)
                {
                    case 0: write.PBufferInfo = &globalsInfo; break;
                    case 1: write.PBufferInfo = &transformsInfo; break;
                    case 2: write.PBufferInfo = &uniformInfo; break;
                    case 3: write.PImageInfo = textureInfos; break;
                    case 16: write.PImageInfo = &shadowInfo; break;
                }

                writes[i] = write;
            }

            vk.UpdateDescriptorSets(device, writes, 0, null);

            return descriptorSet;
        }

        protected Pipeline CreateGraphicsPipeline(Extent2D size)
        {
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

            var viewport = config.invertY
                ? new Viewport(0, size.Height, size.Width, -size.Height, 0, 1f)
                : new Viewport(0, 0, size.Width, size.Height, 0, 1f);

            var scissor = new Rect2D(new Offset2D(0, 0), size);

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
                PolygonMode = config.polyMode,
                LineWidth = 1,
                CullMode = config.cullMode,
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
                RasterizationSamples = config.msaaSamples,
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

            var stageIndex = 0;
            var shaderStages = stackalloc PipelineShaderStageCreateInfo[3];

            if(this.device.TryGetShader(config.shader, ShaderType.Vertex, out var vert))
                shaderStages[stageIndex++] = vert.stageInfo;

            if (this.device.TryGetShader(config.shader, ShaderType.Geometry, out var geom))
                shaderStages[stageIndex++] = geom.stageInfo;

            if (this.device.TryGetShader(config.shader, ShaderType.Fragment, out var frag))
                shaderStages[stageIndex++] = frag.stageInfo;

            var depthStencil = new PipelineDepthStencilStateCreateInfo
            {
                SType = StructureType.PipelineDepthStencilStateCreateInfo,
                DepthTestEnable = config.depthTest,
                DepthWriteEnable = true,
                DepthCompareOp = CompareOp.Less,
                DepthBoundsTestEnable = false,
                StencilTestEnable = false,
            };

            var pipelineCreate = new GraphicsPipelineCreateInfo
            {
                SType = StructureType.GraphicsPipelineCreateInfo,
                StageCount = (uint)stageIndex,
                PStages = shaderStages,
                PVertexInputState = &vertInput,
                PInputAssemblyState = &inputAssembly,
                PViewportState = &viewportState,
                PRasterizationState = &rasterizer,
                PMultisampleState = &msaa,
                PDepthStencilState = &depthStencil,
                PColorBlendState = &colorBlendState,
                //PDynamicState = &dynamicState,
                Layout = PipelineLayout,
                RenderPass = renderPass,
                Subpass = 0,
                BasePipelineHandle = default,
                BasePipelineIndex = -1,
            };

            SUCCESS(vk.CreateGraphicsPipelines(device, default, 1, &pipelineCreate, null, out var graphicsPipeline), "Pipeline create failed");
            return graphicsPipeline;
        }

        public void DestroyResources()
        {
            vk.DestroyPipeline(device, Pipeline, null);
        }

        public void CreateResources(Extent2D viewport)
        {
            Pipeline = CreateGraphicsPipeline(viewport);
        }

        protected virtual void Dispose(bool disposing)
        {
            vk.DestroyDescriptorSetLayout(device, DescriptorSetLayout, null);
            vk.DestroyPipelineLayout(device, PipelineLayout, null);

            vk.DestroyPipeline(device, Pipeline, null);

            vk.DestroyDescriptorPool(this.device, DescriptorPool, null);
        }

        ~GeneralGraphicsPipeline()
        {
            throw new Exception("Dispose not being called on this resource is an error!");
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
