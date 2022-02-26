using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Shaders;
using Silk.NET.Vulkan;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace OpenH2.Rendering.Vulkan
{
    public struct VulkanTestVertex
    {
        public Vector2 VertexPosition;
        public Vector3 VertexColor;

        public VulkanTestVertex(Vector2 pos, Vector3 color)
        {
            this.VertexPosition = pos;
            this.VertexColor = color;
        }
    }

    public sealed unsafe class VulkanGraphicsAdapter : VkObject, IGraphicsAdapter, IDisposable
    {
        const int MaxFramesInFlight = 2;

        private VulkanHost vulkanHost;
        private VkInstance instance;
        private VkDevice device;
        private VkSwapchain swapchain;
        private VkDefaultGraphicsPipeline pipeline;

        private CommandPool commandPool;

        private bool recreateSwapchain = false;

        VulkanTestVertex[] vertices = new VulkanTestVertex[]
        {
            new (new (0f, -0.5f), new (1f, 1f, 1f)),
            new (new (0.5f, 0.5f), new (0f, 1f, 0f)),
            new (new (-0.5f, 0.5f), new (0f, 0f, 1f)),
        };
        private VkBuffer<VulkanTestVertex> vertexBuffer;

        // TODO: need multiple of these to support multiple in-flight frames
        private CommandBuffer commandBuffer;
        private Semaphore imageAvailableSemaphore;
        private Semaphore renderFinishedSemaphore;
        private Fence inFlightFence;

        public VulkanGraphicsAdapter(VulkanHost vulkanHost) : base(vulkanHost.vk)
        {
            this.vulkanHost = vulkanHost;

            vulkanHost.window.Resize += (s) => recreateSwapchain = true;

            this.instance = new VkInstance(vulkanHost);
            this.device = instance.CreateDevice();
            this.swapchain = device.CreateSwapchain();
            this.pipeline = new VkDefaultGraphicsPipeline(vulkanHost, device, swapchain);

            var poolCreate = new CommandPoolCreateInfo
            {
                SType = StructureType.CommandPoolCreateInfo,
                Flags = CommandPoolCreateFlags.CommandPoolCreateResetCommandBufferBit,
                QueueFamilyIndex = device.GraphicsQueueFamily.Value
            };

            SUCCESS(vk.CreateCommandPool(device, in poolCreate, null, out commandPool), "CommandPool create failed");

            // =======================
            // vertex buffer setup
            // =======================

            using (var vertexStagingBuffer = device.CreateBuffer<VulkanTestVertex>(vertices.Length,
                BufferUsageFlags.BufferUsageTransferSrcBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit | MemoryPropertyFlags.MemoryPropertyHostCoherentBit))
            {
                vertexStagingBuffer.Load(vertices);

                this.vertexBuffer = device.CreateBuffer<VulkanTestVertex>(vertices.Length,
                    BufferUsageFlags.BufferUsageVertexBufferBit | BufferUsageFlags.BufferUsageTransferDstBit,
                    MemoryPropertyFlags.MemoryPropertyDeviceLocalBit);

                this.vertexBuffer.QueueLoad(vertexStagingBuffer, commandPool);
            }

            // =======================
            // commandbuffer setup
            // =======================

            var commandBufAlloc = new CommandBufferAllocateInfo
            {
                SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = commandPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = 1
            };

            SUCCESS(vk.AllocateCommandBuffers(device, in commandBufAlloc, out commandBuffer), "Command buffer alloc failed");

            // =======================
            // sync setup
            // =======================

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

            var acquired = swapchain.AcquireNextImage(imageAvailableSemaphore, default, ref imageIndex);

            if(acquired == Result.ErrorOutOfDateKhr)
            {
                RecreateSwapchain();
                return;
            } 
            else if (acquired != Result.Success && acquired != Result.SuboptimalKhr)
            {
                throw new Exception("Failed to acquire swapchain image");
            }

            vk.ResetFences(device, 1, in inFlightFence);

            vk.ResetCommandBuffer(commandBuffer, (CommandBufferResetFlags)0);

            var bufBegin = new CommandBufferBeginInfo
            {
                SType = StructureType.CommandBufferBeginInfo
            };

            SUCCESS(vk.BeginCommandBuffer(commandBuffer, in bufBegin), "Unable to begin writing to command buffer");

            this.pipeline.BeginPass(commandBuffer, imageIndex);

            this.pipeline.Bind(commandBuffer);

            var vertexBuffers = stackalloc Buffer[]{ vertexBuffer };
            var offsets = stackalloc ulong[] { 0 };
            vk.CmdBindVertexBuffers(commandBuffer, 0, 1, vertexBuffers, offsets);

            vk.CmdDraw(commandBuffer, (uint)vertices.Length, 1, 0, 0);
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

            var presented = swapchain.QueuePresent(in presentInfo);

            if (presented == Result.ErrorOutOfDateKhr || presented == Result.SuboptimalKhr || recreateSwapchain)
            {
                RecreateSwapchain();
            }
            else if (presented != Result.Success)
            {
                throw new Exception("Failed to acquire swapchain image");
            }
        }

        private void RecreateSwapchain()
        {
            // TODO: spin render thread if width/height are zero, invalid framebuffer for vulkan (happens when minimized)

            recreateSwapchain = false;
            vk.DeviceWaitIdle(device);
            this.pipeline.DestroyResources();
            this.swapchain.DestroyResources();
            this.swapchain.CreateResources();
            this.pipeline.CreateResources();
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

            this.vertexBuffer.Dispose();

            vk.DestroySemaphore(device, imageAvailableSemaphore, null);
            vk.DestroySemaphore(device, renderFinishedSemaphore, null);
            vk.DestroyFence(device, inFlightFence, null);

            vk.DestroyCommandPool(device, commandPool, null);

            // destroy swapchain
            this.swapchain.Dispose();

            // destroy pipeline
            this.pipeline.Dispose();

            // destroy device
            this.device.Dispose();

            // destroy instance
            this.instance.Dispose();
        }
    }
}
