using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenH2.Rendering.Vulkan
{
    internal unsafe class VkBuffer<T> : VkObject, IDisposable where T: unmanaged
    {
        private readonly VkDevice device;
        private readonly ulong memorySize;

        private Silk.NET.Vulkan.Buffer buffer;
        private DeviceMemory bufferMemory;

        public VkBuffer(VkDevice device, int count, BufferUsageFlags usage, MemoryPropertyFlags memoryProperties) : base(device.vk)
        {
            this.device = device;
            this.memorySize = (ulong)(sizeof(T) * count);

            var vertCreate = new BufferCreateInfo
            {
                SType = StructureType.BufferCreateInfo,
                Size = (ulong)(sizeof(T) * count),
                Usage = usage,
                SharingMode = SharingMode.Exclusive
            };

            SUCCESS(vk.CreateBuffer(device, in vertCreate, null, out buffer), "Vertex buffer create failed");

            vk.GetBufferMemoryRequirements(device, buffer, out var memReq);

            var vertAlloc = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = memReq.Size,
                MemoryTypeIndex = device.FindMemoryType(memReq.MemoryTypeBits, memoryProperties)
            };

            SUCCESS(vk.AllocateMemory(device, in vertAlloc, null, out bufferMemory), "Unable to allocate vertex buffer memory");
            vk.BindBufferMemory(device, buffer, bufferMemory, 0);
        }

        public static implicit operator Silk.NET.Vulkan.Buffer(VkBuffer<T> @this) => @this.buffer;

        public void Dispose()
        {
            vk.DestroyBuffer(device, buffer, null);
            vk.FreeMemory(device, bufferMemory, null);
        }

        internal void Load(T[] items)
        {
            void* data = null;
            vk.MapMemory(device, bufferMemory, 0, memorySize, 0, ref data);
            fixed (T* itemsPtr = items)
                System.Buffer.MemoryCopy(itemsPtr, data, memorySize, memorySize);
            vk.UnmapMemory(device, bufferMemory);
        }
    }
}
