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
    public sealed unsafe class VulkanGraphicsAdapter : IGraphicsAdapter, IDisposable
    {
        private VulkanHost vulkanHost;
        private Vk vk;
        private IVkSurface vkSurface;
        private KhrSurface khrSurfaceExtension;
        private Instance instance;
        private Device device;
        private SurfaceKHR surface;
        private Queue graphicsQueue;
        private Queue presentQueue;
        private KhrSwapchain khrSwapchainExt;
        private SwapchainKHR swapchain;
        private Image[] swapchainImages = Array.Empty<Image>();
        private ImageView[] swapchainImageviews = Array.Empty<ImageView>();
        private Framebuffer[] swapchainFramebuffers = Array.Empty<Framebuffer>();
        private (Format format, Extent2D extent) swapchainParams;
        private PipelineLayout pipelineLayout;
        private RenderPass renderPass;
        private Pipeline graphicsPipeline;
        private CommandPool commandPool;
        private CommandBuffer commandBuffer;

        private Semaphore imageAvailableSemaphore;
        private Semaphore renderFinishedSemaphore;
        private Fence inFlightFence;

        public VulkanGraphicsAdapter(VulkanHost vulkanHost, IVkSurface vkSurface)
        {
            this.vkSurface = vkSurface;
            this.vulkanHost = vulkanHost;
            this.vk = Vk.GetApi();

            var extensionStrings = vkSurface.GetRequiredExtensions(out var reqExtensionCount);

            uint extensionCount = 0;
            vk.EnumerateInstanceExtensionProperties(0, ref extensionCount, null);
            var extensions = new ExtensionProperties[extensionCount];
            vk.EnumerateInstanceExtensionProperties(0, ref extensionCount, ref extensions[0]);

            Console.WriteLine("Extensions");
            foreach (var ext in extensions)
            {
                Console.WriteLine("\t" + Encoding.UTF8.GetNullTerminatedString(ext.ExtensionName, 128));
            }

            uint availableLayerCount = 0;
            vk.EnumerateInstanceLayerProperties(ref availableLayerCount, null);
            var layers = new LayerProperties[availableLayerCount];
            vk.EnumerateInstanceLayerProperties(ref availableLayerCount, ref layers[0]);

            var layerCount = 1u;
            var layerNames = stackalloc byte*[] { PinnedUtf8.Get("VK_LAYER_KHRONOS_validation") };

            Console.WriteLine("Layers");
            foreach (var layer in layers)
            {
                Console.WriteLine("\t" + Encoding.UTF8.GetNullTerminatedString(layer.LayerName, 128));
            }

            var appInfo = new ApplicationInfo
            {
                SType = StructureType.ApplicationInfo,
                PApplicationName = PinnedUtf8.Get("OpenH2"),
                ApplicationVersion = Vk.MakeVersion(0, 0, 1),
                PEngineName = PinnedUtf8.Get("OpenH2.Engine"),
                EngineVersion = Vk.MakeVersion(0, 0, 1),
                ApiVersion = Vk.Version10
            };

            var instanceInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo,
                EnabledExtensionCount = reqExtensionCount,
                PpEnabledExtensionNames = extensionStrings,
                EnabledLayerCount = layerCount,
                PpEnabledLayerNames = layerNames
            };

            SUCCESS(vk.CreateInstance(in instanceInfo, null, out instance), "Couldn't create instance");
            SUCCESS(vk.TryGetInstanceExtension(instance, out khrSurfaceExtension), "Couldn't get surface ext");
            this.surface = this.vkSurface.Create<AllocationCallbacks>(instance.ToHandle(), null).ToSurface();

            // Choose appropriate physical device and get relevant settings to create logical device and swapchains
            if (!ChooseDevice(instance, out var phyDevice, out var queueFamilies, out var caps, out var format, out var presentMode, out var extent))
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

            var deviceCreate = new DeviceCreateInfo
            {
                SType = StructureType.DeviceCreateInfo,
                PQueueCreateInfos = queueCreateInfos,
                QueueCreateInfoCount = (uint)uniqueFamilies.Count,
                PEnabledFeatures = &deviceFeatures,
                EnabledLayerCount = layerCount,
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
                Surface = this.surface,
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

            // =======================
            //  start pipeline setup
            // =======================

            var vertShader = VkShaderCompiler.LoadSpirvShader(vk, device, "VulkanTest", "vert");
            var fragShader = VkShaderCompiler.LoadSpirvShader(vk, device, "VulkanTest", "frag");

            var vertCreate = new PipelineShaderStageCreateInfo
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.ShaderStageVertexBit,
                Module = vertShader,
                PName = PinnedUtf8.Get("main")
            };

            var fragCreate = new PipelineShaderStageCreateInfo
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.ShaderStageFragmentBit,
                Module = fragShader,
                PName = PinnedUtf8.Get("main")
            };

            var shaderStages = stackalloc PipelineShaderStageCreateInfo[] { vertCreate, fragCreate };

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

            var viewport = new Viewport(0, 0, swapchainParams.extent.Width, swapchainParams.extent.Height, 0, 1f);
            var scissor = new Rect2D(new Offset2D(0, 0), swapchainParams.extent);

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
                Format = swapchainParams.format,
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

            SUCCESS(vk.CreateGraphicsPipelines(device, default, 1, &pipelineCreate, null, out graphicsPipeline), "Pipeline create failed");

            // Clear to destroy shader info after pipeline creation
            vk.DestroyShaderModule(device, vertShader, null);
            vk.DestroyShaderModule(device, fragShader, null);

            // =======================
            // framebuffer setup
            // =======================

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
                    Width = swapchainParams.extent.Width,
                    Height = swapchainParams.extent.Height,
                    Layers = 1
                };

                vk.CreateFramebuffer(device, in framebufferCreate, null, out swapchainFramebuffers[i]);
            }

            // =======================
            // commandbuffer setup
            // =======================

            var poolCreate = new CommandPoolCreateInfo
            {
                SType = StructureType.CommandPoolCreateInfo,
                Flags = CommandPoolCreateFlags.CommandPoolCreateResetCommandBufferBit,
                QueueFamilyIndex = queueFamilies.graphics.Value
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

        private bool QuerySwapchainSupport(PhysicalDevice phyDevice, 
            out SurfaceCapabilitiesKHR capabilities, 
            out SurfaceFormatKHR[] formats, 
            out PresentModeKHR[] presentModes )
        {
            var kse = khrSurfaceExtension;
            kse.GetPhysicalDeviceSurfaceCapabilities(phyDevice, this.surface, out capabilities);

            var formatCount = 0u;
            kse.GetPhysicalDeviceSurfaceFormats(phyDevice, this.surface, ref formatCount, null);
            formats = new SurfaceFormatKHR[formatCount];
            kse.GetPhysicalDeviceSurfaceFormats(phyDevice, this.surface, ref formatCount, out formats[0]);

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

                if(presentSupported)
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

        public void AddLight(PointLight light)
        {
        }

        private uint imageIndex = 0;
        public void BeginFrame(GlobalUniform matricies)
        {
            vk.WaitForFences(device, 1, in inFlightFence, true, ulong.MaxValue);
            vk.ResetFences(device, 1, in inFlightFence);

            khrSwapchainExt.AcquireNextImage(device, swapchain, ulong.MaxValue, imageAvailableSemaphore, default, ref imageIndex);

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
                Framebuffer = swapchainFramebuffers[imageIndex],
                RenderArea = new Rect2D(new Offset2D(0, 0), swapchainParams.extent),
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

            SUCCESS(vk.QueueSubmit(graphicsQueue, 1, in submitInfo, inFlightFence));

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

            khrSwapchainExt.QueuePresent(presentQueue, in presentInfo);
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
            vk.DestroySemaphore(device, imageAvailableSemaphore, null);
            vk.DestroySemaphore(device, renderFinishedSemaphore, null);
            vk.DestroyFence(device, inFlightFence, null);

            vk.DestroyCommandPool(device, commandPool, null);

            foreach(var buf in swapchainFramebuffers)
            {
                vk.DestroyFramebuffer(device, buf, null);
            }

            vk.DestroyPipeline(device, graphicsPipeline, null);
            vk.DestroyPipelineLayout(device, pipelineLayout, null);
            vk.DestroyRenderPass(device, renderPass, null);

            foreach(var imgView in swapchainImageviews)
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
            vk.DestroyInstance(this.instance, null);
        }

        void SUCCESS(Result result, string? description = null)
        {
            if(result != Result.Success)
            {
                throw new Exception(description ?? "Vulkan operation failed");
            }
        }

        void SUCCESS(bool result, string? description = null)
        {
            if (!result)
            {
                throw new Exception(description ?? "Vulkan operation failed");
            }
        }

        private class PinnedUtf8
        {
            private static ConditionalWeakTable<string, byte[]> pins = new ConditionalWeakTable<string, byte[]>();
            public readonly byte* Address;

            public PinnedUtf8(string value)
            {
                this.Address = Get(value);
            }

            public static byte* Get(string value)
            {
                // Maybe shouldn't use framework interning?
                var interned = string.Intern(value);

                if(pins.TryGetValue(interned, out var arr))
                {
                    return (byte*)Unsafe.AsPointer(ref arr[0]);
                }

                lock(pins)
                {
                    if(pins.TryGetValue(interned, out arr))
                    {
                        return (byte*)Unsafe.AsPointer(ref arr[0]);
                    }

                    arr = GC.AllocateUninitializedArray<byte>(Encoding.UTF8.GetByteCount(interned) + 1, true);
                    Encoding.UTF8.GetBytes(interned, arr);
                    arr[arr.Length - 1] = 0; // Null terminate
                    pins.Add(interned, arr);
                }

                return (byte*)Unsafe.AsPointer(ref arr[0]); // Just grab pointer since it's already pinned
            }

            public static implicit operator byte*(PinnedUtf8 obj)
            {
                return obj.Address;
            }
        }
    }
}
