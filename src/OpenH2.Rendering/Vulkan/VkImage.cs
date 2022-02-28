using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenH2.Rendering.Vulkan
{
    internal unsafe sealed class VkImage : VkObject, IDisposable
    {
        private readonly VkDevice device;
        private readonly uint width;
        private readonly uint height;
        private readonly Format format;
        private readonly ImageAspectFlags aspectFlags;

        private Image image;
        private DeviceMemory memory;

        public ImageView View { get; private set; }

        public const ImageUsageFlags TransferUsage = ImageUsageFlags.ImageUsageTransferDstBit | ImageUsageFlags.ImageUsageSampledBit;
        public const ImageAspectFlags ColorAspect = ImageAspectFlags.ImageAspectColorBit;

        public VkImage(VkDevice device, Extent2D dims, Format format, ImageUsageFlags usage = TransferUsage, ImageAspectFlags aspectFlags = ColorAspect)
            : this(device, dims.Width, dims.Height, format, usage, aspectFlags) { }

        public VkImage(VkDevice device, uint width, uint height, Format format, ImageUsageFlags usage = TransferUsage, ImageAspectFlags aspectFlags = ColorAspect) : base(device.vk)
        {
            this.device = device;
            this.width = width;
            this.height = height;
            this.format = format;
            this.aspectFlags = aspectFlags;
            var imageCreate = new ImageCreateInfo
            {
                SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.ImageType2D,
                Extent = new Extent3D(width, height, 1),
                MipLevels = 1,
                ArrayLayers = 1,
                Format = format,
                Tiling = ImageTiling.Optimal,
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

            vk.BindImageMemory(device, image, memory, 0);
        }

        public void TransitionLayout(ImageLayout oldLayout, ImageLayout newLayout)
        {
            device.OneShotCommand(commandBuffer =>
            {
                var barrier = new ImageMemoryBarrier
                {
                    SType = StructureType.ImageMemoryBarrier,
                    OldLayout = oldLayout,
                    NewLayout = newLayout,

                    SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
                    DstQueueFamilyIndex = Vk.QueueFamilyIgnored,

                    Image = image,
                    SubresourceRange = new ImageSubresourceRange(aspectFlags, 0, 1, 0, 1),
                };

                PipelineStageFlags sourceStage;
                PipelineStageFlags destinationStage;

                if (oldLayout == ImageLayout.Undefined && newLayout == ImageLayout.TransferDstOptimal)
                {
                    barrier.SrcAccessMask = 0;
                    barrier.DstAccessMask = AccessFlags.AccessTransferWriteBit;

                    sourceStage = PipelineStageFlags.PipelineStageTopOfPipeBit;
                    destinationStage = PipelineStageFlags.PipelineStageTransferBit;
                }
                else if (oldLayout == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.ShaderReadOnlyOptimal)
                {
                    barrier.SrcAccessMask = AccessFlags.AccessTransferWriteBit;
                    barrier.DstAccessMask = AccessFlags.AccessShaderReadBit;

                    sourceStage = PipelineStageFlags.PipelineStageTransferBit;
                    destinationStage = PipelineStageFlags.PipelineStageFragmentShaderBit;
                }
                else
                {
                    throw new Exception($"Unsupported layout transition {oldLayout}->{newLayout}");
                }

                vk.CmdPipelineBarrier(commandBuffer,
                    sourceStage, destinationStage,
                    0,
                    0, null,
                    0, null,
                    1, in barrier);
            });
        }

        // TODO: make implicit with image creation?
        public void CreateView()
        {
            this.View = CreateView(device, image, format, aspectFlags);
        }

        public static ImageView CreateView(VkDevice device, Image image, Format format, ImageAspectFlags aspectFlags)
        {
            var viewCreate = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = image,
                ViewType = ImageViewType.ImageViewType2D,
                Format = format,
                SubresourceRange = new ImageSubresourceRange(aspectFlags, 0, 1, 0, 1)
            };

            SUCCESS(device.vk.CreateImageView(device, in viewCreate, null, out var view), "ImageView creation failed");
            return view;
        }

        public void QueueLoad(VkBuffer<byte> source)
        {
            device.OneShotCommand(commandBuffer =>
            {
                var copy = new BufferImageCopy
                {
                    BufferOffset = 0,
                    BufferRowLength = 0,
                    BufferImageHeight = 0,

                    ImageSubresource = new ImageSubresourceLayers(aspectFlags, 0, 0, 1),
                    ImageOffset = new Offset3D(0,0,0),
                    ImageExtent = new Extent3D(width, height, 1)
                };

                vk.CmdCopyBufferToImage(commandBuffer, source, image, ImageLayout.TransferDstOptimal, 1, in copy);
            });
        }

        public static implicit operator Image(VkImage @this) => @this.image;

        public void Dispose()
        {
            vk.DestroyImageView(device, View, null);
            vk.DestroyImage(device, image, null);
            vk.FreeMemory(device, memory, null);
        }
    }
}
