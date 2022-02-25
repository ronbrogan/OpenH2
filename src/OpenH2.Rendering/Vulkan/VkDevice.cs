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
        private KhrSurface khrSurfaceExtension;

        private SurfaceKHR surface;
        private SurfaceFormatKHR format;

        private (uint? graphics, uint? present) queueFamilies;
        private Queue graphicsQueue;
        private Queue presentQueue;

        private (Format format, Extent2D extent) swapchainParams;
        private readonly VulkanHost vulkanHost;

        public KhrSurface KhrSurfaceExtension => khrSurfaceExtension;
        public SurfaceKHR Surface => surface;
        public SurfaceFormatKHR SurfaceFormat => format;

        public uint? GraphicsQueueFamily => queueFamilies.graphics;
        public Queue GraphicsQueue => graphicsQueue;
        public uint? PresentQueueFamily => queueFamilies.present;
        public Queue PresentQueue => presentQueue;

        private PresentModeKHR presentMode;
        public PresentModeKHR PresentMode => presentMode;

        private SurfaceCapabilitiesKHR caps;
        public SurfaceCapabilitiesKHR SurfaceCapabilities => caps;

        public Extent2D Extent => this.ChooseSwapExtent(this.caps);

        public VkDevice(VulkanHost host, VkInstance instance, string[] validationLayers) : base(instance.vk)
        {
            this.vulkanHost = host;
            this.instance = instance;

            SUCCESS(vk.TryGetInstanceExtension(instance, out khrSurfaceExtension), "Couldn't get surface ext");
            this.surface = host.window.VkSurface.Create<AllocationCallbacks>(instance.Instance.ToHandle(), null).ToSurface();

            // Choose appropriate physical device and get relevant settings to create logical device and swapchains
            if (!ChooseDevice(instance, out var phyDevice, out queueFamilies, out caps, out format, out presentMode))
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
            vk.GetDeviceQueue(device, queueFamilies.graphics.Value, 0, out graphicsQueue);
            vk.GetDeviceQueue(device, queueFamilies.present.Value, 0, out presentQueue);
        }

        public VkSwapchain CreateSwapchain()
        {
            return new VkSwapchain(this.vulkanHost, this.instance, this);
        }

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
            out PresentModeKHR presentMode)
        {
            queueFamilies = default;
            capabilities = default;
            format = default;
            presentMode = PresentModeKHR.PresentModeFifoKhr;

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
            if (khrSurfaceExtension != null)
            {
                khrSurfaceExtension.DestroySurface(instance, surface, null);
                khrSurfaceExtension.Dispose();
            }

            vk.DestroyDevice(this.device, null);
        }
    }
}
