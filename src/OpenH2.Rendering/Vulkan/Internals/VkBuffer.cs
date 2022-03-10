using Silk.NET.Vulkan;
using System;

namespace OpenH2.Rendering.Vulkan.Internals
{
    internal unsafe struct VkBufferSlice<T> where T : unmanaged
    {
        private int index;

        public VkBuffer<T> Buffer { get; private set; }
        public ulong Start { get; private set; }
        public ulong Length { get; private set; }

        public VkBufferSlice(VkBuffer<T> buffer, ulong start, ulong length)
        {
            Buffer = buffer;
            this.index = (int)(start / (ulong)sizeof(T));
            Start = start;
            Length = length;
        }

        public VkBufferSlice(VkBuffer<T> buffer, int index)
        {
            this.Buffer = buffer;
            this.index = index;
            this.Start = (ulong)sizeof(T) * (ulong)index;
            this.Length = (ulong)sizeof(T);
        }

        public DescriptorBufferInfo GetInfo()
        {
            return new DescriptorBufferInfo(this.Buffer, this.Start, this.Length);
        }

        public void Load(T item)
        {
            this.Buffer.LoadMapped((ulong)(this.index * sizeof(T)), item, flush: true);
        }
    }

    internal unsafe class VkBuffer<T> : VkObject, IDisposable where T : unmanaged
    {
        private readonly VkDevice device;
        private readonly ulong memorySize;

        private Silk.NET.Vulkan.Buffer buffer;
        private DeviceMemory bufferMemory;

        private void* bufferPtr = null;

        public static VkBuffer<T> CreateDynamicUniformBuffer(VkDevice device, ulong count, BufferUsageFlags usage, MemoryPropertyFlags memoryProperties)
        {
            var bytes = device.AlignUboItem<T>(1) * count;
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

        public VkBufferSlice<T> SliceRaw(ulong start, ulong length)
        {
            return new VkBufferSlice<T>(this, start, length);
        }

        public VkBufferSlice<T> Slice(int itemIndex)
        {
            return new VkBufferSlice<T>(this, itemIndex);
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

        public void Load(ulong itemOffset, T item)
        {
            void* data = null;
            vk.MapMemory(device, bufferMemory, 0, memorySize, 0, ref data);
            System.Buffer.MemoryCopy(&item, (byte*)bufferPtr + itemOffset, (long)(memorySize - itemOffset), sizeof(T));
            vk.UnmapMemory(device, bufferMemory);
        }

        public void Map()
        {
            if (this.bufferPtr != null) 
                return;

            vk.MapMemory(device, bufferMemory, 0, memorySize, 0, ref bufferPtr);
        }

        public void LoadMapped(ulong itemOffset, T item, bool flush = false)
        {
            if (this.bufferPtr == null) 
                throw new Exception("Buffer was not mapped already");

            System.Buffer.MemoryCopy(&item, (byte*)bufferPtr + itemOffset, (long)(memorySize - itemOffset), sizeof(T));

            if(flush)
            {
                vk.FlushMappedMemoryRanges(device, 1, new MappedMemoryRange
                {
                    SType = StructureType.MappedMemoryRange,
                    Memory = this.bufferMemory,
                    Offset = itemOffset,
                    Size = (ulong)sizeof(T)
                });
            }
        }

        public void Unmap()
        {
            if (this.bufferPtr == null)
                throw new Exception("Buffer was already unmapped");

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

        public void Dispose()
        {
            if (this.bufferPtr != null)
            {
                this.Unmap();
            }

            vk.DestroyBuffer(device, buffer, null);
            vk.FreeMemory(device, bufferMemory, null);
        }
    }
}
