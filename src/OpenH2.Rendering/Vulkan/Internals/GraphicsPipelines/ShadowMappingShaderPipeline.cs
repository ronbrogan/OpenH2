using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using OpenH2.Foundation;
using OpenH2.Rendering.Shaders;
using Silk.NET.Vulkan;

namespace OpenH2.Rendering.Vulkan.Internals.GraphicsPipelines
{
    internal class ShadowMappingTriListPipeline : ShadowMappingShaderPipeline<ShadowMappingTriListPipeline>
    {
        public ShadowMappingTriListPipeline(VkDevice device, ShadowMapPass shadowPass)
            : base(device, shadowPass, MeshElementType.TriangleList)
        {
        }
    }

    internal class ShadowMappingTriStripPipeline : ShadowMappingShaderPipeline<ShadowMappingTriStripPipeline>
    {
        public ShadowMappingTriStripPipeline(VkDevice device, ShadowMapPass shadowPass)
            : base(device, shadowPass, MeshElementType.TriangleStrip)
        {
        }
    }

    internal unsafe class ShadowMappingShaderPipeline<T> : BaseGraphicsPipeline
    {
        private static long Instances = 0;
        private long InstanceId = -1;

        protected static DescriptorPool DescriptorPool;
        protected static VkShader VertexShader;
        protected static VkShader? GeometryShader;
        protected static VkShader FragmentShader;
        protected static DescriptorSetLayout DescriptorSetLayout;
        protected static PipelineLayout PipelineLayout;
        protected static Pipeline Pipeline;

        protected DescriptorSet DescriptorSet;
        private readonly ShadowMapPass shadowPass;
        protected readonly MeshElementType primitiveType;

        private bool disposedValue;

        public ShadowMappingShaderPipeline(VkDevice device, ShadowMapPass shadowPass, MeshElementType primitiveType) 
            : base(device)
        {
            this.shadowPass = shadowPass;
            this.primitiveType = primitiveType;

            this.InstanceId = Interlocked.Increment(ref Instances);

            if (this.InstanceId == 1)
            {
                VertexShader = VkShader.CreateIfPresent(device, Shader.ShadowMapping, ShaderType.Vertex);
                GeometryShader = VkShader.CreateIfPresent(device, Shader.ShadowMapping, ShaderType.Geometry);
                FragmentShader = VkShader.CreateIfPresent(device, Shader.ShadowMapping, ShaderType.Fragment);

                DescriptorPool = this.CreateDescriptorPool();

                var (descSet, pipeline) = CreateLayouts();
                DescriptorSetLayout = descSet;
                PipelineLayout = pipeline;

                Pipeline = CreateGraphicsPipeline();

                BaseGraphicsPipeline.RegisterOwningInstance(this);
            }
        }

        protected Pipeline CreateGraphicsPipeline()
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

            var extent = new Extent2D(ShadowMapPass.MapSize, ShadowMapPass.MapSize);

            var viewport = new Viewport(0, 0, extent.Width, extent.Height, 0, 1f);
            var scissor = new Rect2D(new Offset2D(0, 0), extent);

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
                BlendEnable = false,
            };

            var colorBlendState = new PipelineColorBlendStateCreateInfo()
            {
                SType = StructureType.PipelineColorBlendStateCreateInfo,
                AttachmentCount = 1,
                PAttachments = &colorBlend
            };

            var stageIndex = 0;
            var stageCount = GeometryShader == null ? 2 : 3;
            var shaderStages = stackalloc PipelineShaderStageCreateInfo[stageCount];

            shaderStages[stageIndex++] = VertexShader.stageInfo;
            if (GeometryShader != null)
                shaderStages[stageIndex++] = GeometryShader.stageInfo;
            shaderStages[stageIndex++] = FragmentShader.stageInfo;

            var depthStencil = new PipelineDepthStencilStateCreateInfo
            {
                SType = StructureType.PipelineDepthStencilStateCreateInfo,
                DepthTestEnable = true,
                DepthWriteEnable = true,
                DepthCompareOp = CompareOp.LessOrEqual,
                DepthBoundsTestEnable = false,
                StencilTestEnable = false,
                Back = new StencilOpState
                {
                    CompareOp = CompareOp.Always
                }
            };

            var pipelineCreate = new GraphicsPipelineCreateInfo
            {
                SType = StructureType.GraphicsPipelineCreateInfo,
                StageCount = (uint)stageCount,
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
                RenderPass = this.shadowPass,
                Subpass = 0,
                BasePipelineHandle = default,
                BasePipelineIndex = -1,
            };

            SUCCESS(vk.CreateGraphicsPipelines(device, default, 1, &pipelineCreate, null, out var graphicsPipeline), "Pipeline create failed");
            return graphicsPipeline;
        }

        protected (DescriptorSetLayout, PipelineLayout) CreateLayouts()
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

            var bindings = stackalloc[] { globalsBinding, transformBinding };

            var descCreate = new DescriptorSetLayoutCreateInfo
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = 2,
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


        public override void CreateDescriptors(VkBuffer<GlobalUniform> globals, VkBuffer<TransformUniform> transform, VkBufferSlice uniform, (VkImage image, VkSampler sampler)[] textures)
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
            var transformsInfo = new DescriptorBufferInfo(transform, 0, device.AlignUboItem<TransformUniform>(1));


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
                }
            };

            vk.UpdateDescriptorSets(device, writes, 0, null);

            this.DescriptorSet = descriptorSet;
        }

        protected DescriptorPool CreateDescriptorPool()
        {
            var count = 4096u;

            var uboPoolSize = new DescriptorPoolSize(DescriptorType.UniformBuffer, 4 * count);
            var sizes = stackalloc DescriptorPoolSize[] { uboPoolSize };
            var createInfo =  new DescriptorPoolCreateInfo
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = 1,
                PPoolSizes = sizes,
                MaxSets = 1 * count
            };

            SUCCESS(vk.CreateDescriptorPool(device, in createInfo, null, out var descriptorPool));
            return descriptorPool;
        }

        protected override void DestroyResources()
        {
            if (this.InstanceId != 1)
                throw new Exception("This instance does not own the per-Type resources, resource management is not allowed");

            vk.DestroyPipeline(device, Pipeline, null);
        }

        protected override void CreateResources()
        {
            if (this.InstanceId != 1)
                throw new Exception("This instance does not own the per-Type resources, resource management is not allowed");

            Pipeline = CreateGraphicsPipeline();
        }


        public override void Bind(in CommandBuffer commandBuffer)
        {
            vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Graphics, Pipeline);
        }

        public override void BindDescriptors(in CommandBuffer commandBuffer)
        {
            vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Graphics, PipelineLayout, 0, 1, in DescriptorSet, 0, null);
        }

        public override void BindDescriptors(in CommandBuffer commandBuffer, Span<uint> dynamicOffsets)
        {
            Debug.Assert(dynamicOffsets.Length > 0);
            var ptr = (uint*)Unsafe.AsPointer(ref dynamicOffsets[0]);
            vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Graphics, PipelineLayout, 0, 1, in DescriptorSet, (uint)dynamicOffsets.Length, ptr);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                var remain = Interlocked.Decrement(ref Instances);

                // Per-pipeline disposal
                if (remain == 0)
                {
                    VertexShader?.Dispose();
                    FragmentShader?.Dispose();

                    vk.DestroyDescriptorSetLayout(device, DescriptorSetLayout, null);
                    vk.DestroyPipelineLayout(device, PipelineLayout, null);

                    vk.DestroyPipeline(device, Pipeline, null);

                    vk.DestroyDescriptorPool(this.device, DescriptorPool, null);

                    // TODO: reconcile out of order disposal and the concept owning instances
                    //BaseGraphicsPipeline.UnregisterOwningInstance(this);
                }

                disposedValue = true;
            }
        }

        ~ShadowMappingShaderPipeline()
        {
            Debug.Fail("Dispose not being called on this resource is an error!");
            throw new Exception("Dispose not being called on this resource is an error!");
        }

        public override sealed void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
