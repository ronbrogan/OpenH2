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
        private PhysicalDevice physicalDevice;
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

        public CommandPool CommandPool { get; private set; }
        public DescriptorPool DescriptorPool { get; private set; }

        public Extent2D Extent => this.ChooseSwapExtent();

        public PhysicalDeviceProperties PhysicalProperties { get; private set; }
        public PhysicalDeviceMemoryProperties MemoryProperties { get; private set; }


        public VkDevice(VulkanHost host, VkInstance instance, string[] validationLayers) : base(instance.vk)
        {
            this.vulkanHost = host;
            this.instance = instance;

            SUCCESS(vk.TryGetInstanceExtension(instance, out khrSurfaceExtension), "Couldn't get surface ext");
            this.surface = host.window.VkSurface.Create<AllocationCallbacks>(instance.Instance.ToHandle(), null).ToSurface();

            // Choose appropriate physical device and get relevant settings to create logical device and swapchains
            if (!ChooseDevice(instance, out physicalDevice, out var props, out queueFamilies, out caps, out format, out presentMode))
            {
                throw new Exception("No supported devices found");
            }

            this.PhysicalProperties = props;

            vk.GetPhysicalDeviceMemoryProperties(physicalDevice, out var memProps);
            this.MemoryProperties = memProps;

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

            var deviceFeatures = new PhysicalDeviceFeatures
            { 
                SamplerAnisotropy = true
            };


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
            SUCCESS(vk.CreateDevice(physicalDevice, in deviceCreate, null, out device), "Logical device creation failed");
            vk.GetDeviceQueue(device, queueFamilies.graphics.Value, 0, out graphicsQueue);
            vk.GetDeviceQueue(device, queueFamilies.present.Value, 0, out presentQueue);

            var poolCreate = new CommandPoolCreateInfo
            {
                SType = StructureType.CommandPoolCreateInfo,
                Flags = CommandPoolCreateFlags.CommandPoolCreateResetCommandBufferBit,
                QueueFamilyIndex = GraphicsQueueFamily.Value
            };

            SUCCESS(vk.CreateCommandPool(device, in poolCreate, null, out var commandPool), "CommandPool create failed");

            var uboPoolSize = new DescriptorPoolSize(DescriptorType.UniformBuffer, 1);
            var texPoolSize = new DescriptorPoolSize(DescriptorType.CombinedImageSampler, 1);
            var sizes = stackalloc DescriptorPoolSize[] { uboPoolSize, texPoolSize };
            var descPoolCreate = new DescriptorPoolCreateInfo
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = 2,
                PPoolSizes = sizes,
                MaxSets = 1
            };

            SUCCESS(vk.CreateDescriptorPool(device, in descPoolCreate, null, out var descriptorPool), "DescriptorPool create failed");

            this.CommandPool = commandPool;
            this.DescriptorPool = descriptorPool;
        }

        public VkSwapchain CreateSwapchain()
        {
            return new VkSwapchain(this.vulkanHost, this.instance, this);
        }

        public VkBuffer<T> CreateBuffer<T>(int count, BufferUsageFlags usage, MemoryPropertyFlags memoryProperties) where T: unmanaged
        {
            return new VkBuffer<T>(this, count, usage, memoryProperties);
        }

        public uint FindMemoryType(uint typeFilter, MemoryPropertyFlags properties)
        {
            for(var i = 0; i < this.MemoryProperties.MemoryTypeCount; i++)
            {
                if((typeFilter & (1 << i)) != 0 && (this.MemoryProperties.MemoryTypes[i].PropertyFlags & properties) == properties)
                {
                    return (uint)i;
                }
            }

            throw new NotSupportedException("Unable to find suitable memory type");
        }

        public void OneShotCommand(Action<CommandBuffer> command)
        {
            var bufferCreate = new CommandBufferAllocateInfo
            {
                SType = StructureType.CommandBufferAllocateInfo,
                Level = CommandBufferLevel.Primary,
                CommandPool = CommandPool,
                CommandBufferCount = 1
            };

            vk.AllocateCommandBuffers(device, in bufferCreate, out var commandBuffer);

            var begin = new CommandBufferBeginInfo
            {
                SType = StructureType.CommandBufferBeginInfo,
                Flags = CommandBufferUsageFlags.CommandBufferUsageOneTimeSubmitBit
            };

            vk.BeginCommandBuffer(commandBuffer, in begin);

            command(commandBuffer);

            vk.EndCommandBuffer(commandBuffer);

            var submit = new SubmitInfo
            {
                SType = StructureType.SubmitInfo,
                CommandBufferCount = 1,
                PCommandBuffers = &commandBuffer
            };

            vk.QueueSubmit(GraphicsQueue, 1, in submit, default);

            vk.QueueWaitIdle(GraphicsQueue);

            vk.FreeCommandBuffers(device, CommandPool, 1, in commandBuffer);
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
            out PhysicalDeviceProperties physicalDeviceProps,
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
                physicalDeviceProps = props;
                return true;
            }

            physicalDevice = default;
            physicalDeviceProps = default;
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

        private Extent2D ChooseSwapExtent()
        {
            khrSurfaceExtension.GetPhysicalDeviceSurfaceCapabilities(physicalDevice, surface, out caps);

            if (caps.CurrentExtent.Width != uint.MaxValue)
                return caps.CurrentExtent;

            var fbSize = this.vulkanHost.window.FramebufferSize;

            return new Extent2D
            {
                Width = (uint)Math.Clamp(fbSize.X, caps.MinImageExtent.Width, caps.MaxImageExtent.Width),
                Height = (uint)Math.Clamp(fbSize.Y, caps.MinImageExtent.Height, caps.MaxImageExtent.Height),
            };
        }

        public static implicit operator Device(VkDevice @this) => @this.device;

        public void Dispose()
        {
            vk.DestroyCommandPool(device, CommandPool, null);
            vk.DestroyDescriptorPool(device, DescriptorPool, null);

            if (khrSurfaceExtension != null)
            {
                khrSurfaceExtension.DestroySurface(instance, surface, null);
                khrSurfaceExtension.Dispose();
            }

            vk.DestroyDevice(this.device, null);
        }
    }
}
