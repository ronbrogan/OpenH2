using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Shaders;
using OpenH2.Rendering.Vulkan.Internals;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using Buffer = Silk.NET.Vulkan.Buffer;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace OpenH2.Rendering.Vulkan
{
    public struct VulkanTestUniform
    {
        public Sampler Texture;
    }

    public sealed unsafe class VulkanGraphicsAdapter : VkObject, IGraphicsAdapter, IDisposable
    {
        const int MaxFramesInFlight = 2;

        private VulkanHost vulkanHost;
        private VkInstance instance;
        private VkDevice device;
        private VulkanTextureBinder textureBinder;
        private VkSwapchain swapchain;
        private VkRenderPass renderpass;
        
        private VkImage image;
        private VkSampler sampler;
        private bool recreateSwapchain = false;

        private Dictionary<MeshElementType, VkDefaultGraphicsPipeline> pipelines = new();

        private int nextModelHandle = 0;
        private (VkBuffer<int> indices, VkBuffer<VertexFormat> vertices)[] models = new (VkBuffer<int> indices, VkBuffer<VertexFormat> vertices)[2048];


        // TODO: need multiple of these to support multiple in-flight frames
        private VkBuffer<GlobalUniform> globalBuffer;
        private VkBuffer<VulkanTestUniform> shaderUniformBuffer;

        private int nextTransformIndex = 0;
        private VkBuffer<TransformUniform> transformBuffer;
        private CommandBuffer renderCommands;
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
            this.renderpass = new VkRenderPass(device, swapchain);

            // TODO: precompute all pipelines?
            this.pipelines.Add(MeshElementType.TriangleList, new VkDefaultGraphicsPipeline(vulkanHost, device, swapchain, renderpass, MeshElementType.TriangleList));
            this.pipelines.Add(MeshElementType.TriangleStrip, new VkDefaultGraphicsPipeline(vulkanHost, device, swapchain, renderpass, MeshElementType.TriangleStrip));


            this.image = this.textureBinder.TestBind();
            this.sampler = image.CreateSampler();


            this.globalBuffer = device.CreateBuffer<GlobalUniform>(1, 
                BufferUsageFlags.BufferUsageUniformBufferBit, 
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit | MemoryPropertyFlags.MemoryPropertyHostCoherentBit);

            this.shaderUniformBuffer = device.CreateBuffer<VulkanTestUniform>(1,
                BufferUsageFlags.BufferUsageUniformBufferBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit | MemoryPropertyFlags.MemoryPropertyHostCoherentBit);

            this.transformBuffer = device.CreateDynamicUniformBuffer<TransformUniform>(16384,
                BufferUsageFlags.BufferUsageUniformBufferBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit | MemoryPropertyFlags.MemoryPropertyHostCoherentBit);

            // Leaving mapped so that we can write during render
            this.transformBuffer.Map();

            this.pipelines[MeshElementType.TriangleList].CreateDescriptors(this.globalBuffer, this.transformBuffer, this.shaderUniformBuffer, new[] { (this.image, this.sampler) });
            this.pipelines[MeshElementType.TriangleStrip].CreateDescriptors(this.globalBuffer, this.transformBuffer, this.shaderUniformBuffer, new[] { (this.image, this.sampler) });

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

            SUCCESS(vk.AllocateCommandBuffers(device, in commandBufAlloc, out renderCommands), "Command buffer alloc failed");

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
        private Shader currentShader;
        private TransformUniform currentTransform;

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

            UpdateGlobals(imageIndex, matricies);

            vk.ResetCommandBuffer(renderCommands, (CommandBufferResetFlags)0);

            var bufBegin = new CommandBufferBeginInfo
            {
                SType = StructureType.CommandBufferBeginInfo
            };

            SUCCESS(vk.BeginCommandBuffer(renderCommands, in bufBegin), "Unable to begin writing to command buffer");

            this.renderpass.Begin(renderCommands, imageIndex);

        }

        public void DrawMeshes(DrawCommand[] commands)
        {
            if(this.currentShader != Shader.Generic)
            {
                return;
            }

            if (commands.Length == 0)
                return;

            if (!this.pipelines.TryGetValue(commands[0].ElementType, out var pipeline))
                return;

            pipeline.Bind(renderCommands);


            // Find properly aligned offset to write current transform into
            var xformIndex = Interlocked.Increment(ref nextTransformIndex);
            //if (xformIndex >= 255) return;
            var xformOffset = device.AlignUboItem<TransformUniform>(xformIndex);
            // Write the current transform to that offset
            // TODO: do bounds check and re-allocate larger buffer, or somthing
            this.transformBuffer.LoadMapped(xformOffset, this.currentTransform);
            // Bind the descriptor with the offset
            Span<uint> dynamics = stackalloc uint[] { (uint)xformOffset };
            pipeline.BindDescriptors(renderCommands, dynamics);

            var vertexBuffers = stackalloc Buffer[1];
            var offsets = stackalloc ulong[] { 0 };
            for (var i = 0; i < commands.Length; i++)
            {
                ref DrawCommand command = ref commands[i];

                // Upload shader uniform data
                //BindOrCreateShaderUniform(ref command);


                

                // TODO: make pipelines for this purpose, extension not widely available
                //vk.CmdSetPrimitiveTopology(renderCommands, primitiveType.Item1);
                //vk.CmdSetPrimitiveRestartEnable(renderCommands, primitiveType.Item2);



                // TODO: We use the same buffers for all commands in a renderable, so we can skip rebinding every loop
                var (indexBuffer, vertexBuffer) = this.models[commands[i].VaoHandle];

                if (indexBuffer == null || vertexBuffer == null)
                    return;

                vertexBuffers[0] = vertexBuffer;
                offsets[0] = (uint)command.VertexBase * (uint)sizeof(VertexFormat);

                vk.CmdBindVertexBuffers(renderCommands, 0, 1, vertexBuffers, offsets);

                vk.CmdBindIndexBuffer(renderCommands, indexBuffer, 0, IndexType.Uint32);

                vk.CmdDrawIndexed(renderCommands, (uint)command.IndiciesCount, 1, (uint)command.IndexBase, 0, 0);
            }
        }

        public void EndFrame()
        {
            this.nextTransformIndex = 0;

            vk.CmdEndRenderPass(renderCommands);

            SUCCESS(vk.EndCommandBuffer(renderCommands), "Failed to record command buffer");

            var waitSems = stackalloc Semaphore[] { imageAvailableSemaphore };
            var waitStages = stackalloc PipelineStageFlags[] { PipelineStageFlags.PipelineStageColorAttachmentOutputBit };

            var signalSems = stackalloc Semaphore[] { renderFinishedSemaphore };

            var buf = renderCommands;
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

        private void UpdateGlobals(uint imageIndex, GlobalUniform globals)
        {
            this.globalBuffer/*[imageIndex]*/.LoadFull(globals);
        }

        private void RecreateSwapchain()
        {
            // TODO: spin render thread if width/height are zero, invalid framebuffer for vulkan (happens when minimized)

            recreateSwapchain = false;
            vk.DeviceWaitIdle(device);

            foreach(var (_,p) in this.pipelines)
                p.DestroyResources();

            this.swapchain.DestroyResources();
            this.swapchain.CreateResources();
            this.renderpass.InitializeFramebuffers();

            foreach (var (_, p) in this.pipelines)
                p.CreateResources();

            // TODO: multiple frame support: recreate uniform buffers, command buffers, pools??
        }

        public void SetSunLight(Vector3 sunDirection)
        {
        }

        public int UploadModel(Model<BitmapTag> model, out DrawCommand[] meshCommands)
        {
            var handle = Interlocked.Increment(ref nextModelHandle);

            var vertCount = model.Meshes.Sum(m => m.Verticies.Length);
            var indxCount = model.Meshes.Sum(m => m.Indicies.Length);

            meshCommands = new DrawCommand[model.Meshes.Length];
            var vertices = new VertexFormat[vertCount];
            var indices = new int[indxCount];

            var currentVert = 0;
            var currentIndx = 0;

            for (var i = 0; i < model.Meshes.Length; i++)
            {
                var mesh = model.Meshes[i];

                var command = new DrawCommand(mesh)
                {
                    VaoHandle = handle,
                    IndexBase = currentIndx,
                    VertexBase = currentVert,
                    ColorChangeData = model.ColorChangeData
                };

                Array.Copy(mesh.Verticies, 0, vertices, currentVert, mesh.Verticies.Length);
                currentVert += mesh.Verticies.Length;

                Array.Copy(mesh.Indicies, 0, indices, currentIndx, mesh.Indicies.Length);
                currentIndx += mesh.Indicies.Length;

                meshCommands[i] = command;
            }

            if (vertCount == 0 || indxCount == 0)
            {
                return handle;
            }

            using var vertStage = device.CreateBuffer<VertexFormat>(vertices.Length,
                BufferUsageFlags.BufferUsageTransferSrcBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit | MemoryPropertyFlags.MemoryPropertyHostCoherentBit);
            
            vertStage.LoadFull(vertices);

            var vertexBuffer = device.CreateBuffer<VertexFormat>(vertices.Length,
                BufferUsageFlags.BufferUsageVertexBufferBit | BufferUsageFlags.BufferUsageTransferDstBit,
                MemoryPropertyFlags.MemoryPropertyDeviceLocalBit);

            vertexBuffer.QueueLoad(vertStage);


            using var indexStage = device.CreateBuffer<int>(indices.Length,
                BufferUsageFlags.BufferUsageTransferSrcBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit | MemoryPropertyFlags.MemoryPropertyHostCoherentBit);
            
            indexStage.LoadFull(indices);

            var indexBuffer = device.CreateBuffer<int>(indices.Length,
                BufferUsageFlags.BufferUsageIndexBufferBit | BufferUsageFlags.BufferUsageTransferDstBit,
                MemoryPropertyFlags.MemoryPropertyDeviceLocalBit);

            indexBuffer.QueueLoad(indexStage);

            this.models[handle] = (indexBuffer, vertexBuffer);
            return handle;
        }

        public void UseShader(Shader shader)
        {
            // TODO: lookup and bind appropriate pipeline
            this.currentShader = shader;
        }

        public void UseTransform(Matrix4x4 transform)
        {
            var success = Matrix4x4.Invert(transform, out var inverted);
            Debug.Assert(success);

            this.currentTransform = new TransformUniform(transform, inverted);
        }

        public void Dispose()
        {
            vk.DeviceWaitIdle(device);

            for(var i = 0; i < nextModelHandle; i++)
            {
                if(models[i] != default)
                {
                    models[i].indices.Dispose();
                    models[i].vertices.Dispose();
                }
            }

            this.globalBuffer.Dispose();
            this.transformBuffer.Dispose();

            vk.DestroySemaphore(device, imageAvailableSemaphore, null);
            vk.DestroySemaphore(device, renderFinishedSemaphore, null);
            vk.DestroyFence(device, inFlightFence, null);

            this.renderpass.Dispose();
            this.swapchain.Dispose();

            this.image.Dispose();
            this.sampler.Dispose();

            // destroy pipelines
            foreach (var (_, p) in this.pipelines)
                p.Dispose();

            // destroy device
            this.device.Dispose();

            // destroy instance
            this.instance.Dispose();
        }
    }
}
