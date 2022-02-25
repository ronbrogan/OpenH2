using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenH2.Rendering.Vulkan
{
    public abstract class VkObject
    {
        public readonly Vk vk;

        public VkObject(Vk vkApi)
        {
            this.vk = vkApi;
        }

        protected void SUCCESS(Result result, string? description = null)
        {
            if (result != Result.Success)
            {
                throw new Exception(description ?? "Vulkan operation failed");
            }
        }

        protected void SUCCESS(bool result, string? description = null)
        {
            if (!result)
            {
                throw new Exception(description ?? "Vulkan operation failed");
            }
        }
    }
}
