using OpenH2.Foundation;
using OpenH2.Rendering.Shaders;
using OpenH2.Rendering.Shaders.Generic;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace OpenH2.Rendering.Vulkan.Internals.GraphicsPipelines
{
    internal abstract class BaseGraphicsPipeline : VkObject, IDisposable
    {
        private static List<BaseGraphicsPipeline> owningInstances = new();
        protected readonly VkDevice device;

        public BaseGraphicsPipeline(VkDevice device) : base(device.vk)
        {
            this.device = device;
        }

        protected abstract void DestroyResources();
        protected abstract void CreateResources();

        public static void DestroyAllPipelineResources()
        {
            foreach (var inst in owningInstances)
                inst.DestroyResources();
        }

        public static void CreateAllPipelineResources()
        {
            foreach (var inst in owningInstances)
                inst.CreateResources();
        }

        protected static void RegisterOwningInstance(BaseGraphicsPipeline baseGraphicsPipeline)
        {
            owningInstances.Add(baseGraphicsPipeline);
        }

        protected static void UnregisterOwningInstance(BaseGraphicsPipeline baseGraphicsPipeline)
        {
            owningInstances.Remove(baseGraphicsPipeline);
        }

        public abstract void Bind(in CommandBuffer commandBuffer);
        public abstract void BindDescriptors(in CommandBuffer commandBuffer);
        public abstract void BindDescriptors(in CommandBuffer commandBuffer, Span<uint> dynamicOffsets);
        public abstract void CreateDescriptors(VkBuffer<GlobalUniform> globals, VkBuffer<TransformUniform> transform, VkBufferSlice uniform, (VkImage image, VkSampler sampler)[] textures);
        public abstract void Dispose();
    }

    internal unsafe abstract class BaseGraphicsPipeline<T> : BaseGraphicsPipeline, IDisposable
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

        protected readonly VkSwapchain swapchain;
        protected readonly VkRenderPass renderPass;
        protected readonly ShadowMapPass shadowPass;
        protected readonly MeshElementType primitiveType;
        protected readonly bool depthTestEnable;
        protected readonly PolygonMode polyMode;
        protected readonly CullModeFlags cullMode;
        private bool disposedValue;

        protected BaseGraphicsPipeline(VkDevice device, VkSwapchain swapchain, VkRenderPass renderPass, ShadowMapPass shadowPass, Shader shader, 
            MeshElementType primitiveType, bool depthTestEnable = true, PolygonMode polyMode = PolygonMode.Fill, CullModeFlags cullMode = CullModeFlags.CullModeBackBit)
            : base(device)
        {
            this.swapchain = swapchain;
            this.renderPass = renderPass;
            this.shadowPass = shadowPass;
            this.primitiveType = primitiveType;
            this.depthTestEnable = depthTestEnable;
            this.polyMode = polyMode;
            this.cullMode = cullMode;

            this.InstanceId = Interlocked.Increment(ref Instances);

            if (this.InstanceId == 1)
            {
                VertexShader = VkShader.CreateIfPresent(device, shader, ShaderType.Vertex);
                GeometryShader = VkShader.CreateIfPresent(device, shader, ShaderType.Geometry);
                FragmentShader = VkShader.CreateIfPresent(device, shader, ShaderType.Fragment);

                DescriptorPool = this.CreateDescriptorPool();

                var (descSet, pipeline) = CreateLayouts();
                DescriptorSetLayout = descSet;
                PipelineLayout = pipeline;

                Pipeline = CreateGraphicsPipeline();
                
                BaseGraphicsPipeline.RegisterOwningInstance(this);
            }
        }

        protected abstract DescriptorPool CreateDescriptorPool();

        protected abstract (DescriptorSetLayout, PipelineLayout) CreateLayouts();

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
                    DescriptorType = DescriptorType.UniformBuffer,
                    DescriptorCount = 1,
                    PBufferInfo = &uniformInfo,
                    PImageInfo = null,
                    PTexelBufferView = null
                },
                new WriteDescriptorSet
                {
                    SType = StructureType.WriteDescriptorSet,
                    DstSet = descriptorSet,
                    DstBinding = 3,
                    DstArrayElement = 0,
                    DescriptorType = DescriptorType.CombinedImageSampler,
                    DescriptorCount = 8,
                    PBufferInfo = null,
                    PImageInfo = textureInfos,
                    PTexelBufferView = null
                },
                new WriteDescriptorSet
                {
                    SType = StructureType.WriteDescriptorSet,
                    DstSet = descriptorSet,
                    DstBinding = 16,
                    DstArrayElement = 0,
                    DescriptorType = DescriptorType.CombinedImageSampler,
                    DescriptorCount = 1,
                    PBufferInfo = null,
                    PImageInfo = &shadowInfo,
                    PTexelBufferView = null
                }
            };

            vk.UpdateDescriptorSets(device, writes, 0, null);

            this.DescriptorSet = descriptorSet;
        }

        protected virtual Pipeline CreateGraphicsPipeline()
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
                PolygonMode = this.polyMode,
                LineWidth = 1,
                CullMode = this.cullMode,
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

            var dynamicStates = stackalloc[] { DynamicState.Viewport, DynamicState.LineWidth };

            var dynamicState = new PipelineDynamicStateCreateInfo
            {
                SType = StructureType.PipelineDynamicStateCreateInfo,
                DynamicStateCount = 2,
                PDynamicStates = dynamicStates,
            };

            var stageIndex = 0;
            var stageCount = GeometryShader == null ? 2 : 3;
            var shaderStages = stackalloc PipelineShaderStageCreateInfo[stageCount];

            shaderStages[stageIndex++] = VertexShader.stageInfo;
            if(GeometryShader != null)
                shaderStages[stageIndex++] = GeometryShader.stageInfo;
            shaderStages[stageIndex++] = FragmentShader.stageInfo;
            


            var depthStencil = new PipelineDepthStencilStateCreateInfo
            {
                SType = StructureType.PipelineDepthStencilStateCreateInfo,
                DepthTestEnable = depthTestEnable,
                DepthWriteEnable = true,
                DepthCompareOp = CompareOp.Less,
                DepthBoundsTestEnable = false,
                StencilTestEnable = false,
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
                RenderPass = renderPass,
                Subpass = 0,
                BasePipelineHandle = default,
                BasePipelineIndex = -1,
            };

            SUCCESS(vk.CreateGraphicsPipelines(device, default, 1, &pipelineCreate, null, out var graphicsPipeline), "Pipeline create failed");
            return graphicsPipeline;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                var remain = Interlocked.Decrement(ref Instances);

                // Per-pipeline disposal
                if(remain == 0)
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

        ~BaseGraphicsPipeline()
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
