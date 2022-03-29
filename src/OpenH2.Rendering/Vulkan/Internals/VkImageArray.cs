using Silk.NET.Vulkan;
using System;
using VMASharp;

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
        private Allocation memory;

        public ImageView View { get; private set; }

        public const ImageUsageFlags TransferUsage = ImageUsageFlags.ImageUsageTransferDstBit | ImageUsageFlags.ImageUsageSampledBit;
        public const ImageAspectFlags ColorAspect = ImageAspectFlags.ImageAspectColorBit;

        public VkImageArray(VkDevice device, Extent3D dims, Format format, ImageUsageFlags usage = TransferUsage, ImageAspectFlags aspectFlags = ColorAspect, ImageTiling tiling = ImageTiling.Optimal, bool generateMips = false)
             : base(device)
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

            this.memory = vma.AllocateMemoryForImage(image, new AllocationCreateInfo(requiredFlags: MemoryPropertyFlags.MemoryPropertyDeviceLocalBit), bindToImage: true);

            View = CreateView(device, image, format, aspectFlags, mips, layers);
        }

        public VkSampler CreateSampler()
        {
            return new VkSampler(device, (int)mips, SamplerAddressMode.ClampToBorder, BorderColor.FloatOpaqueWhite);
        }

        private static ImageView CreateView(VkDevice device, Image image, Format format, ImageAspectFlags aspectFlags, uint mipMaps, uint layers)
        {
            var viewCreate = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = image,
                ViewType = ImageViewType.ImageViewType2DArray,
                Format = format,
                SubresourceRange = new ImageSubresourceRange(aspectFlags, 0, mipMaps, 0, layers)
            };

            SUCCESS(device.vk.CreateImageView(device, in viewCreate, null, out var view), "ImageView creation failed");
            return view;
        }

        public static implicit operator Image(VkImageArray @this) => @this.image;

        public void Dispose()
        {
            vk.DestroyImageView(device, View, null);
            vk.DestroyImage(device, image, null);
            this.memory?.Dispose();
        }
    }
}
