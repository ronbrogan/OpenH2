using Silk.NET.Vulkan;
using System;

namespace OpenH2.Rendering.Vulkan.Internals
{
    internal unsafe sealed class VkImageArray : VkObject, IDisposable
    {
        private readonly VkDevice device;
        private readonly uint width;
        private readonly uint height;
        private readonly uint layers;
        private readonly uint mips;
        private readonly Format format;
        private readonly ImageAspectFlags aspectFlags;

        private Image image;
        private DeviceMemory memory;

        public ImageView View { get; private set; }

        public const ImageUsageFlags TransferUsage = ImageUsageFlags.ImageUsageTransferDstBit | ImageUsageFlags.ImageUsageSampledBit;
        public const ImageAspectFlags ColorAspect = ImageAspectFlags.ImageAspectColorBit;

        public VkImageArray(VkDevice device, Extent3D dims, Format format, ImageUsageFlags usage = TransferUsage, ImageAspectFlags aspectFlags = ColorAspect, ImageTiling tiling = ImageTiling.Optimal, bool generateMips = false)
             : base(device.vk)
        {
            this.device = device;
            this.width = dims.Width;
            this.height = dims.Height;
            this.layers = dims.Depth;
            this.mips = 1;
            this.format = format;
            this.aspectFlags = aspectFlags;

            var imageCreate = new ImageCreateInfo
            {
                SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.ImageType2D,
                Extent = new Extent3D(width, height, 1),
                MipLevels = this.mips,
                ArrayLayers = this.layers,
                Format = format,
                Tiling = tiling,
                InitialLayout = ImageLayout.Undefined,
                Usage = usage,
                SharingMode = SharingMode.Exclusive,
                Samples = SampleCountFlags.SampleCount1Bit,
                Flags = 0,
            };

            SUCCESS(vk.CreateImage(device, in imageCreate, null, out image), "Image create failed");

            vk.GetImageMemoryRequirements(device, image, out var reqs);

            var allocateInfo = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = reqs.Size,
                MemoryTypeIndex = device.FindMemoryType(reqs.MemoryTypeBits, MemoryPropertyFlags.MemoryPropertyDeviceLocalBit)
            };

            SUCCESS(vk.AllocateMemory(device, in allocateInfo, null, out memory), "Image allocation failed");

            SUCCESS(vk.BindImageMemory(device, image, memory, 0));

            View = CreateView(device, image, format, aspectFlags, mips, layers);
        }

        public VkSampler CreateSampler()
        {
            return new VkSampler(device, (int)mips, SamplerAddressMode.ClampToEdge);
        }

        private static ImageView CreateView(VkDevice device, Image image, Format format, ImageAspectFlags aspectFlags, uint mipMaps, uint layers)
        {
            var viewCreate = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = image,
                ViewType = ImageViewType.ImageViewType2DArray,
                Format = format,
                SubresourceRange = new ImageSubresourceRange(aspectFlags, 0, mipMaps, 0, layers),
            };

            SUCCESS(device.vk.CreateImageView(device, in viewCreate, null, out var view), "ImageView creation failed");
            return view;
        }

        public static implicit operator Image(VkImageArray @this) => @this.image;

        public void Dispose()
        {
            vk.DestroyImageView(device, View, null);
            vk.DestroyImage(device, image, null);
            vk.FreeMemory(device, memory, null);
        }
    }
}
