using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenH2.Rendering.Vulkan.Internals
{
    public abstract class VkObject
    {
        public readonly Vk vk;

        public VkObject(Vk vkApi)
        {
            vk = vkApi;
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
