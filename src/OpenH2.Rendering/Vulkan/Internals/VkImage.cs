using Silk.NET.Vulkan;
using System;

namespace OpenH2.Rendering.Vulkan.Internals
{
    internal unsafe sealed class VkImage : VkObject, IDisposable
    {
        private readonly VkDevice device;
        private readonly uint width;
        private readonly uint height;
        private readonly uint mips;
        private readonly Format format;
        private readonly ImageAspectFlags aspectFlags;

        private Image image;
        private DeviceMemory memory;

        public ImageView View { get; private set; }

        public const ImageUsageFlags TransferUsage = ImageUsageFlags.ImageUsageTransferDstBit | ImageUsageFlags.ImageUsageSampledBit | (ImageUsageFlags)FormatFeatureFlags2.FormatFeature2BlitDstBit;
        public const ImageAspectFlags ColorAspect = ImageAspectFlags.ImageAspectColorBit;

        public VkImage(VkDevice device, Extent2D dims, Format format, ImageUsageFlags usage = TransferUsage, ImageAspectFlags aspectFlags = ColorAspect, ImageTiling tiling = ImageTiling.Optimal, bool generateMips = false)
            : this(device, dims.Width, dims.Height, format, usage, aspectFlags, tiling, generateMips) { }

        public VkImage(VkDevice device, uint width, uint height, Format format, ImageUsageFlags usage = TransferUsage, ImageAspectFlags aspectFlags = ColorAspect, ImageTiling tiling = ImageTiling.Optimal, bool generateMips = false) : base(device.vk)
        {
            this.device = device;
            this.width = width;
            this.height = height;
            this.format = format;
            this.aspectFlags = aspectFlags;

            mips = generateMips
                ? CalculateMips(width, height)
                : 1;

            if (mips > 1)
            {
                // We'll need to blit to generate mips
                usage |= ImageUsageFlags.ImageUsageTransferSrcBit | ImageUsageFlags.ImageUsageTransferDstBit | (ImageUsageFlags)FormatFeatureFlags2.FormatFeature2BlitDstBit;
            }

            var imageCreate = new ImageCreateInfo
            {
                SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.ImageType2D,
                Extent = new Extent3D(width, height, 1),
                MipLevels = mips,
                ArrayLayers = 1,
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

            vk.BindImageMemory(device, image, memory, 0);
        }

        public VkSampler CreateSampler()
        {
            return new VkSampler(device, (int)mips);
        }

        private static uint CalculateMips(uint width, uint height)
        {
            return (uint)Math.Floor(Math.Log2(Math.Max(width, height))) + 1;
        }

        public void TransitionLayout(ImageLayout oldLayout, ImageLayout newLayout)
        {
            device.OneShotCommand(commandBuffer =>
            {
                TransitionLayout(commandBuffer, oldLayout, newLayout);
            });
        }

        public void TransitionLayout(CommandBuffer commandBuffer, ImageLayout oldLayout, ImageLayout newLayout)
        {
            var barrier = new ImageMemoryBarrier
            {
                SType = StructureType.ImageMemoryBarrier,
                OldLayout = oldLayout,
                NewLayout = newLayout,

                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
                DstQueueFamilyIndex = Vk.QueueFamilyIgnored,

                Image = image,
                SubresourceRange = new ImageSubresourceRange(aspectFlags, 0, mips, 0, 1),
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
        }

        // TODO: make implicit with image creation?
        public void CreateView()
        {
            var defaultSwizzle = new ComponentMapping(ComponentSwizzle.R, ComponentSwizzle.G, ComponentSwizzle.B, ComponentSwizzle.A);

            View = CreateView(device, image, format, aspectFlags, defaultSwizzle, mips);
        }

        public void CreateView(ComponentMapping swizzle)
        {
            View = CreateView(device, image, format, aspectFlags, swizzle, mips);
        }

        public static ImageView CreateView(VkDevice device, Image image, Format format, ImageAspectFlags aspectFlags, ComponentMapping swizzle, uint mipMaps)
        {
            var viewCreate = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = image,
                ViewType = ImageViewType.ImageViewType2D,
                Format = format,
                SubresourceRange = new ImageSubresourceRange(aspectFlags, 0, mipMaps, 0, 1),
                Components = swizzle
            };

            SUCCESS(device.vk.CreateImageView(device, in viewCreate, null, out var view), "ImageView creation failed");
            return view;
        }

        public void QueueLoad(VkBuffer<byte> source)
        {
            device.OneShotCommand(commandBuffer =>
            {
                TransitionLayout(commandBuffer, ImageLayout.Undefined, ImageLayout.TransferDstOptimal);

                var copy = new BufferImageCopy
                {
                    BufferOffset = 0,
                    BufferRowLength = 0,
                    BufferImageHeight = 0,

                    ImageSubresource = new ImageSubresourceLayers(aspectFlags, 0, 0, 1),
                    ImageOffset = new Offset3D(0, 0, 0),
                    ImageExtent = new Extent3D(width, height, 1)
                };

                vk.CmdCopyBufferToImage(commandBuffer, source, image, ImageLayout.TransferDstOptimal, 1, in copy);

                if (mips > 1)
                    GenerateMipmaps(commandBuffer);
                else
                    TransitionLayout(commandBuffer, ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal);
            });
        }

        private void GenerateMipmaps(CommandBuffer commandBuffer)
        {
            // TODO: Check if the current format supports linear filtering via
            //       GetPhysicalDeviceFormatProperties.OptimalTilingFeatures & FormatFeatureFlags.FormatFeatureSampledImageFilterLinearBit
            //       We should fallback to another format or software blitting


            var barrier = new ImageMemoryBarrier
            {
                SType = StructureType.ImageMemoryBarrier,
                Image = image,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
                DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                SubresourceRange = new ImageSubresourceRange(aspectFlags, 0, 1, 0, 1)
            };

            var mipWidth = (int)width;
            var mipHeight = (int)height;

            for (var i = 1; i < mips; i++)
            {
                barrier.SubresourceRange.BaseMipLevel = (uint)(i - 1);
                barrier.OldLayout = ImageLayout.TransferDstOptimal;
                barrier.NewLayout = ImageLayout.TransferSrcOptimal;
                barrier.SrcAccessMask = AccessFlags.AccessTransferWriteBit;
                barrier.DstAccessMask = AccessFlags.AccessTransferReadBit;

                vk.CmdPipelineBarrier(commandBuffer,
                    PipelineStageFlags.PipelineStageTransferBit, PipelineStageFlags.PipelineStageTransferBit, 0,
                    0, null,
                    0, null,
                    1, barrier);

                var blit = new ImageBlit
                {
                    SrcOffsets = new() { Element0 = new(0, 0, 0), Element1 = new(mipWidth, mipHeight, 1) },
                    SrcSubresource = new ImageSubresourceLayers(aspectFlags, (uint)i - 1, 0, 1),
                    DstOffsets = new() { Element0 = new(0, 0, 0), Element1 = new(mipWidth > 1 ? mipWidth / 2 : 1, mipHeight > 1 ? mipHeight / 2 : 1, 1) },
                    DstSubresource = new ImageSubresourceLayers(aspectFlags, (uint)i, 0, 1)
                };

                // WARNING: command buffer must have graphics capability for blitting
                vk.CmdBlitImage(commandBuffer,
                    image, ImageLayout.TransferSrcOptimal,
                    image, ImageLayout.TransferDstOptimal,
                    1, in blit, Filter.Linear);

                barrier.OldLayout = ImageLayout.TransferSrcOptimal;
                barrier.NewLayout = ImageLayout.ShaderReadOnlyOptimal;
                barrier.SrcAccessMask = AccessFlags.AccessTransferReadBit;
                barrier.DstAccessMask = AccessFlags.AccessShaderReadBit;

                vk.CmdPipelineBarrier(commandBuffer,
                    PipelineStageFlags.PipelineStageTransferBit, PipelineStageFlags.PipelineStageFragmentShaderBit, 0,
                    0, null,
                    0, null,
                    1, barrier);

                if (mipWidth > 1) mipWidth /= 2;
                if (mipHeight > 1) mipHeight /= 2;
            }

            // Barrier on last mip's transition
            barrier.SubresourceRange.BaseMipLevel = mips - 1;
            barrier.OldLayout = ImageLayout.TransferDstOptimal;
            barrier.NewLayout = ImageLayout.ShaderReadOnlyOptimal;
            barrier.SrcAccessMask = AccessFlags.AccessTransferWriteBit;
            barrier.DstAccessMask = AccessFlags.AccessShaderReadBit;

            vk.CmdPipelineBarrier(commandBuffer,
                PipelineStageFlags.PipelineStageTransferBit, PipelineStageFlags.PipelineStageFragmentShaderBit, 0,
                0, null,
                0, null,
                1, barrier);
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
