using Silk.NET.Vulkan;
using System;
using System.Diagnostics;

namespace OpenH2.Rendering.Vulkan.Internals
{
    internal unsafe struct VkBufferSlice
    {
        private int index;

        public VkBuffer Buffer { get; private set; }
        public ulong Start { get; private set; }
        public ulong Length { get; private set; }

        public VkBufferSlice(VkBuffer buffer, int index, ulong start, ulong length)
        {
            this.Buffer = buffer;
            this.index = index;
            this.Start = start;
            this.Length = length;
        }

        public DescriptorBufferInfo GetInfo()
        {
            return new DescriptorBufferInfo(this.Buffer, this.Start, this.Length);
        }

        public void Load<T>(T item) where T: unmanaged
        {
            this.Buffer.LoadMapped(item, Start, Length, flush: true);
        }
    }

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

    internal abstract unsafe class VkBuffer : VkObject, IDisposable
    {
        protected readonly VkDevice device;
        protected readonly ulong memorySize;
        private readonly int itemSize;
        private readonly bool uboAlign;

        protected Silk.NET.Vulkan.Buffer buffer;
        protected DeviceMemory bufferMemory;
        protected void* bufferPtr = null;

        protected VkBuffer(VkDevice device, ulong bytes, BufferUsageFlags usage, MemoryPropertyFlags memoryProperties, int itemSize, bool uboAlign) : base(device.vk)
        {
            this.device = device;
            this.memorySize = bytes;
            this.itemSize = itemSize;
            this.uboAlign = uboAlign;

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

        public static implicit operator Silk.NET.Vulkan.Buffer(VkBuffer @this) => @this.buffer;

        public void Map()
        {
            if (this.bufferPtr != null)
                return;

            vk.MapMemory(device, bufferMemory, 0, memorySize, 0, ref bufferPtr);
        }

        public void Unmap()
        {
            if (this.bufferPtr == null)
                throw new Exception("Buffer was already unmapped");

            vk.UnmapMemory(device, bufferMemory);
            this.bufferPtr = null;
        }

        public void LoadMapped<T>(T item, ulong itemOffset, bool flush = false) where T:unmanaged
        {
            this.LoadMapped<T>(item, itemOffset, (ulong)sizeof(T), flush);
        }

        public void LoadMapped<T>(T item, ulong itemOffset, ulong itemSize, bool flush = false) where T: unmanaged
        {
            if (this.bufferPtr == null)
                throw new Exception("Buffer was not mapped already");

            System.Buffer.MemoryCopy(&item, (byte*)bufferPtr + itemOffset, (long)(memorySize - itemOffset), (long)itemSize);

            if (flush)
            {
                vk.FlushMappedMemoryRanges(device, 1, new MappedMemoryRange
                {
                    SType = StructureType.MappedMemoryRange,
                    Memory = this.bufferMemory,
                    Offset = itemOffset,
                    Size = itemSize
                });
            }
        }

        public VkBufferSlice Slice(int itemIndex)
        {
            var start = this.Align(itemIndex, (ulong)this.itemSize);
            var end = this.Align(itemIndex+1, (ulong)this.itemSize);

            return new VkBufferSlice(this, itemIndex, start, end-start);
        }

        public ulong Align(int index, ulong itemSize)
        {
            if (this.uboAlign)
                return this.device.AlignUboItem(itemSize, (ulong)index);

            return itemSize * (ulong)index;
        }

        public ulong Align<T>(int index) where T : unmanaged
        {
            if (this.uboAlign)
                return this.device.AlignUboItem<T>((ulong)index);

            return (ulong)sizeof(T) * (ulong)index;
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

    internal unsafe class VkBuffer<T> : VkBuffer where T : unmanaged
    {

        public static VkBuffer<T> CreateUboAligned(VkDevice device, ulong count, BufferUsageFlags usage, MemoryPropertyFlags memoryProperties)
        {
            var bytes = device.AlignUboItem<T>(1) * count;
            return new VkBuffer<T>(device, bytes, usage, memoryProperties, uboAlign: true);
        }

        public static VkBuffer<T> CreatePacked(VkDevice device, int count, BufferUsageFlags usage, MemoryPropertyFlags memoryProperties)
        {
            var bytes = (ulong)(sizeof(T) * count);
            return new VkBuffer<T>(device, bytes, usage, memoryProperties);
        }

        public VkBuffer(VkDevice device, ulong bytes, BufferUsageFlags usage, MemoryPropertyFlags memoryProperties, bool uboAlign = false) 
            : base(device, bytes, usage, memoryProperties, sizeof(T), uboAlign)
        {
        }

        public static implicit operator Silk.NET.Vulkan.Buffer(VkBuffer<T> @this) => @this.buffer;

        public VkBufferSlice<T> SliceRaw(ulong start, ulong length)
        {
            return new VkBufferSlice<T>(this, start, length);
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
