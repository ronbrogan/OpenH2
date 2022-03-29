using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using Silk.NET.Vulkan;

namespace OpenH2.Rendering.Vulkan.Internals
{
    /// <summary>
    /// Holds all of the textures required for a scene. Surfaces a global DescriptorSet to bind to access textures
    /// </summary>
    internal unsafe class TextureSet : VkObject, IDisposable
    {
        private const int TextureCount = 16536;

        private readonly VkDevice device;
        private readonly DescriptorPool pool;
        private readonly DescriptorSetLayout layout;
        private readonly DescriptorSet descriptorSet;
        public DescriptorSet DescriptorSet => descriptorSet;
        public DescriptorSetLayout DescriptorSetLayout => layout;
        public int State => boundTextureSlot;

        private int boundTextureSlot = 0;
        private (VkImage image, VkSampler sampler)[] textures = new (VkImage image, VkSampler sampler)[TextureCount];
        private ConcurrentQueue<int> texturesToUpload = new();

        public TextureSet(VkDevice device) : base(device)
        {
            this.device = device;

            this.pool = CreateDescriptorPool();
            this.layout = CreateLayout();
            this.descriptorSet = CreateDescriptors(layout, pool);
        }

        public int AddTexture(VkImage image, VkSampler sampler)
        {
            var next = Interlocked.Increment(ref boundTextureSlot);

            textures[next] = (image, sampler);
            texturesToUpload.Enqueue(next);

            return next;
        }

        public void EnsureUpdated()
        {
            var toUpload = texturesToUpload.Count;

            var writeCount = 0;
            Span<WriteDescriptorSet> writes = stackalloc WriteDescriptorSet[toUpload];
            Span<DescriptorImageInfo> images = stackalloc DescriptorImageInfo[toUpload];

            while (writeCount < toUpload && texturesToUpload.TryDequeue(out var i))
            {
                images[writeCount] = new DescriptorImageInfo(textures[i].sampler, textures[i].image.View, ImageLayout.ShaderReadOnlyOptimal);

                writes[writeCount] = new WriteDescriptorSet
                {
                    SType = StructureType.WriteDescriptorSet,
                    DstSet = descriptorSet,
                    DstBinding = 3,
                    DstArrayElement = (uint)i,
                    DescriptorType = DescriptorType.CombinedImageSampler,
                    DescriptorCount = 1,
                    PBufferInfo = null,
                    PImageInfo = (DescriptorImageInfo*)Unsafe.AsPointer(ref images[writeCount]),
                    PTexelBufferView = null
                };

                writeCount++;
            }

            vk.UpdateDescriptorSets(device, writes, 0, null);
        }

        protected DescriptorPool CreateDescriptorPool()
        {
            var sizes = stackalloc[]
            {
                new DescriptorPoolSize(DescriptorType.CombinedImageSampler, TextureCount)
            };

            var createInfo = new DescriptorPoolCreateInfo
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = 1,
                PPoolSizes = sizes,
                MaxSets = 1u * TextureCount,
                Flags = DescriptorPoolCreateFlags.DescriptorPoolCreateUpdateAfterBindBit
            };

            SUCCESS(vk.CreateDescriptorPool(device, in createInfo, null, out var descriptorPool));
            return descriptorPool;
        }

        private DescriptorSetLayout CreateLayout()
        {
            var pBindings = stackalloc DescriptorSetLayoutBinding[1];
            var bindingFlags = stackalloc DescriptorBindingFlags[1];

            
            bindingFlags[0] = DescriptorBindingFlags.DescriptorBindingPartiallyBoundBit | DescriptorBindingFlags.DescriptorBindingUpdateAfterBindBit | DescriptorBindingFlags.DescriptorBindingVariableDescriptorCountBit;
            pBindings[0] = new DescriptorSetLayoutBinding
            {
                Binding = 3,
                DescriptorCount = TextureCount,
                DescriptorType = DescriptorType.CombinedImageSampler,
                StageFlags = ShaderStageFlags.ShaderStageAllGraphics
            };
            
            var flags = new DescriptorSetLayoutBindingFlagsCreateInfo
            {
                SType = StructureType.DescriptorSetLayoutBindingFlagsCreateInfo,
                BindingCount = 1,
                PBindingFlags = bindingFlags
            };

            var descCreate = new DescriptorSetLayoutCreateInfo
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = 1,
                PBindings = pBindings,
                PNext = &flags,
                Flags = DescriptorSetLayoutCreateFlags.DescriptorSetLayoutCreateUpdateAfterBindPoolBit
            };

            SUCCESS(vk.CreateDescriptorSetLayout(device, in descCreate, null, out var descriptorSetLayout), "Descriptor set layout create failed");
            return descriptorSetLayout;
        }

        public DescriptorSet CreateDescriptors(DescriptorSetLayout DescriptorSetLayout, DescriptorPool DescriptorPool)
        {
            var layouts = stackalloc[] { DescriptorSetLayout };

            var maxBind = TextureCount - 1u;
            var variableCount = new DescriptorSetVariableDescriptorCountAllocateInfo
            {
                SType = StructureType.DescriptorSetVariableDescriptorCountAllocateInfo,
                DescriptorSetCount = 1,
                PDescriptorCounts = &maxBind
            };

            var alloc = new DescriptorSetAllocateInfo
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = DescriptorPool,
                DescriptorSetCount = 1,
                PSetLayouts = layouts,
                PNext = &variableCount
            };

            SUCCESS(vk.AllocateDescriptorSets(device, in alloc, out var descriptorSet), "DescriptorSet allocate failed");

            return descriptorSet;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            vk.DestroyDescriptorSetLayout(device, layout, null);
            vk.DestroyDescriptorPool(device, pool, null);
        }

        ~TextureSet()
        {
            throw new Exception("Dispose not being called on this resource is an error!");
        }
    }
}
