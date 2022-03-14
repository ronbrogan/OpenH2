using System;
using Silk.NET.Vulkan;

namespace OpenH2.Rendering.Vulkan.Internals
{
    internal unsafe class ShadowMapPass : VkObject, IDisposable
    {
        public const int MapSize = 4096;
        public const int CascadeCount = 4;

        protected readonly VkDevice device;

        private RenderPass renderPass;
        private Extent3D extent = new Extent3D(MapSize, MapSize, CascadeCount);
        public Framebuffer Framebuffer { get; private set; }
        public VkImageArray Image { get; private set; }
        public (VkSampler sampler, ImageView view) Texture { get; private set; }

        public ShadowMapPass(VkDevice device) : base(device.vk)
        {
            this.device = device;

            this.renderPass = CreatePass();
            CreateFramebuffer();
        }

        public RenderPass CreatePass()
        {
            // TODO: derive depth format from common place
            var depthAttach = new AttachmentDescription
            {
                Format = Format.D32Sfloat,
                Samples = SampleCountFlags.SampleCount1Bit,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.DepthStencilReadOnlyOptimal
            };

            var depthAttachRef = new AttachmentReference(0, ImageLayout.DepthStencilAttachmentOptimal);

            var subpass = new SubpassDescription
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachmentCount = 0,
                PDepthStencilAttachment = &depthAttachRef,
            };

            var dependencies = stackalloc[]
            {
                new SubpassDependency
                {
                    SrcSubpass = Vk.SubpassExternal,
                    DstSubpass = 0,
                    SrcStageMask = PipelineStageFlags.PipelineStageFragmentShaderBit,
                    SrcAccessMask = AccessFlags.AccessShaderReadBit,
                    DstStageMask = PipelineStageFlags.PipelineStageEarlyFragmentTestsBit,
                    DstAccessMask = AccessFlags.AccessDepthStencilAttachmentWriteBit,
                    DependencyFlags = DependencyFlags.DependencyByRegionBit
                },
                new SubpassDependency
                {
                    SrcSubpass = 0,
                    DstSubpass = Vk.SubpassExternal,
                    SrcStageMask = PipelineStageFlags.PipelineStageLateFragmentTestsBit,
                    SrcAccessMask = AccessFlags.AccessDepthStencilAttachmentWriteBit,
                    DstStageMask = PipelineStageFlags.PipelineStageFragmentShaderBit,
                    DstAccessMask = AccessFlags.AccessShaderReadBit,
                    DependencyFlags = DependencyFlags.DependencyByRegionBit
                }
            };

            var renderPassCreate = new RenderPassCreateInfo
            {
                SType = StructureType.RenderPassCreateInfo,
                AttachmentCount = 1,
                PAttachments = &depthAttach,
                SubpassCount = 1,
                PSubpasses = &subpass,
                DependencyCount = 2,
                PDependencies = dependencies
            };

            SUCCESS(vk.CreateRenderPass(device, in renderPassCreate, null, out var renderPass), "Render pass create failed");
            return renderPass;
        }

        public void CreateFramebuffer()
        {
            // TODO find a supported depth format instead of hardcoding D32Sfloat
            var depthImage = new VkImageArray(device, extent, Format.D32Sfloat, ImageUsageFlags.ImageUsageDepthStencilAttachmentBit | ImageUsageFlags.ImageUsageSampledBit, ImageAspectFlags.ImageAspectDepthBit);
            var depthSampler = depthImage.CreateSampler();

            var attachments = stackalloc ImageView[]
            {
                depthImage.View
            };

            var framebufferCreate = new FramebufferCreateInfo
            {
                SType = StructureType.FramebufferCreateInfo,
                RenderPass = renderPass,
                AttachmentCount = 1,
                PAttachments = attachments,
                Width = extent.Width,
                Height = extent.Height,
                Layers = extent.Depth
            };

            SUCCESS(vk.CreateFramebuffer(device, in framebufferCreate, null, out var fb));

            this.Framebuffer = fb;
            this.Image = depthImage;
            this.Texture = (depthSampler, depthImage.View);
        }

        public virtual void Begin(in CommandBuffer commandBuffer)
        {
            var clearColors = stackalloc[] {
                new ClearValue(depthStencil: new ClearDepthStencilValue(1.0f, 0))
            };
            var renderBegin = new RenderPassBeginInfo
            {
                SType = StructureType.RenderPassBeginInfo,
                RenderPass = renderPass,
                Framebuffer = Framebuffer,
                RenderArea = new Rect2D(new Offset2D(0, 0), new Extent2D(extent.Width, extent.Height)),
                ClearValueCount = 1,
                PClearValues = clearColors
            };

            vk.CmdBeginRenderPass(commandBuffer, in renderBegin, SubpassContents.Inline);
        }

        public static implicit operator RenderPass(ShadowMapPass @this) => @this.renderPass;

        public void Dispose()
        {
            this.Texture.sampler?.Dispose();
            this.Image?.Dispose();
            vk.DestroyRenderPass(device, renderPass, null);
        }
    }
}
