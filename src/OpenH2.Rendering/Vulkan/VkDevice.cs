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
    internal unsafe sealed class VkDevice : VkObject, IDisposable
    {
        private Device device;
        private VkInstance instance;
        private KhrSwapchain khrSwapchainExt;
        private KhrSurface khrSurfaceExtension;

        private SurfaceKHR surface;

        private (uint? graphics, uint? present) queueFamilies;
        private Queue graphicsQueue;
        private Queue presentQueue;
        private SwapchainKHR swapchain;
        private Image[] swapchainImages;
        private ImageView[] swapchainImageviews;
        private Framebuffer[] swapchainFramebuffers;
        private (Format format, Extent2D extent) swapchainParams;
        private readonly VulkanHost vulkanHost;

        public KhrSurface KhrSurfaceExtension => khrSurfaceExtension;
        public SurfaceKHR Surface => surface;

        public Extent2D Extent => swapchainParams.extent;
        public Format Format => swapchainParams.format;

        public uint? GraphicsQueueFamily => queueFamilies.graphics;
        public Queue GraphicsQueue => graphicsQueue;
        public Queue PresentQueue => presentQueue;

        public SwapchainKHR Swapchain => swapchain;
        public Image[] Images => swapchainImages;
        public ImageView[] ImageViews => swapchainImageviews;
        public Framebuffer[] Framebuffers => swapchainFramebuffers;

        public VkDevice(VulkanHost host, VkInstance instance, string[] validationLayers) : base(instance.vk)
        {
            this.vulkanHost = host;
            this.instance = instance;

            SUCCESS(vk.TryGetInstanceExtension(instance, out khrSurfaceExtension), "Couldn't get surface ext");
            this.surface = host.window.VkSurface.Create<AllocationCallbacks>(instance.Instance.ToHandle(), null).ToSurface();

            // Choose appropriate physical device and get relevant settings to create logical device and swapchains
            if (!ChooseDevice(instance, out var phyDevice, out queueFamilies, out var caps, out var format, out var presentMode, out var extent))
            {
                throw new Exception("No supported devices found");
            }

            var uniqueFamilies = new HashSet<uint> { queueFamilies.graphics.Value, queueFamilies.present.Value };
            var queueCreateInfos = stackalloc DeviceQueueCreateInfo[uniqueFamilies.Count];
            float priority = 1f;
            for (var f = 0; f < uniqueFamilies.Count; f++)
            {
                queueCreateInfos[f] = new DeviceQueueCreateInfo
                {
                    SType = StructureType.DeviceQueueCreateInfo,
                    QueueFamilyIndex = uniqueFamilies.ElementAt(f),
                    QueueCount = 1,
                    PQueuePriorities = &priority
                };
            }

            var deviceFeatures = new PhysicalDeviceFeatures();

            var deviceExtensionCount = 1u;
            var deviceExtensionNames = stackalloc byte*[]
            {
                PinnedUtf8.Get("VK_KHR_swapchain")
            };

            var layerNames = stackalloc byte*[validationLayers.Length];
            for(var i = 0; i < validationLayers.Length; i++)
            {
                layerNames[i] = PinnedUtf8.Get(validationLayers[i]);
            }

            var deviceCreate = new DeviceCreateInfo
            {
                SType = StructureType.DeviceCreateInfo,
                PQueueCreateInfos = queueCreateInfos,
                QueueCreateInfoCount = (uint)uniqueFamilies.Count,
                PEnabledFeatures = &deviceFeatures,
                EnabledLayerCount = (uint)validationLayers.Length,
                PpEnabledLayerNames = layerNames,
                EnabledExtensionCount = deviceExtensionCount,
                PpEnabledExtensionNames = deviceExtensionNames
            };

            // Create logical device and grab device-specific resources
            SUCCESS(vk.CreateDevice(phyDevice, in deviceCreate, null, out device), "Logical device creation failed");
            SUCCESS(vk.TryGetDeviceExtension(instance, device, out khrSwapchainExt), "Couldn't get swapchain extension");
            vk.GetDeviceQueue(device, queueFamilies.graphics.Value, 0, out graphicsQueue);
            vk.GetDeviceQueue(device, queueFamilies.present.Value, 0, out presentQueue);

            // One more than min count, unless we're bound by max image count
            var imgCount = caps.MinImageCount + 1;
            if (caps.MaxImageCount > 0 && imgCount > caps.MaxImageCount)
                imgCount = caps.MaxImageCount;

            // Prepare to create swapchain with what we've found
            var swapchainCreate = new SwapchainCreateInfoKHR
            {
                SType = StructureType.SwapchainCreateInfoKhr,
                Surface = surface,
                MinImageCount = imgCount,
                ImageFormat = format.Format,
                ImageColorSpace = format.ColorSpace,
                ImageExtent = extent,
                ImageArrayLayers = 1,
                ImageUsage = ImageUsageFlags.ImageUsageColorAttachmentBit,
                PreTransform = caps.CurrentTransform,
                CompositeAlpha = CompositeAlphaFlagsKHR.CompositeAlphaOpaqueBitKhr,
                PresentMode = presentMode,
                Clipped = true,
                OldSwapchain = default,
                ImageSharingMode = SharingMode.Exclusive,
            };

            // If we're not using the same queue, setup concurrent images
            if (queueFamilies.graphics != queueFamilies.present)
            {
                var queueFamilyIndices = stackalloc uint[] { queueFamilies.graphics.Value, queueFamilies.present.Value };
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
            swapchainParams = (format.Format, extent);

            for (int i = 0; i < imgCount; i++)
            {
                var imgViewCreate = new ImageViewCreateInfo
                {
                    SType = StructureType.ImageViewCreateInfo,
                    Image = swapchainImages[i],
                    ViewType = ImageViewType.ImageViewType2D,
                    Format = format.Format,
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
            return khrSwapchainExt.QueuePresent(presentQueue, in presentInfo);
        }

        // TODO: move to instance?
        private bool QuerySwapchainSupport(PhysicalDevice phyDevice,
            out SurfaceCapabilitiesKHR capabilities,
            out SurfaceFormatKHR[] formats,
            out PresentModeKHR[] presentModes)
        {
            var kse = khrSurfaceExtension;
            kse.GetPhysicalDeviceSurfaceCapabilities(phyDevice, surface, out capabilities);

            var formatCount = 0u;
            kse.GetPhysicalDeviceSurfaceFormats(phyDevice, surface, ref formatCount, null);
            formats = new SurfaceFormatKHR[formatCount];
            kse.GetPhysicalDeviceSurfaceFormats(phyDevice, surface, ref formatCount, out formats[0]);

            var presentModeCount = 0u;
            kse.GetPhysicalDeviceSurfacePresentModes(phyDevice, surface, ref presentModeCount, null);
            presentModes = new PresentModeKHR[presentModeCount];
            kse.GetPhysicalDeviceSurfacePresentModes(phyDevice, surface, ref presentModeCount, out presentModes[0]);

            return formatCount > 0 && presentModeCount > 0;
        }

        private (uint? graphics, uint? present) FindQueueFamilies(PhysicalDevice device)
        {
            uint? graphics = null;
            uint? present = null;

            uint familyCount = 0;
            vk.GetPhysicalDeviceQueueFamilyProperties(device, ref familyCount, null);

            var props = new QueueFamilyProperties[familyCount];
            vk.GetPhysicalDeviceQueueFamilyProperties(device, ref familyCount, out props[0]);

            for (var i = 0u; i < props.Length; i++)
            {
                QueueFamilyProperties prop = props[i];
                if (prop.QueueFlags.HasFlag(QueueFlags.QueueGraphicsBit))
                    graphics = i;

                SUCCESS(khrSurfaceExtension.GetPhysicalDeviceSurfaceSupport(device, i, surface, out var presentSupported), "Surface query failed");

                if (presentSupported)
                {
                    present = i;
                }

                if (graphics.HasValue && present.HasValue)
                    break;

                i++;
            }

            return (graphics, present);
        }

        private bool ChooseDevice(
            Instance instance,
            out PhysicalDevice physicalDevice,
            out (uint? graphics, uint? present) queueFamilies,
            out SurfaceCapabilitiesKHR capabilities,
            out SurfaceFormatKHR format,
            out PresentModeKHR presentMode,
            out Extent2D extent)
        {
            queueFamilies = default;
            capabilities = default;
            format = default;
            presentMode = PresentModeKHR.PresentModeFifoKhr;
            extent = default;

            uint deviceCount = 0;
            vk.EnumeratePhysicalDevices(instance, ref deviceCount, null);
            var devices = new PhysicalDevice[deviceCount];
            vk.EnumeratePhysicalDevices(instance, ref deviceCount, ref devices[0]);

            Console.WriteLine("Devices");
            foreach (var device in devices)
            {
                vk.GetPhysicalDeviceProperties(device, out var props);
                vk.GetPhysicalDeviceFeatures(device, out var feats);

                Console.WriteLine("\t" + Encoding.UTF8.GetNullTerminatedString(props.DeviceName, 128));

                if (!feats.GeometryShader)
                    continue;

                queueFamilies = FindQueueFamilies(device);

                // TODO: query all required extensions (swapchain, etc)

                if (!QuerySwapchainSupport(device, out capabilities, out var formats, out var presentModes))
                    continue;

                format = ChooseFormat(formats);
                presentMode = ChoosePresentMode(presentModes);
                extent = ChooseSwapExtent(capabilities);

                if (!queueFamilies.graphics.HasValue)
                    continue;

                physicalDevice = device;
                return true;
            }

            physicalDevice = default;
            return false;
        }

        private SurfaceFormatKHR ChooseFormat(SurfaceFormatKHR[] formats)
        {
            foreach (var format in formats)
            {
                if (format.Format == Format.B8G8R8A8Srgb && format.ColorSpace == ColorSpaceKHR.ColorSpaceSrgbNonlinearKhr)
                    return format;
            }

            return formats[0];
        }

        private PresentModeKHR ChoosePresentMode(PresentModeKHR[] modes)
        {
            foreach (var mode in modes)
            {
                if (mode == PresentModeKHR.PresentModeMailboxKhr)
                    return mode;
            }

            return PresentModeKHR.PresentModeFifoKhr;
        }

        private Extent2D ChooseSwapExtent(SurfaceCapabilitiesKHR capabilities)
        {
            if (capabilities.CurrentExtent.Width != uint.MaxValue)
                return capabilities.CurrentExtent;

            var fbSize = this.vulkanHost.window.FramebufferSize;

            return new Extent2D
            {
                Width = (uint)Math.Clamp(fbSize.X, capabilities.MinImageExtent.Width, capabilities.MaxImageExtent.Width),
                Height = (uint)Math.Clamp(fbSize.Y, capabilities.MinImageExtent.Height, capabilities.MaxImageExtent.Height),
            };
        }

        public static implicit operator Device(VkDevice @this) => @this.device;

        public void Dispose()
        {
            foreach (var imgView in swapchainImageviews)
            {
                vk.DestroyImageView(this.device, imgView, null);
            }

            if (khrSwapchainExt != null)
            {
                khrSwapchainExt.DestroySwapchain(device, swapchain, null);
                khrSwapchainExt.Dispose();
            }

            if (khrSurfaceExtension != null)
            {
                khrSurfaceExtension.DestroySurface(instance, surface, null);
                khrSurfaceExtension.Dispose();
            }

            vk.DestroyDevice(this.device, null);
        }

        
    }
}
