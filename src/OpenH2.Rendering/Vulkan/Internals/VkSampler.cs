using Silk.NET.Vulkan;
using System;

namespace OpenH2.Rendering.Vulkan.Internals
{
    internal unsafe sealed class VkSampler : VkObject, IDisposable
    {
        private readonly VkDevice device;
        private Sampler sampler;

        public VkSampler(VkDevice device, int mipmapCount, SamplerAddressMode addressMode = SamplerAddressMode.Repeat) : base(device)
        {
            var samplerCreate = new SamplerCreateInfo
            {
                SType = StructureType.SamplerCreateInfo,
                MagFilter = Filter.Linear,
                MinFilter = Filter.Linear,
                AddressModeU = addressMode,
                AddressModeV = addressMode,
                AddressModeW = addressMode,
                AnisotropyEnable = true,
                MaxAnisotropy = device.PhysicalProperties.Limits.MaxSamplerAnisotropy,
                BorderColor = BorderColor.IntOpaqueWhite,
                UnnormalizedCoordinates = false,
                CompareEnable = false,
                CompareOp = CompareOp.Always,
                MipmapMode = SamplerMipmapMode.Linear,
                MipLodBias = 0,
                MinLod = 0,
                MaxLod = mipmapCount
            };

            SUCCESS(vk.CreateSampler(device, in samplerCreate, null, out sampler), "Sampler create failed");
            this.device = device;
        }

        public static implicit operator Sampler(VkSampler @this) => @this.sampler;

        public void Dispose()
        {
            vk.DestroySampler(device, sampler, null);
        }
    }
}
