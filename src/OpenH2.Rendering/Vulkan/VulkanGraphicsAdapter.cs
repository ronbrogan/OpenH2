using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Shaders;
using OpenH2.Rendering.Shaders.Generic;
using OpenH2.Rendering.Shaders.ShadowMapping;
using OpenH2.Rendering.Shaders.Skybox;
using OpenH2.Rendering.Shaders.Wireframe;
using OpenH2.Rendering.Vulkan.Internals;
using Silk.NET.Vulkan;
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

        private Internals.MainRenderPass renderpass;
        private ShadowMapPass shadowpass;
        private PipelineStore pipelineStore;

        private Dictionary<(Shader, MeshElementType, Material<BitmapTag>), DescriptorSet> descriptorSets = new();
        private Dictionary<IMaterial<BitmapTag>, MaterialBindings> boundMaterials = new();
        private Dictionary<IMaterial<BitmapTag>, int[]> materialUniforms = new ();
        private TextureSet textures;

        private object currentPass = null;

        private bool recreateSwapchain = false;


        private int nextModelHandle = 0;
        private (VkBuffer<int> indices, VkBuffer<VertexFormat> vertices)[] models = new (VkBuffer<int> indices, VkBuffer<VertexFormat> vertices)[4096];


        // TODO: need multiple of these to support multiple in-flight frames
        private VkBuffer<GlobalUniform> globalBuffer;

        private VkBuffer[] shaderUniformBuffers = new VkBuffer[(int)Shader.MAX_VALUE];
        private int[] shaderUniformBufferOffsets = new int[(int)Shader.MAX_VALUE];

        private int nextTransformIndex = 0;
        private VkBuffer<TransformUniform> transformBuffer;
        private CommandBuffer renderCommands;
        private Semaphore imageAvailableSemaphore;
        private Semaphore renderFinishedSemaphore;
        private Fence inFlightFence;

        public VulkanGraphicsAdapter(VulkanHost vulkanHost) : base(vulkanHost.vk, null)
        {
            this.vulkanHost = vulkanHost;

            vulkanHost.window.Resize += (s) => recreateSwapchain = true;

            this.instance = new VkInstance(vulkanHost);
            this.device = instance.CreateDevice();

            this.textures = new TextureSet(this.device);
            this.textureBinder = new VulkanTextureBinder(this.device, this.textures);

            this.swapchain = device.CreateSwapchain();
            this.renderpass = new Internals.MainRenderPass(device, swapchain);
            this.shadowpass = new ShadowMapPass(device);
            this.pipelineStore = new PipelineStore(this.device, this.swapchain, this.renderpass, this.textures, this.shadowpass);

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

            this.shaderUniformBuffers[(int)Shader.Wireframe] = device.CreateUboAligned<WireframeUniform>(16384,
                BufferUsageFlags.BufferUsageUniformBufferBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit | MemoryPropertyFlags.MemoryPropertyHostCoherentBit);

            this.shaderUniformBuffers[(int)Shader.ShadowMapping] = device.CreateUboAligned<ShadowMappingUniform>(1,
                BufferUsageFlags.BufferUsageUniformBufferBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit | MemoryPropertyFlags.MemoryPropertyHostCoherentBit);

            this.shaderUniformBuffers[(int)Shader.Generic].Map();
            this.shaderUniformBuffers[(int)Shader.Skybox].Map();
            this.shaderUniformBuffers[(int)Shader.Wireframe].Map();
            this.shaderUniformBuffers[(int)Shader.ShadowMapping].Map();

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
            this.currentDescriptorSet = ulong.MaxValue;
            this.lastXformOffset = ulong.MaxValue;
            this.currentPass = null;

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
        }

        GeneralGraphicsPipeline currentPipeline = null;
        ulong currentDescriptorSet = ulong.MaxValue;
        int currentModel = -1;
        ulong lastXformOffset = ulong.MaxValue;
        public void DrawMeshes(DrawCommand[] commands)
        {
            if(this.currentShader == Shader.ShadowMapping && this.currentPass != this.shadowpass)
            {
                if (this.currentPass != null)
                {
                    vk.CmdEndRenderPass(this.renderCommands);
                }

                this.shadowpass.Begin(this.renderCommands);
                this.currentPass = this.shadowpass;
            }

            if(this.currentShader != Shader.ShadowMapping && this.currentPass != this.renderpass)
            {
                if(this.currentPass != null)
                {
                    vk.CmdEndRenderPass(this.renderCommands);
                }

                this.renderpass.Begin(this.renderCommands, imageIndex);
                this.currentPass = this.renderpass;
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

                var (pipeline, descriptorSet) = GetOrCreatePipeline(ref command);

                if (pipeline == null)
                    continue;

                if(currentPipeline != pipeline)
                {
                    pipeline.Bind(renderCommands);
                    currentPipeline = pipeline;
                }
                
                pipeline.BindDescriptors(renderCommands, in descriptorSet, dynamics);

                var (indexBuffer, vertexBuffer) = this.models[command.VaoHandle];

                if (indexBuffer == null || vertexBuffer == null)
                    continue;

                // We use the same buffers for all commands in a renderable, so we can skip rebinding every loop
                if (i == 0 && currentModel != command.VaoHandle)
                {
                    vertexBuffers[0] = vertexBuffer;
                    offsets[0] = 0;

                    vk.CmdBindVertexBuffers(renderCommands, 0, 1, vertexBuffers, offsets);
                    vk.CmdBindIndexBuffer(renderCommands, indexBuffer, 0, IndexType.Uint32);

                    currentModel = command.VaoHandle;
                }

                vk.CmdDrawIndexed(renderCommands, (uint)command.IndiciesCount, 1, (uint)command.IndexBase, command.VertexBase, 0);
            }
        }

        public void EndFrame()
        {
            this.textures.EnsureUpdated();

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

            this.pipelineStore.DestroySwapchainResources();

            this.swapchain.DestroyResources();
            this.swapchain.CreateResources();
            this.renderpass.InitializeFramebuffers();

            this.pipelineStore.CreateSwapchainResources();

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
            this.currentShader = shader;
        }

        public void UseTransform(Matrix4x4 transform)
        {
            var success = Matrix4x4.Invert(transform, out var inverted);
            Debug.Assert(success);

            this.currentTransform = new TransformUniform(transform, inverted);
        }

        private (GeneralGraphicsPipeline, DescriptorSet) GetOrCreatePipeline(ref DrawCommand command)
        {
            var key = (currentShader, command.ElementType, command.Mesh.Material as Material<BitmapTag>);

            var pipeline = this.pipelineStore.GetOrCreate(currentShader, command.ElementType);
            
            if(this.descriptorSets.TryGetValue(key, out var descriptorSet))
            {
                return (pipeline, descriptorSet);
            }

            BindTexturesAndUniform(ref command);

            var slice = this.shaderUniformBuffers[(int)currentShader].Slice(command.ShaderUniformHandle[(int)currentShader]);
            descriptorSet = pipeline.CreateDescriptors(this.globalBuffer, this.transformBuffer, slice, this.shadowpass);

            this.descriptorSets[key] = descriptorSet;

            return (pipeline, descriptorSet);
        }


        private unsafe void BindTexturesAndUniform(ref DrawCommand command)
        {
            // If the uniform was already buffered, we'll just reuse that buffered uniform
            // Currently these material uniforms never change at runtime - if this changes
            // there will have to be some sort of invalidation to ensure they're updated
            if (command.ShaderUniformHandle[(int)this.currentShader] == -1)
            {
                if (materialUniforms.TryGetValue(command.Mesh.Material, out var uniforms) == false)
                {
                    uniforms = new int[(int)Shader.MAX_VALUE];
                    materialUniforms[command.Mesh.Material] = uniforms;
                }

                var bindings = SetupTextures(command.Mesh.Material);
                command.ShaderUniformHandle[(int)this.currentShader] = GenerateShaderUniform(in command, bindings);
                uniforms[(int)this.currentShader] = command.ShaderUniformHandle[(int)this.currentShader];
            }
        }


        private MaterialBindings SetupTextures(IMaterial<BitmapTag> material)
        {
            if (boundMaterials.TryGetValue(material, out var cached))
            {
                return cached;
            }

            var bindings = new MaterialBindings
            {
                DiffuseHandle = textureBinder.GetOrBind(material.DiffuseMap),
                Detail1Handle = textureBinder.GetOrBind(material.DetailMap1),
                Detail2Handle = textureBinder.GetOrBind(material.DetailMap2),
                ColorChangeHandle = textureBinder.GetOrBind(material.ColorChangeMask),
                AlphaHandle = textureBinder.GetOrBind(material.AlphaMap),
                EmissiveHandle = textureBinder.GetOrBind(material.EmissiveMap),
                NormalHandle = textureBinder.GetOrBind(material.NormalMap)
            };

            boundMaterials.Add(material, bindings);

            return bindings;
        }

        private int GenerateShaderUniform(in DrawCommand command, in MaterialBindings bindings)
        {
            var mesh = command.Mesh;
            var bufferIndex = 0;

            var buffer = this.shaderUniformBuffers[(int)currentShader];
            ref int offset = ref this.shaderUniformBufferOffsets[(int)currentShader];

            switch (currentShader)
            {
                case Shader.Skybox:
                    BindAndBufferShaderUniform(
                        buffer as VkBuffer<SkyboxUniform>,
                        ref offset,
                        new SkyboxUniform(mesh.Material, bindings),
                        out bufferIndex);
                    break;
                case Shader.Generic:
                    BindAndBufferShaderUniform(
                        buffer as VkBuffer<GenericUniform>,
                        ref offset,
                        new GenericUniform(mesh, command.ColorChangeData, bindings),
                        out bufferIndex);
                    break;
                case Shader.Wireframe:
                    BindAndBufferShaderUniform(
                        buffer as VkBuffer<WireframeUniform>,
                        ref offset,
                        new WireframeUniform(mesh.Material),
                        out bufferIndex);
                    break;
                case Shader.ShadowMapping:
                    break;
                case Shader.Pointviz:
                case Shader.TextureViewer:
                    break;
            }

            return bufferIndex;
        }

        private void BindAndBufferShaderUniform<T>(VkBuffer<T> buffer, ref int uniformIndex, in T uniform, out int index) where T: unmanaged
        {
            if (buffer == null)
                throw new Exception($"Shader buffer is not available for {currentShader}");

            index = Interlocked.Increment(ref uniformIndex);

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

            this.shadowpass.Dispose();
            this.renderpass.Dispose();
            this.swapchain.Dispose();

            this.pipelineStore.Dispose();

            this.textureBinder.Dispose();
            this.textures.Dispose();

            foreach(var buf in this.shaderUniformBuffers)
                buf?.Dispose();

            // destroy device
            this.device.Dispose();

            // destroy instance
            this.instance.Dispose();
        }
    }
}
