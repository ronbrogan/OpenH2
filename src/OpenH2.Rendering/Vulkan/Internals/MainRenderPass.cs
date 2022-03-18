using Silk.NET.Vulkan;
using System;

namespace OpenH2.Rendering.Vulkan.Internals
{

    internal unsafe class MainRenderPass : VkObject, IDisposable
    {
        protected readonly VkDevice device;
        protected readonly VkSwapchain swapchain;

        private RenderPass renderPass;

        public MainRenderPass(VkDevice device, VkSwapchain swapchain) : base(device)
        {
            this.device = device;
            this.swapchain = swapchain;

            this.renderPass = CreateResources();

            InitializeFramebuffers();
        }

        public virtual RenderPass CreateResources()
        {
            var colorAttach = new AttachmentDescription
            {
                Format = device.SurfaceFormat.Format,
                Samples = SampleCountFlags.SampleCount8Bit,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.ColorAttachmentOptimal
            };

            // TODO: derive depth format from common place
            var depthAttach = new AttachmentDescription
            {
                Format = Format.D32Sfloat,
                Samples = SampleCountFlags.SampleCount8Bit,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.DontCare,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.DepthStencilAttachmentOptimal
            };

            var colorAttachmentResolve = new AttachmentDescription
            {
                Format = device.SurfaceFormat.Format,
                Samples = SampleCountFlags.SampleCount1Bit,
                LoadOp = AttachmentLoadOp.DontCare,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.PresentSrcKhr,
            };

            var colorAttachRef = new AttachmentReference(0, ImageLayout.ColorAttachmentOptimal);
            var depthAttachRef = new AttachmentReference(1, ImageLayout.DepthStencilAttachmentOptimal);
            var resolveRef = new AttachmentReference(2, ImageLayout.ColorAttachmentOptimal);

            var subpass = new SubpassDescription
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachmentCount = 1,
                PColorAttachments = &colorAttachRef,
                PDepthStencilAttachment = &depthAttachRef,
                PResolveAttachments = &resolveRef,
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

            var attachments = stackalloc[] { colorAttach, depthAttach, colorAttachmentResolve };
            var renderPassCreate = new RenderPassCreateInfo
            {
                SType = StructureType.RenderPassCreateInfo,
                AttachmentCount = 3,
                PAttachments = attachments,
                SubpassCount = 1,
                PSubpasses = &subpass,
                DependencyCount = 1,
                PDependencies = &dependency
            };

            SUCCESS(vk.CreateRenderPass(device, in renderPassCreate, null, out var renderPass), "Render pass create failed");
            return renderPass;
        }

        public void InitializeFramebuffers()
        {
            swapchain.InitializeFramebuffers(renderPass);
        }

        public virtual void Begin(in CommandBuffer commandBuffer, uint imageIndex)
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

        public static implicit operator RenderPass(MainRenderPass @this) => @this.renderPass;

        public void Dispose()
        {
            vk.DestroyRenderPass(device, renderPass, null);
        }
    }
}
