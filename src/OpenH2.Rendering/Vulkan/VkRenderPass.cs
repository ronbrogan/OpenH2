﻿using Silk.NET.Vulkan;
using System;

namespace OpenH2.Rendering.Vulkan
{
    internal unsafe sealed class VkRenderPass : VkObject, IDisposable
    {
        private readonly VkDevice device;
        private readonly VkSwapchain swapchain;

        private RenderPass renderPass;

        public VkRenderPass(VkDevice device, VkSwapchain swapchain) : base(device.vk)
        {
            this.device = device;
            this.swapchain = swapchain;
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

            // TODO: derive depth format from common place
            var depthAttach = new AttachmentDescription
            {
                Format = Format.D32Sfloat,
                Samples = SampleCountFlags.SampleCount1Bit,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.DontCare,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.DepthStencilAttachmentOptimal
            };

            var colorAttachRef = new AttachmentReference(0, ImageLayout.ColorAttachmentOptimal);
            var depthAttachRef = new AttachmentReference(1, ImageLayout.DepthStencilAttachmentOptimal);

            var subpass = new SubpassDescription
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachmentCount = 1,
                PColorAttachments = &colorAttachRef,
                PDepthStencilAttachment = &depthAttachRef,
            };

            var dependency = new SubpassDependency
            {
                SrcSubpass = Vk.SubpassExternal,
                DstSubpass = 0,
                SrcStageMask = PipelineStageFlags.PipelineStageColorAttachmentOutputBit | PipelineStageFlags.PipelineStageEarlyFragmentTestsBit,
                SrcAccessMask = 0,
                DstStageMask = PipelineStageFlags.PipelineStageColorAttachmentOutputBit | PipelineStageFlags.PipelineStageEarlyFragmentTestsBit,
                DstAccessMask = AccessFlags.AccessColorAttachmentWriteBit | AccessFlags.AccessDepthStencilAttachmentWriteBit,
            };

            var attachments = stackalloc[] { colorAttach, depthAttach };
            var renderPassCreate = new RenderPassCreateInfo
            {
                SType = StructureType.RenderPassCreateInfo,
                AttachmentCount = 2,
                PAttachments = attachments,
                SubpassCount = 1,
                PSubpasses = &subpass,
                DependencyCount = 1,
                PDependencies = &dependency
            };

            SUCCESS(vk.CreateRenderPass(device, in renderPassCreate, null, out renderPass), "Render pass create failed");

            InitializeFramebuffers();
        }

        public void InitializeFramebuffers()
        {
            swapchain.InitializeFramebuffers(renderPass);
        }

        public void Begin(in CommandBuffer commandBuffer, uint imageIndex)
        {
            var clearColors = stackalloc[] {
                new ClearValue(new ClearColorValue(0f, 0f, 0f, 1f)),
                new ClearValue(depthStencil: new ClearDepthStencilValue(1.0f, 0))
            };
            var renderBegin = new RenderPassBeginInfo
            {
                SType = StructureType.RenderPassBeginInfo,
                RenderPass = renderPass,
                Framebuffer = swapchain.Framebuffers[imageIndex],
                RenderArea = new Rect2D(new Offset2D(0, 0), swapchain.Extent),
                ClearValueCount = 2,
                PClearValues = clearColors
            };

            vk.CmdBeginRenderPass(commandBuffer, in renderBegin, SubpassContents.Inline);
        }

        public static implicit operator RenderPass(VkRenderPass @this) => @this.renderPass;

        public void Dispose()
        {
            vk.DestroyRenderPass(device, renderPass, null);
        }
    }
}
