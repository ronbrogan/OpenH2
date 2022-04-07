using Silk.NET.Vulkan;
using System;
using VMASharp;

namespace OpenH2.Rendering.Vulkan.Internals
{
    public abstract class VkObject
    {
        public readonly Vk vk;
        public readonly VulkanMemoryAllocator vma;

        public VkObject(Vk vk, VulkanMemoryAllocator vma)
        {
            this.vk = vk;
            this.vma = vma;
        }

        internal VkObject(VkDevice device)
        {
            this.vk = device.vk;
            this.vma = device.vma;
        }

        protected static void SUCCESS(Result result, string description = null)
        {
            if (result != Result.Success)
            {
                throw new Exception(description ?? "Vulkan operation failed");
            }
        }

        protected static void SUCCESS(bool result, string description = null)
        {
            if (!result)
            {
                throw new Exception(description ?? "Vulkan operation failed");
            }
        }
    }
}
