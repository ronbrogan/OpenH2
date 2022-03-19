using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenBlam.Core.Texturing;
using OpenH2.Core.Tags;
using OpenH2.Rendering.Vulkan.Internals;
using Silk.NET.Vulkan;

namespace OpenH2.Rendering.Vulkan
{
    internal class VulkanTextureBinder : IDisposable
    {
        private static Dictionary<TextureFormat, Format> FormatMappings = new Dictionary<TextureFormat, Format>
        {
            { TextureFormat.A8, Format.R8Unorm },
            { TextureFormat.L8, Format.R8Unorm },
            { TextureFormat.A8L8, Format.R8G8Unorm },
            { TextureFormat.U8V8, Format.R8G8Unorm },
            { TextureFormat.R5G6B5, Format.B5G6R5UnormPack16 },
            { TextureFormat.A4R4G4B4, Format.B4G4R4A4UnormPack16 },
            { TextureFormat.R8G8B8, Format.B8G8R8A8Unorm },
            { TextureFormat.A8R8G8B8, Format.B8G8R8A8Unorm },
            { TextureFormat.DXT1, Format.BC1RgbUnormBlock },
            { TextureFormat.DXT23, Format.BC2UnormBlock },
            { TextureFormat.DXT45, Format.BC3UnormBlock },
        };

        private static Dictionary<TextureFormat, Format> DecompressedFormatMappings = new Dictionary<TextureFormat, Format>
        {
            { TextureFormat.DXT1, Format.B8G8R8A8Unorm },
            { TextureFormat.DXT23, Format.B8G8R8A8Unorm },
            { TextureFormat.DXT45, Format.B8G8R8A8Unorm },
        };

        private Dictionary<uint, (VkImage, VkSampler)> UploadedTextures = new Dictionary<uint, (VkImage, VkSampler)>();
        private Dictionary<uint, int> BoundTextureIndexes = new Dictionary<uint, int>();

        private VkDevice device;
        private Vk vk;
        private TextureSet textures;

        public VulkanTextureBinder(VkDevice device, TextureSet textures)
        {
            this.device = device;
            this.vk = device.vk;
            this.textures = textures;

            this.device.UnboundTexture = this.CreateErrTexture();
        }

        public unsafe VkImage TestBind()
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "Shaders", "VulkanTest", "texture.bmp");

            using var bmp = new Bitmap(fullPath);
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            var size = bmp.Width * bmp.Height * 4; // rgba

            using var tmp = VkBuffer<byte>.CreatePacked(device, size, BufferUsageFlags.BufferUsageTransferSrcBit, MemoryPropertyFlags.MemoryPropertyHostVisibleBit | MemoryPropertyFlags.MemoryPropertyHostCoherentBit);
            tmp.LoadFull(new Span<byte>((void*)data.Scan0, size));
            bmp.UnlockBits(data);

            var image = new VkImage(device, (uint)bmp.Width, (uint)bmp.Height, Format.B8G8R8A8Srgb, generateMips: true);
            
            image.QueueLoad(tmp);
            image.CreateView();

            return image;
        }

        public int GetOrBind(BitmapTag bitm)
        {
            if (BoundTextureIndexes.TryGetValue(bitm.Id, out var index))
                return index;

            var (img, sampler) = Upload(bitm);

            index = this.textures.AddTexture(img, sampler);
            BoundTextureIndexes[bitm.Id] = index;
            return index;
        }

        public unsafe (VkImage, VkSampler) Upload(Core.Tags.BitmapTag bitm)
        {
            if (UploadedTextures.TryGetValue(bitm.Id, out var tex))
            {
                return tex;
            }

            // HACK: hard coding texture 0
            var width = bitm.TextureInfos[0].Width;
            var height = bitm.TextureInfos[0].Height;

            if (width == 0 || height == 0)
            {
                return default;
            }

            var topLod = bitm.TextureInfos[0].LevelsOfDetail[0];

            using var tmp = VkBuffer<byte>.CreatePacked(device, topLod.Data.Length, BufferUsageFlags.BufferUsageTransferSrcBit, MemoryPropertyFlags.MemoryPropertyHostVisibleBit | MemoryPropertyFlags.MemoryPropertyHostCoherentBit);
            tmp.LoadFull(topLod.Data.Span);

            VkImage image;
            var format = bitm.TextureInfos[0].Format;
            if (format == TextureFormat.DXT1 || format == TextureFormat.DXT23 || format == TextureFormat.DXT45)
                image = CreateCompressed(bitm, width, height, tmp);
            else
                image = CreateUncompressed(bitm, width, height, tmp);

            tex = (image, image.CreateSampler());

            UploadedTextures.Add(bitm.Id, tex);

            return tex;
        }

        private unsafe VkImage CreateUncompressed(BitmapTag bitm, short width, short height, VkBuffer<byte> tmp)
        {
            var image = new VkImage(device, (uint)width, (uint)height, FormatMappings[bitm.TextureInfos[0].Format], tiling: ImageTiling.Optimal, generateMips: true);
            image.QueueLoad(tmp);

            var red = ComponentSwizzle.R;
            var green = ComponentSwizzle.G;
            var alpha = ComponentSwizzle.A;

            switch (bitm.TextureInfos[0].Format)
            {
                case TextureFormat.A8:
                    image.CreateView(new(alpha, alpha, alpha, alpha));
                    break;

                case TextureFormat.L8:
                    image.CreateView(new(red, red, red, red));
                    break;

                case TextureFormat.A8L8:
                    image.CreateView(new(red, red, red, green));
                    break;

                default:
                    image.CreateView();
                    break;
            }

            return image;
        }

        private VkImage CreateCompressed(BitmapTag bitm, short width, short height, VkBuffer<byte> tmp)
        {
            var sourceUsage = ImageUsageFlags.ImageUsageTransferSrcBit | ImageUsageFlags.ImageUsageSampledBit | ImageUsageFlags.ImageUsageTransferDstBit;
            using var source = new VkImage(device, (uint)width, (uint)height, FormatMappings[bitm.TextureInfos[0].Format], usage: sourceUsage, tiling: ImageTiling.Optimal, generateMips: false);
            source.QueueLoadForBlit(tmp);

            var decompressed = DecompressedFormatMappings[bitm.TextureInfos[0].Format];
            var dest = new VkImage(device, (uint)width, (uint)height, decompressed, tiling: ImageTiling.Optimal, generateMips: true);
            dest.BlitFrom(source);

            dest.CreateView();

            return dest;
        }

        private unsafe (VkImage, VkSampler) CreateErrTexture()
        {
            var width = 4;
            var height = 4;

            var size = width * height * 4;

            using var tmp = VkBuffer<byte>.CreatePacked(device, size, BufferUsageFlags.BufferUsageTransferSrcBit, MemoryPropertyFlags.MemoryPropertyHostVisibleBit | MemoryPropertyFlags.MemoryPropertyHostCoherentBit);
            tmp.LoadFull(new byte[]
            {
                0xFF, 0x00, 0xFF, 0xFF,
                0xFF, 0x00, 0xFF, 0xFF,
                0xFF, 0x00, 0xFF, 0xFF,
                0xFF, 0x00, 0xFF, 0xFF,

                0x00, 0x00, 0x00, 0xFF,
                0x00, 0x00, 0x00, 0xFF,
                0x00, 0x00, 0x00, 0xFF,
                0x00, 0x00, 0x00, 0xFF,

                0xFF, 0x00, 0xFF, 0xFF,
                0xFF, 0x00, 0xFF, 0xFF,
                0xFF, 0x00, 0xFF, 0xFF,
                0xFF, 0x00, 0xFF, 0xFF,

                0x00, 0x00, 0x00, 0xFF,
                0x00, 0x00, 0x00, 0xFF,
                0x00, 0x00, 0x00, 0xFF,
                0x00, 0x00, 0x00, 0xFF,
            });

            var image = new VkImage(device, (uint)width, (uint)height, Format.B8G8R8A8Srgb, tiling: ImageTiling.Optimal, generateMips: true);
            image.QueueLoad(tmp);
            image.CreateView();

            return (image, image.CreateSampler());
        }

        public void Dispose()
        {
            foreach(var (key, item) in this.UploadedTextures)
            {
                item.Item2.Dispose();
                item.Item1.Dispose();
            }
        }
    }
}
