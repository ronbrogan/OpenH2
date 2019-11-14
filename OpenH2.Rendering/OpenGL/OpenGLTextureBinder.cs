using OpenH2.Core.Enums.Texture;
using OpenH2.Rendering.Abstractions;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace OpenH2.Rendering.OpenGL
{
    public class OpenGLTextureBinder : ITextureBinder
    {
        private static Dictionary<TextureFormat, (SizedInternalFormat, PixelFormat)> FormatMappings = new Dictionary<TextureFormat, (SizedInternalFormat, PixelFormat)>
        {
            { TextureFormat.A8, (SizedInternalFormat.R8, PixelFormat.Red)},
            { TextureFormat.L8, (SizedInternalFormat.R8, PixelFormat.Red)},
            { TextureFormat.A8L8, (SizedInternalFormat.Rg16, PixelFormat.Rg)},
            { TextureFormat.U8V8, (SizedInternalFormat.Rg16, PixelFormat.Rg) },
            { TextureFormat.R5G6B5, ((SizedInternalFormat)InternalFormat.Rgb4, PixelFormat.Bgr) },
            { TextureFormat.A4R4G4B4, ((SizedInternalFormat)InternalFormat.Rgba4, PixelFormat.Bgra)},
            { TextureFormat.R8G8B8, (SizedInternalFormat.Rgba8, PixelFormat.Bgra)},
            { TextureFormat.A8R8G8B8, (SizedInternalFormat.Rgba8, PixelFormat.Bgra)},
            { TextureFormat.DXT1, ((SizedInternalFormat)InternalFormat.CompressedRgbS3tcDxt1Ext, (PixelFormat)InternalFormat.CompressedRgbS3tcDxt1Ext) },
            { TextureFormat.DXT23, ((SizedInternalFormat)InternalFormat.CompressedRgbaS3tcDxt3Ext, (PixelFormat)InternalFormat.CompressedRgbaS3tcDxt3Ext) },
            { TextureFormat.DXT45, ((SizedInternalFormat)InternalFormat.CompressedRgbaS3tcDxt5Ext, (PixelFormat)InternalFormat.CompressedRgbaS3tcDxt5Ext) },
        };

        private static Dictionary<TextureFormat, Func<int, int, int>> MipSizeFuncs = new Dictionary<TextureFormat, Func<int, int, int>>
        {
            { TextureFormat.A8, (w,h) => w*h},
            { TextureFormat.L8, (w,h) => w*h},
            { TextureFormat.A8L8, (w,h) => w*h*2},
            { TextureFormat.R5G6B5, (w,h) => w*h*4 },
            { TextureFormat.A4R4G4B4, (w,h) => w*h*4},
            { TextureFormat.R8G8B8, (w,h) => w*h*4},
            { TextureFormat.A8R8G8B8, (w,h) => w*h*4},
            { TextureFormat.DXT1, (w,h) => ((w + 3) / 4) * ((h + 3) / 4) * 8 },
            { TextureFormat.DXT23, (w,h) => ((w + 3) / 4) * ((h + 3) / 4) * 16 },
            { TextureFormat.DXT45, (w,h) => ((w + 3) / 4) * ((h + 3) / 4) * 16 }
        };

        public int Bind(string filename)
        {
            var fullPath = Path.GetFullPath(filename);

            var bmp = new Bitmap(fullPath);
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            var texAddr = Bind(data, bmp.Width, bmp.Height);
            bmp.UnlockBits(data);
            bmp.Dispose();
            return texAddr;
        }

        public int Bind(Stream textureData)
        {
            var bmp = new Bitmap(textureData);
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var texAddr = Bind(data, bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            bmp.UnlockBits(data);
            bmp.Dispose();
            return texAddr;
        }

        public int Bind(Core.Tags.BitmapTag bitm, out long handle)
        {
            var width = bitm.Width;
            var height = bitm.Height;

            if(width == 0 || height == 0)
            {
                handle = long.MaxValue;
                return int.MaxValue;
            }

            var topLod = bitm.LevelsOfDetail[0];

            GL.GenTextures(1, out int texId);
            GL.BindTexture(TextureTarget.Texture2D, texId);

            UploadMips(topLod.Data.Span, bitm.TextureFormat, bitm.Format, width, height, bitm.MipMapCount == 0 ? bitm.MipMapCount2 : bitm.MipMapCount);

            SetCommonTextureParams();

            handle = GL.Arb.GetTextureHandle(texId);

            GL.Arb.MakeTextureHandleResident(handle);

            CheckTextureBindErrors();

            return texId;
        }

        private void UploadMips(Span<byte> data, TextureCompressionFormat format, TextureFormat format2, int width, int height, int mipMaps)
        {
            int offset = 0;

            var (sizedFormat, pixelFormat) = FormatMappings[format2];
            var size = 0;

            if (mipMaps == 0)
                mipMaps = 1;

            GL.TexStorage2D(TextureTarget2d.Texture2D, mipMaps, sizedFormat, width, height);

            switch (format2)
            {
                case TextureFormat.A8:
                {
                    var alpha = (int)PixelFormat.Alpha;
                    GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleRgba,  new[] { alpha, alpha, alpha, alpha });
                    break;
                }

                case TextureFormat.L8:
                {
                    var red = (int)PixelFormat.Red;
                    GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleRgba, new[] { red, red, red, red });
                    break;
                }

                case TextureFormat.A8L8:
                {
                    var red = (int)PixelFormat.Red;
                    var alpha = (int)PixelFormat.Alpha;
                    GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleRgba, new[] { red, red, red, alpha });
                    break;
                }
            }

            if (width == 0)
                width = 1;
            if (height == 0)
                height = 1;

            var maxSize = MipSizeFuncs[format2](width, height);

            var bytes = new byte[maxSize];

            for (var i = 0; i < mipMaps; i++)
            {
                if (width == 0)
                    width = 1;
                if (height == 0)
                    height = 1;

                size = MipSizeFuncs[format2](width, height);
                 
                // Handle corrupt images
                if(data.Length < offset || data.Length < offset+size)
                {
                    break;
                }

                for(var j = 0; j < size; j++)
                {
                    bytes[j] = data[offset + j];
                }

                switch (format)
                {
                    case TextureCompressionFormat.DXT1:
                    case TextureCompressionFormat.DXT23:
                    case TextureCompressionFormat.DXT45:
                        GL.CompressedTexSubImage2D(TextureTarget.Texture2D, i, 0, 0, width, height, pixelFormat, size, bytes);
                        break;
                    case TextureCompressionFormat.SixteenBit:
                    case TextureCompressionFormat.ThirtyTwoBit:
                    case TextureCompressionFormat.Monochrome:
                        GL.TexSubImage2D(TextureTarget.Texture2D, i, 0, 0, width, height, pixelFormat, PixelType.UnsignedByte, bytes);
                        break;
                }

                offset += size;
                width >>= 1;
                height >>= 1;
            }
        }

        public int Bind(BitmapData data, int width, int height, System.Drawing.Imaging.PixelFormat inputFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb)
        {
            GL.GenTextures(1, out int texHandle);
            GL.BindTexture(TextureTarget.Texture2D, texHandle);

            var mipMapLevels = (int)Math.Floor(Math.Log(Math.Max(width, height), 2)) + 1;
            GL.TexStorage2D(TextureTarget2d.Texture2D, mipMapLevels, SizedInternalFormat.Rgba8, width, height);

            switch (inputFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                    break;

                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                default:
                    GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
                    break;
            }

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            SetCommonTextureParams();
            CheckTextureBindErrors();

            return texHandle;
        }

        public int Bind3D<TData>(TData[] data, int width, int height, int depth, bool genMipMaps, out long handle) where TData: struct
        {
            GL.GenTextures(1, out int texId);
            GL.BindTexture(TextureTarget.Texture3D, texId);

            int levels = 7;
            GL.TexStorage3D(TextureTarget3d.Texture3D, levels, SizedInternalFormat.Rgba8, width, height, depth);
            GL.TexImage3D(TextureTarget.Texture3D, 0, PixelInternalFormat.Rgba8, width, height, depth, 0, PixelFormat.Rgba, PixelType.Float, data);
            if(genMipMaps)
            {
                GL.GenerateMipmap(GenerateMipmapTarget.Texture3D);
            }

            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            handle = GL.Arb.GetTextureHandle(texId);

            GL.Arb.MakeTextureHandleResident(handle);

            CheckTextureBindErrors();

            return texId;
        }

        private void SetCommonTextureParams()
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);

            float maxAniso;
            GL.GetFloat((GetPName)ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt, out maxAniso);
            GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, maxAniso);
        }

        private void CheckTextureBindErrors()
        {
            var error1 = GL.GetError();
            if (error1 != ErrorCode.NoError)
            {
                Console.WriteLine("-- Error {0} occured at {1}", error1, "some place in texture loader");
            }
        }
    }
}
