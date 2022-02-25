using OpenH2.Core.Tags;
using OpenH2.Core.Extensions;
using OpenH2.Foundation;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Core.Contexts;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace OpenH2.Rendering.Vulkan
{
    public sealed unsafe class VulkanGraphicsAdapter : VkObject, IGraphicsAdapter, IDisposable
    {
        const int MaxFramesInFlight = 2;

        private VulkanHost vulkanHost;
        private IVkSurface vkSurface;
        private VkInstance instance;
        private VkDevice device;
        private VkSwapchain swapchain;
        private PipelineLayout pipelineLayout;
        private RenderPass renderPass;
        private Pipeline graphicsPipeline;
        private CommandPool commandPool;

        // TODO: need multiple of these to support multiple in-flight frames

        private CommandBuffer commandBuffer;

        private Semaphore imageAvailableSemaphore;
        private Semaphore renderFinishedSemaphore;
        private Fence inFlightFence;

        public VulkanGraphicsAdapter(VulkanHost vulkanHost, IVkSurface vkSurface) : base(vulkanHost.vk)
        {
            this.vkSurface = vkSurface;
            this.vulkanHost = vulkanHost;

            this.instance = new VkInstance(vulkanHost);
            this.device = instance.CreateDevice();
            this.swapchain = device.CreateSwapchain();

            // =======================
            //  start pipeline setup
            // =======================

            using var vertShader = new VkShader(device, "VulkanTest", ShaderType.Vertex);
            using var fragShader = new VkShader(device, "VulkanTest", ShaderType.Fragment);

            var shaderStages = stackalloc PipelineShaderStageCreateInfo[] { vertShader.stageInfo, fragShader.stageInfo };

            var vertInput = new PipelineVertexInputStateCreateInfo
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexBindingDescriptionCount = 0,
                VertexAttributeDescriptionCount = 0
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
                FrontFace = FrontFace.Clockwise,
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

            var layoutCreate = new PipelineLayoutCreateInfo
            {
                SType = StructureType.PipelineLayoutCreateInfo,
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

            GC.Collect(2, GCCollectionMode.Forced, true);

            SUCCESS(vk.CreateGraphicsPipelines(device, default, 1, &pipelineCreate, null, out graphicsPipeline), "Pipeline create failed");

            // =======================
            // framebuffer setup
            // =======================

            var attachments = stackalloc ImageView[1];
            for (int i = 0; i < swapchain.ImageViews.Length; i++)
            {
                attachments[0] = swapchain.ImageViews[i];

                var framebufferCreate = new FramebufferCreateInfo
                {
                    SType = StructureType.FramebufferCreateInfo,
                    RenderPass = renderPass,
                    AttachmentCount = 1,
                    PAttachments = attachments,
                    Width = swapchain.Extent.Width,
                    Height = swapchain.Extent.Height,
                    Layers = 1
                };

                vk.CreateFramebuffer(device, in framebufferCreate, null, out swapchain.Framebuffers[i]);
            }

            // =======================
            // commandbuffer setup
            // =======================

            var poolCreate = new CommandPoolCreateInfo
            {
                SType = StructureType.CommandPoolCreateInfo,
                Flags = CommandPoolCreateFlags.CommandPoolCreateResetCommandBufferBit,
                QueueFamilyIndex = device.GraphicsQueueFamily.Value
            };

            SUCCESS(vk.CreateCommandPool(device, in poolCreate, null, out commandPool), "CommandPool create failed");

            var commandBufAlloc = new CommandBufferAllocateInfo
            {
                SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = commandPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = 1
            };

            SUCCESS(vk.AllocateCommandBuffers(device, in commandBufAlloc, out commandBuffer), "Command buffer alloc failed");

            var semInfo = new SemaphoreCreateInfo(StructureType.SemaphoreCreateInfo);
            SUCCESS(vk.CreateSemaphore(device, &semInfo, null, out imageAvailableSemaphore));
            SUCCESS(vk.CreateSemaphore(device, &semInfo, null, out renderFinishedSemaphore));

            var fenceInfo = new FenceCreateInfo(StructureType.FenceCreateInfo, flags: FenceCreateFlags.FenceCreateSignaledBit);
            SUCCESS(vk.CreateFence(device, &fenceInfo, null, out inFlightFence));
        }


        public void AddLight(PointLight light)
        {
        }

        private uint imageIndex = 0;
        public void BeginFrame(GlobalUniform matricies)
        {
            vk.WaitForFences(device, 1, in inFlightFence, true, ulong.MaxValue);
            vk.ResetFences(device, 1, in inFlightFence);

            swapchain.AcquireNextImage(imageAvailableSemaphore, default, ref imageIndex);

            vk.ResetCommandBuffer(commandBuffer, (CommandBufferResetFlags)0);

            var bufBegin = new CommandBufferBeginInfo
            {
                SType = StructureType.CommandBufferBeginInfo
            };

            SUCCESS(vk.BeginCommandBuffer(commandBuffer, in bufBegin), "Unable to begin writing to command buffer");

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

            vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Graphics, graphicsPipeline);

            vk.CmdDraw(commandBuffer, 3, 1, 0, 0);
        }

        public void DrawMeshes(DrawCommand[] commands)
        {
            
        }

        public void EndFrame()
        {
            vk.CmdEndRenderPass(commandBuffer);

            SUCCESS(vk.EndCommandBuffer(commandBuffer), "Failed to record command buffer");

            var waitSems = stackalloc Semaphore[] { imageAvailableSemaphore };
            var waitStages = stackalloc PipelineStageFlags[] { PipelineStageFlags.PipelineStageColorAttachmentOutputBit };

            var signalSems = stackalloc Semaphore[] { renderFinishedSemaphore };


            var buf = commandBuffer;
            var submitInfo = new SubmitInfo
            {
                SType = StructureType.SubmitInfo,
                WaitSemaphoreCount = 1,
                PWaitSemaphores = waitSems,
                PWaitDstStageMask = waitStages,
                CommandBufferCount = 1,
                PCommandBuffers = &buf,
                SignalSemaphoreCount = 1,
                PSignalSemaphores = signalSems
            };

            SUCCESS(vk.QueueSubmit(device.GraphicsQueue, 1, in submitInfo, inFlightFence));

            var chains = stackalloc SwapchainKHR[] { swapchain };
            var imgIndex = imageIndex;
            var presentInfo = new PresentInfoKHR
            {
                SType = StructureType.PresentInfoKhr,
                WaitSemaphoreCount = 1,
                PWaitSemaphores = signalSems,
                SwapchainCount = 1,
                PSwapchains = chains,
                PImageIndices = &imgIndex,
                PResults = null
            };

            swapchain.QueuePresent(in presentInfo);
        }

        public void SetSunLight(Vector3 sunDirection)
        {
        }

        public int UploadModel(Model<BitmapTag> model, out DrawCommand[] meshCommands)
        {
            meshCommands = new DrawCommand[0];
            return 0;
        }

        public void UseShader(Shader shader)
        {
        }

        public void UseTransform(Matrix4x4 transform)
        {
        }

        public void Dispose()
        {
            vk.DeviceWaitIdle(device);

            vk.DestroySemaphore(device, imageAvailableSemaphore, null);
            vk.DestroySemaphore(device, renderFinishedSemaphore, null);
            vk.DestroyFence(device, inFlightFence, null);

            vk.DestroyCommandPool(device, commandPool, null);

            // destroy swapchain
            this.swapchain.Dispose();

            // Destroy pipeline

            vk.DestroyPipeline(device, graphicsPipeline, null);
            vk.DestroyPipelineLayout(device, pipelineLayout, null);
            vk.DestroyRenderPass(device, renderPass, null);

            // destroy device

            this.device.Dispose();

            // destroy instance

            this.instance.Dispose();
        }
    }
}
