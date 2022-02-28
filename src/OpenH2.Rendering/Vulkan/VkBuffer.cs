using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenH2.Rendering.Vulkan
{
    internal unsafe class VkBuffer<T> : VkObject, IDisposable where T : unmanaged
    {
        private readonly VkDevice device;
        private readonly ulong memorySize;

        private Silk.NET.Vulkan.Buffer buffer;
        private DeviceMemory bufferMemory;

        public VkBuffer(VkDevice device, int count, BufferUsageFlags usage, MemoryPropertyFlags memoryProperties) : base(device.vk)
        {
            this.device = device;
            this.memorySize = (ulong)(sizeof(T) * count);

            var bufCreate = new BufferCreateInfo
            {
                SType = StructureType.BufferCreateInfo,
                Size = (ulong)(sizeof(T) * count),
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

        public void Load(ReadOnlySpan<T> items)
        {
            void* data = null;
            vk.MapMemory(device, bufferMemory, 0, memorySize, 0, ref data);
            fixed (T* itemsPtr = items)
                System.Buffer.MemoryCopy(itemsPtr, data, memorySize, memorySize);
            vk.UnmapMemory(device, bufferMemory);
        }

        public void Load(T item)
        {
            void* data = null;
            vk.MapMemory(device, bufferMemory, 0, memorySize, 0, ref data);
            System.Buffer.MemoryCopy(&item, data, memorySize, memorySize);
            vk.UnmapMemory(device, bufferMemory);
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

                vk.CmdCopyBuffer(c, source.buffer, this.buffer, 1, in copy);
            });
        }
    }
}
