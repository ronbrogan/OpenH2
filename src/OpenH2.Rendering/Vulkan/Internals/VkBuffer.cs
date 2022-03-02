using Silk.NET.Vulkan;
using System;

namespace OpenH2.Rendering.Vulkan.Internals
{
    internal unsafe class VkBuffer<T> : VkObject, IDisposable where T : unmanaged
    {
        private readonly VkDevice device;
        private readonly ulong memorySize;

        private Silk.NET.Vulkan.Buffer buffer;
        private DeviceMemory bufferMemory;

        private void* bufferPtr = null;

        public static VkBuffer<T> CreateDynamicUniformBuffer(VkDevice device, int count, BufferUsageFlags usage, MemoryPropertyFlags memoryProperties)
        {
            var bytes = (ulong)(device.AlignUboItem<T>(1) * count);
            return new VkBuffer<T>(device, bytes, usage, memoryProperties);
        }

        public static VkBuffer<T> CreatePacked(VkDevice device, int count, BufferUsageFlags usage, MemoryPropertyFlags memoryProperties)
        {
            var bytes = (ulong)(sizeof(T) * count);
            return new VkBuffer<T>(device, bytes, usage, memoryProperties);
        }

        private VkBuffer(VkDevice device, ulong bytes, BufferUsageFlags usage, MemoryPropertyFlags memoryProperties) : base(device.vk)
        {
            this.device = device;
            memorySize = bytes;

            var bufCreate = new BufferCreateInfo
            {
                SType = StructureType.BufferCreateInfo,
                Size = bytes,
                Usage = usage,
                SharingMode = SharingMode.Exclusive
            };

            SUCCESS(vk.CreateBuffer(device, in bufCreate, null, out buffer), "Buffer create failed");

            vk.GetBufferMemoryRequirements(device, buffer, out var memReq);

            var vertAlloc = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = memReq.Size,
                MemoryTypeIndex = device.FindMemoryType(memReq.MemoryTypeBits, memoryProperties)
            };

            SUCCESS(vk.AllocateMemory(device, in vertAlloc, null, out bufferMemory), "Unable to allocate buffer memory");
            vk.BindBufferMemory(device, buffer, bufferMemory, 0);
        }

        public static implicit operator Silk.NET.Vulkan.Buffer(VkBuffer<T> @this) => @this.buffer;

        public void Dispose()
        {
            vk.DestroyBuffer(device, buffer, null);
            vk.FreeMemory(device, bufferMemory, null);
        }

        public void LoadFull(ReadOnlySpan<T> items)
        {
            void* data = null;
            vk.MapMemory(device, bufferMemory, 0, memorySize, 0, ref data);
            fixed (T* itemsPtr = items)
                System.Buffer.MemoryCopy(itemsPtr, data, memorySize, memorySize);
            vk.UnmapMemory(device, bufferMemory);
        }

        public void LoadFull(T item)
        {
            void* data = null;
            vk.MapMemory(device, bufferMemory, 0, memorySize, 0, ref data);
            System.Buffer.MemoryCopy(&item, data, memorySize, memorySize);
            vk.UnmapMemory(device, bufferMemory);
        }

        public void Map()
        {
            vk.MapMemory(device, bufferMemory, 0, memorySize, 0, ref bufferPtr);
        }

        public void LoadMapped(int xformOffset, T item)
        {
            if (bufferPtr == null) throw new Exception("Buffer was not mapped already");

            System.Buffer.MemoryCopy(&item, (byte*)bufferPtr + xformOffset, (long)memorySize - xformOffset, sizeof(T));
        }

        public void Unmap()
        {
            vk.UnmapMemory(device, bufferMemory);
            this.bufferPtr = null;
        }

        public void QueueLoad(VkBuffer<T> source)
        {
            device.OneShotCommand(c =>
            {
                var copy = new BufferCopy
                {
                    SrcOffset = 0,
                    DstOffset = 0,
                    Size = memorySize
                };

                vk.CmdCopyBuffer(c, source.buffer, buffer, 1, in copy);
            });
        }
    }
}
