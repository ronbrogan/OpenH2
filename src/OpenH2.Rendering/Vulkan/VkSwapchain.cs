using OpenH2.Core.Extensions;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenH2.Rendering.Vulkan
{
    internal unsafe sealed class VkSwapchain : VkObject, IDisposable
    {
        private VkDevice device;
        private VkInstance instance;
        private KhrSwapchain khrSwapchainExt;


        private SwapchainKHR swapchain;
        private Image[] swapchainImages;
        private ImageView[] swapchainImageviews;
        private Framebuffer[] swapchainFramebuffers;
        private (Format format, Extent2D extent) swapchainParams;
        private readonly VulkanHost vulkanHost;


        public Extent2D Extent => swapchainParams.extent;
        public Format Format => swapchainParams.format;

        public Image[] Images => swapchainImages;
        public ImageView[] ImageViews => swapchainImageviews;
        public Framebuffer[] Framebuffers => swapchainFramebuffers;

        public VkSwapchain(VulkanHost host, VkInstance instance, VkDevice device) : base(host.vk)
        {
            this.instance = instance;
            this.device = device;
            this.vulkanHost = host;

            var caps = device.SurfaceCapabilities;
            
            SUCCESS(vk.TryGetDeviceExtension(instance, device, out khrSwapchainExt), "Couldn't get swapchain extension");
            

            // One more than min count, unless we're bound by max image count
            var imgCount = caps.MinImageCount + 1;
            if (caps.MaxImageCount > 0 && imgCount > caps.MaxImageCount)
                imgCount = caps.MaxImageCount;

            // Prepare to create swapchain with what we've found
            var swapchainCreate = new SwapchainCreateInfoKHR
            {
                SType = StructureType.SwapchainCreateInfoKhr,
                Surface = device.Surface,
                MinImageCount = imgCount,
                ImageFormat = device.SurfaceFormat.Format,
                ImageColorSpace = device.SurfaceFormat.ColorSpace,
                ImageExtent = device.Extent,
                ImageArrayLayers = 1,
                ImageUsage = ImageUsageFlags.ImageUsageColorAttachmentBit,
                PreTransform = caps.CurrentTransform,
                CompositeAlpha = CompositeAlphaFlagsKHR.CompositeAlphaOpaqueBitKhr,
                PresentMode = device.PresentMode,
                Clipped = true,
                OldSwapchain = default,
                ImageSharingMode = SharingMode.Exclusive,
            };

            // If we're not using the same queue, setup concurrent images
            if (device.GraphicsQueueFamily != device.PresentQueueFamily)
            {
                var queueFamilyIndices = stackalloc uint[] { device.GraphicsQueueFamily.Value, device.PresentQueueFamily.Value };
                swapchainCreate.ImageSharingMode = SharingMode.Concurrent;
                swapchainCreate.QueueFamilyIndexCount = 2;
                swapchainCreate.PQueueFamilyIndices = queueFamilyIndices;
            }

            SUCCESS(khrSwapchainExt.CreateSwapchain(device, in swapchainCreate, null, out swapchain), "failed to create swapchain");
            khrSwapchainExt.GetSwapchainImages(device, swapchain, ref imgCount, null);
            swapchainImages = new Image[imgCount];
            swapchainImageviews = new ImageView[imgCount];
            swapchainFramebuffers = new Framebuffer[imgCount];

            khrSwapchainExt.GetSwapchainImages(device, swapchain, ref imgCount, out swapchainImages[0]);
            swapchainParams = (device.SurfaceFormat.Format, device.Extent);

            for (int i = 0; i < imgCount; i++)
            {
                var imgViewCreate = new ImageViewCreateInfo
                {
                    SType = StructureType.ImageViewCreateInfo,
                    Image = swapchainImages[i],
                    ViewType = ImageViewType.ImageViewType2D,
                    Format = device.SurfaceFormat.Format,
                    Components = new ComponentMapping(ComponentSwizzle.Identity, ComponentSwizzle.Identity, ComponentSwizzle.Identity, ComponentSwizzle.Identity),
                    SubresourceRange = new ImageSubresourceRange
                    {
                        AspectMask = ImageAspectFlags.ImageAspectColorBit,
                        BaseMipLevel = 0,
                        LevelCount = 1,
                        BaseArrayLayer = 0,
                        LayerCount = 1
                    }
                };

                SUCCESS(vk.CreateImageView(device, in imgViewCreate, null, out swapchainImageviews[i]), "Image view create failed");
            }
        }

        public Result AcquireNextImage(Semaphore semaphore, Fence fence, ref uint index)
        {
            return khrSwapchainExt.AcquireNextImage(device, swapchain, ulong.MaxValue, semaphore, fence, ref index);
        }

        public Result QueuePresent(in PresentInfoKHR presentInfo)
        {
            return khrSwapchainExt.QueuePresent(device.PresentQueue, in presentInfo);
        }

        public void InitializeFramebuffers(in RenderPass renderPass)
        {
            var attachments = stackalloc ImageView[1];
            for (int i = 0; i < swapchainImageviews.Length; i++)
            {
                attachments[0] = swapchainImageviews[i];

                var framebufferCreate = new FramebufferCreateInfo
                {
                    SType = StructureType.FramebufferCreateInfo,
                    RenderPass = renderPass,
                    AttachmentCount = 1,
                    PAttachments = attachments,
                    Width = Extent.Width,
                    Height = Extent.Height,
                    Layers = 1
                };

                vk.CreateFramebuffer(device, in framebufferCreate, null, out swapchainFramebuffers[i]);
            }
        }

        public static implicit operator SwapchainKHR(VkSwapchain @this) => @this.swapchain;

        public void Dispose()
        {
            foreach (var buf in swapchainFramebuffers)
            {
                vk.DestroyFramebuffer(device, buf, null);
            }

            foreach (var imgView in swapchainImageviews)
            {
                vk.DestroyImageView(this.device, imgView, null);
            }

            if (khrSwapchainExt != null)
            {
                khrSwapchainExt.DestroySwapchain(device, swapchain, null);
                khrSwapchainExt.Dispose();
            }
        }
    }
}
