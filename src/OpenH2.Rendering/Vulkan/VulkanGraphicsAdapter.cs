using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Shaders;
using OpenH2.Rendering.Shaders.Generic;
using OpenH2.Rendering.Shaders.ShadowMapping;
using OpenH2.Rendering.Shaders.Skybox;
using OpenH2.Rendering.Shaders.Wireframe;
using OpenH2.Rendering.Vulkan.Internals;
using OpenH2.Rendering.Vulkan.Internals.GraphicsPipelines;
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
    public sealed unsafe class VulkanGraphicsAdapter : VkObject, IGraphicsAdapter, IDisposable
    {
        const int MaxFramesInFlight = 2;

        private VulkanHost vulkanHost;
        private VkInstance instance;
        private VkDevice device;
        private VulkanTextureBinder textureBinder;
        private VkSwapchain swapchain;
        private VkRenderPass renderpass;
        
        private bool recreateSwapchain = false;

        private Dictionary<(Shader, MeshElementType, uint), BaseGraphicsPipeline> pipelines = new();

        private int nextModelHandle = 0;
        private (VkBuffer<int> indices, VkBuffer<VertexFormat> vertices)[] models = new (VkBuffer<int> indices, VkBuffer<VertexFormat> vertices)[4096];


        // TODO: need multiple of these to support multiple in-flight frames
        private VkBuffer<GlobalUniform> globalBuffer;

        private VkBuffer[] shaderUniformBuffers = new VkBuffer[(int)Shader.MAX_VALUE];

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

            this.globalBuffer = device.CreateBuffer<GlobalUniform>(1, 
                BufferUsageFlags.BufferUsageUniformBufferBit, 
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit | MemoryPropertyFlags.MemoryPropertyHostCoherentBit);

            // TODO: clean up per-shader uniform buffer management
            this.shaderUniformBuffers[(int)Shader.Generic] = device.CreateBuffer<GenericUniform>(16384,
                BufferUsageFlags.BufferUsageUniformBufferBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit | MemoryPropertyFlags.MemoryPropertyHostCoherentBit);

            this.shaderUniformBuffers[(int)Shader.Skybox] = device.CreateUboAligned<SkyboxUniform>(16384,
                BufferUsageFlags.BufferUsageUniformBufferBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit | MemoryPropertyFlags.MemoryPropertyHostCoherentBit);

            this.shaderUniformBuffers[(int)Shader.Generic].Map();
            this.shaderUniformBuffers[(int)Shader.Skybox].Map();

            this.transformBuffer = device.CreateUboAligned<TransformUniform>(16384,
                BufferUsageFlags.BufferUsageUniformBufferBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit | MemoryPropertyFlags.MemoryPropertyHostCoherentBit);

            // Leaving mapped so that we can write during render
            this.transformBuffer.Map();

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
            this.currentShader = Shader.MAX_VALUE;
            this.nextTransformIndex = 0;
            this.currentModel = -1;
            this.currentPipeline = null;
            this.lastXformOffset = ulong.MaxValue;

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

        private BaseGraphicsPipeline CreatePipeline(MeshElementType elementType)
        {
            if(this.currentShader == Shader.Generic)
            {
                return elementType switch
                {
                    MeshElementType.TriangleList => new GenericTriListPipeline(device, swapchain, renderpass),
                    MeshElementType.TriangleStrip => new GenericTriStripPipeline(device, swapchain, renderpass),
                    MeshElementType.TriangleStripDecal => new GenericTriStripPipeline(device, swapchain, renderpass),
                    MeshElementType.Point => null,
                    _ => null,
                };
            }
            else if(this.currentShader == Shader.Skybox)
            {
                return elementType switch
                {
                    MeshElementType.TriangleList => new SkyboxTriListPipeline(device, swapchain, renderpass),
                    MeshElementType.TriangleStrip => new SkyboxTriStripPipeline(device, swapchain, renderpass),
                    MeshElementType.TriangleStripDecal => new SkyboxTriStripPipeline(device, swapchain, renderpass),
                    MeshElementType.Point => null,
                    _ => null,
                };
            }

            return null;
        }

        BaseGraphicsPipeline currentPipeline = null;
        int currentModel = -1;
        ulong lastXformOffset = ulong.MaxValue;
        public void DrawMeshes(DrawCommand[] commands)
        {
            if(this.currentShader != Shader.Generic && this.currentShader != Shader.Skybox)
            {
                return;
            }

            if (commands.Length == 0)
                return;

            // Find properly aligned offset to write current transform into
            var xformIndex = (ulong)Interlocked.Increment(ref nextTransformIndex);
            //if (xformIndex >= 255) return;
            var xformOffset = device.AlignUboItem<TransformUniform>(xformIndex);
            // Write the current transform to that offset
            // TODO: do bounds check and re-allocate larger buffer, or somthing
            this.transformBuffer.LoadMapped(xformOffset, this.currentTransform);
            // Bind the descriptor with the offset
            Span<uint> dynamics = stackalloc uint[] { (uint)xformOffset };

            var vertexBuffers = stackalloc Buffer[1];
            var offsets = stackalloc ulong[] { 0 };
            for (var i = 0; i < commands.Length; i++)
            {
                ref DrawCommand command = ref commands[i];

                var pipeline = GetOrCreatePipeline(command);

                if (pipeline == null)
                    continue;

                if (currentPipeline != pipeline || lastXformOffset != xformOffset)
                {
                    pipeline.Bind(renderCommands);
                    pipeline.BindDescriptors(renderCommands, dynamics);
                }

                // We use the same buffers for all commands in a renderable, so we can skip rebinding every loop
                
                var (indexBuffer, vertexBuffer) = this.models[command.VaoHandle];

                if (indexBuffer == null || vertexBuffer == null)
                    continue;

                vertexBuffers[0] = vertexBuffer;
                offsets[0] = (uint)command.VertexBase * (uint)sizeof(VertexFormat);

                vk.CmdBindVertexBuffers(renderCommands, 0, 1, vertexBuffers, offsets);

                if (currentModel != command.VaoHandle)
                {
                    vk.CmdBindIndexBuffer(renderCommands, indexBuffer, 0, IndexType.Uint32);

                    currentModel = command.VaoHandle;
                }



                vk.CmdDrawIndexed(renderCommands, (uint)command.IndiciesCount, 1, (uint)command.IndexBase, 0, 0);
            }
        }

        public void EndFrame()
        {
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

            BaseGraphicsPipeline.DestroyAllPipelineResources();

            this.swapchain.DestroyResources();
            this.swapchain.CreateResources();
            this.renderpass.InitializeFramebuffers();

            BaseGraphicsPipeline.CreateAllPipelineResources();

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

        private BaseGraphicsPipeline GetOrCreatePipeline(DrawCommand command)
        {
            if (command.Mesh.Material.DiffuseMap == null)
                return null;

            var key = (currentShader, command.ElementType, command.Mesh.Material.DiffuseMap.Id);

            if (this.pipelines.TryGetValue(key, out var pipeline))
                return pipeline;

            pipeline = CreatePipeline(command.ElementType);

            if (pipeline == null)
                return null;

            var tex = textureBinder.GetOrBind(command.Mesh.Material.DiffuseMap);

            if (tex == default)
                return null;

            var textures = BindTexturesAndUniform(ref command);

            if(textures == null)
            {
                throw new Exception("No textures bound");
            }

            var slice = this.shaderUniformBuffers[(int)currentShader].Slice(command.ShaderUniformHandle[(int)currentShader]);
            pipeline.CreateDescriptors(this.globalBuffer, this.transformBuffer, slice, textures);

            this.pipelines[key] = pipeline;

            return pipeline;
        }

        private Dictionary<IMaterial<BitmapTag>, int[]> MaterialUniforms = new Dictionary<IMaterial<BitmapTag>, int[]>();

        private int currentlyBoundShaderUniform = -1;
        private unsafe (VkImage image, VkSampler sampler)[] BindTexturesAndUniform(ref DrawCommand command)
        {
            // If the uniform was already buffered, we'll just reuse that buffered uniform
            // Currently these material uniforms never change at runtime - if this changes
            // there will have to be some sort of invalidation to ensure they're updated
            if (command.ShaderUniformHandle[(int)this.currentShader] == -1)
            {
                if (MaterialUniforms.TryGetValue(command.Mesh.Material, out var uniforms) == false)
                {
                    uniforms = new int[(int)Shader.MAX_VALUE];
                    MaterialUniforms[command.Mesh.Material] = uniforms;
                }

                var (bindings, textures) = SetupTextures(command.Mesh.Material);
                command.ShaderUniformHandle[(int)this.currentShader] = GenerateShaderUniform(command, bindings);
                uniforms[(int)this.currentShader] = command.ShaderUniformHandle[(int)this.currentShader];
                return textures;
            }

            return null;
        }


        private Dictionary<IMaterial<BitmapTag>, (MaterialBindings, (VkImage image, VkSampler sampler)[])> boundMaterials = new ();
        private (MaterialBindings, (VkImage image, VkSampler sampler)[]) SetupTextures(IMaterial<BitmapTag> material)
        {
            if (boundMaterials.TryGetValue(material, out var cached))
            {
                return cached;
            }

            var bindings = new MaterialBindings();
            var textures = new (VkImage image, VkSampler sampler)[8];
            var texIndex = 0;

            if (material.DiffuseMap != null)
            {
                textures[texIndex] = textureBinder.GetOrBind(material.DiffuseMap);
                bindings.DiffuseHandle = texIndex++;
            }

            if (material.DetailMap1 != null)
            {
                textures[texIndex] = textureBinder.GetOrBind(material.DetailMap1);
                bindings.Detail1Handle = texIndex++;
            }

            if (material.DetailMap2 != null)
            {
                textures[texIndex] = textureBinder.GetOrBind(material.DetailMap2);
                bindings.Detail2Handle = texIndex++;
            }

            if (material.ColorChangeMask != null)
            {
                textures[texIndex] = textureBinder.GetOrBind(material.ColorChangeMask);
                bindings.ColorChangeHandle = texIndex++;
            }

            if (material.AlphaMap != null)
            {
                textures[texIndex] = textureBinder.GetOrBind(material.AlphaMap);
                bindings.AlphaHandle = texIndex++;
            }

            if (material.EmissiveMap != null)
            {
                textures[texIndex] = textureBinder.GetOrBind(material.EmissiveMap);
                bindings.EmissiveHandle = texIndex++;
            }

            if (material.NormalMap != null)
            {
                textures[texIndex] = textureBinder.GetOrBind(material.NormalMap);
                bindings.NormalHandle = texIndex++;
            }

            boundMaterials.Add(material, (bindings, textures));

            return (bindings, textures);
        }

        private int GenerateShaderUniform(DrawCommand command, MaterialBindings bindings)
        {
            var mesh = command.Mesh;
            var bufferIndex = 0;

            var buffer = this.shaderUniformBuffers[(int)currentShader];

            if (buffer == null)
                throw new Exception($"Shader buffer is not available for {currentShader}");

            switch (currentShader)
            {
                case Shader.Skybox:
                    BindAndBufferShaderUniform(
                        buffer as VkBuffer<SkyboxUniform>,
                        new SkyboxUniform(mesh.Material, bindings),
                        out bufferIndex);
                    break;
                case Shader.Generic:
                    BindAndBufferShaderUniform(
                        buffer as VkBuffer<GenericUniform>,
                        new GenericUniform(mesh, command.ColorChangeData, bindings),
                        out bufferIndex);
                    break;
                case Shader.Wireframe:
                    BindAndBufferShaderUniform(
                        null,
                        new WireframeUniform(mesh.Material),
                        out bufferIndex);
                    break;
                case Shader.ShadowMapping:
                    BindAndBufferShaderUniform(
                        null,
                        new ShadowMappingUniform(),
                        out bufferIndex);
                    break;
                case Shader.Pointviz:
                case Shader.TextureViewer:
                    break;
            }

            return bufferIndex;
        }

        private int nextUniformIndex = 0;
        private void BindAndBufferShaderUniform<T>(VkBuffer<T> buffer, in T uniform, out int index) where T: unmanaged
        {
            // TODO: index per buffer
            index = Interlocked.Increment(ref nextUniformIndex);

            buffer.Slice(index).Load(uniform);
        }

        public void Dispose()
        {
            vk.DeviceWaitIdle(device);

            for(var i = 0; i <= nextModelHandle; i++)
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

            // destroy pipelines
            foreach (var (_, p) in this.pipelines)
                p.Dispose();

            this.textureBinder.Dispose();

            foreach(var buf in this.shaderUniformBuffers)
                buf?.Dispose();

            // destroy device
            this.device.Dispose();

            // destroy instance
            this.instance.Dispose();
        }
    }
}
