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
        public Vector2 TexCoord;

        public VulkanTestVertex(Vector2 pos, Vector3 color, Vector2 tex)
        {
            this.VertexPosition = pos;
            this.VertexColor = color;
            this.TexCoord = tex;
        }
    }

    public struct UBO
    {
        public Matrix4x4 Model;
        public Matrix4x4 View;
        public Matrix4x4 Project;
    }

    public sealed unsafe class VulkanGraphicsAdapter : VkObject, IGraphicsAdapter, IDisposable
    {
        const int MaxFramesInFlight = 2;

        private VulkanHost vulkanHost;
        private VkInstance instance;
        private VkDevice device;
        private VulkanTextureBinder textureBinder;
        private VkSwapchain swapchain;
        private VkDefaultGraphicsPipeline pipeline;
        private VkImage image;
        private VkSampler sampler;
        private bool recreateSwapchain = false;

        ushort[] indices = new ushort[] { 0, 1, 2, 2, 3, 0 };
        VulkanTestVertex[] vertices = new VulkanTestVertex[]
        {
            new (new (-0.5f, -0.5f), new (1f, 0f, 0f), new (1f,0)),
            new (new (0.5f, -0.5f), new (0f, 1f, 0f), new (0,0)),
            new (new (0.5f, 0.5f), new (0f, 0f, 1f), new (0,1f)),
            new (new (-0.5f, 0.5f), new (1f, 1f, 1f), new (1f,1f)),
        };

        private VkBuffer<ushort> indexBuffer;
        private VkBuffer<VulkanTestVertex> vertexBuffer;


        // TODO: need multiple of these to support multiple in-flight frames
        private VkBuffer<UBO> uboBuffer;
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
            this.textureBinder = new VulkanTextureBinder(this.device);
            this.swapchain = device.CreateSwapchain();
            this.pipeline = new VkDefaultGraphicsPipeline(vulkanHost, device, swapchain);

            this.image = this.textureBinder.TestBind();
            this.sampler = new VkSampler(this.device);

            // =======================
            // vertex buffer setup
            // =======================

            using (var staging = device.CreateBuffer<VulkanTestVertex>(vertices.Length,
                BufferUsageFlags.BufferUsageTransferSrcBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit | MemoryPropertyFlags.MemoryPropertyHostCoherentBit))
            {
                staging.Load(vertices);

                this.vertexBuffer = device.CreateBuffer<VulkanTestVertex>(vertices.Length,
                    BufferUsageFlags.BufferUsageVertexBufferBit | BufferUsageFlags.BufferUsageTransferDstBit,
                    MemoryPropertyFlags.MemoryPropertyDeviceLocalBit);

                this.vertexBuffer.QueueLoad(staging);
            }

            using (var staging = device.CreateBuffer<ushort>(indices.Length,
                BufferUsageFlags.BufferUsageTransferSrcBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit | MemoryPropertyFlags.MemoryPropertyHostCoherentBit))
            {
                staging.Load(indices);

                this.indexBuffer = device.CreateBuffer<ushort>(indices.Length,
                    BufferUsageFlags.BufferUsageIndexBufferBit | BufferUsageFlags.BufferUsageTransferDstBit,
                    MemoryPropertyFlags.MemoryPropertyDeviceLocalBit);

                this.indexBuffer.QueueLoad(staging);
            }

            this.uboBuffer = device.CreateBuffer<UBO>(1, 
                BufferUsageFlags.BufferUsageUniformBufferBit, 
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit | MemoryPropertyFlags.MemoryPropertyHostCoherentBit);




            this.pipeline.CreateDescriptors(this.uboBuffer, this.image, this.sampler);

            // =======================
            // commandbuffer setup
            // =======================

            var commandBufAlloc = new CommandBufferAllocateInfo
            {
                SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = device.CommandPool,
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

            UpdateUbo(imageIndex);

            this.pipeline.BeginPass(commandBuffer, imageIndex);

            this.pipeline.Bind(commandBuffer);

            var vertexBuffers = stackalloc Buffer[]{ vertexBuffer };
            var offsets = stackalloc ulong[] { 0 };
            vk.CmdBindVertexBuffers(commandBuffer, 0, 1, vertexBuffers, offsets);

            vk.CmdBindIndexBuffer(commandBuffer, indexBuffer, 0, IndexType.Uint16);

            //vk.CmdDraw(commandBuffer, (uint)vertices.Length, 1, 0, 0);
            vk.CmdDrawIndexed(commandBuffer, (uint)indices.Length, 1, 0, 0, 0);
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

        

        static DateTimeOffset start = DateTimeOffset.UtcNow;
        const float RadiansPer = (MathF.PI / 180);
        private static float DegToRad(float deg) => RadiansPer * deg;
        private void UpdateUbo(uint imageIndex)
        {
            var time = (float)(DateTimeOffset.UtcNow - start).TotalSeconds;

            var ubo = new UBO
            {
                Model = Matrix4x4.CreateRotationZ(time * DegToRad(90)),
                View = Matrix4x4.CreateLookAt(new(2f, 2f, 2f), new(0, 0, 0), new(0, 0, 1f)),
                Project = Matrix4x4.CreatePerspectiveFieldOfView(DegToRad(45), swapchain.Extent.Width / (float)swapchain.Extent.Height, 0.1f, 10f)
            };

            // Correct for OpenGL Y inversion
            ubo.Project.M22 *= -1;

            this.uboBuffer/*[imageIndex]*/.Load(stackalloc UBO[] { ubo });
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

            // TODO: multiple frame support: recreate uniform buffers, command buffers, pools??
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
            this.indexBuffer.Dispose();
            this.uboBuffer.Dispose();

            vk.DestroySemaphore(device, imageAvailableSemaphore, null);
            vk.DestroySemaphore(device, renderFinishedSemaphore, null);
            vk.DestroyFence(device, inFlightFence, null);

            // destroy swapchain
            this.swapchain.Dispose();

            this.image.Dispose();
            this.sampler.Dispose();

            // destroy pipeline
            this.pipeline.Dispose();

            // destroy device
            this.device.Dispose();

            // destroy instance
            this.instance.Dispose();
        }
    }
}
