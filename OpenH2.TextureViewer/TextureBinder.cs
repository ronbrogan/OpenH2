using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using OpenH2.Core.Enums.Texture;
using OpenTK.Graphics.OpenGL;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace OpenH2.TextureViewer
{
    public class OpenGLTextureBinder
    {
        public int Bind(Stream textureData)
        {
            var bmp = new Bitmap(textureData);
            var pixFmt = bmp.GetPixel(0, 0);
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var texAddr = Bind(data, bmp.Width, bmp.Height, PixelFormat.Format32bppArgb);
            bmp.UnlockBits(data);
            bmp.Dispose();
            return texAddr;
        }

        public int Bind(OpenH2.Core.Tags.Bitmap bitm)
        {
            var width = bitm.Width;
            var height = bitm.Height;

            var topLod = bitm.LevelsOfDetail[0];
            byte[] lodBytes;

            using (var inputStream = new MemoryStream(topLod.Data.Span.Slice(2).ToArray()))
            using (var decompress = new DeflateStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream((int)inputStream.Length))
            {
                decompress.CopyTo(outputStream);

                outputStream.Seek(0, SeekOrigin.Begin);
                lodBytes = outputStream.ToArray();
            }

            int textureObject;

            GL.GenTextures(1, out textureObject);
            GL.BindTexture(TextureTarget.Texture2D, textureObject);

            


            switch (bitm.TextureFormat)
            {
                case TextureFormat.DXT1:
                    HandleCompressed(lodBytes, InternalFormat.CompressedRgbS3tcDxt1Ext, width, height, bitm.MipMapCount2);
                    break;
                case TextureFormat.DXT23:
                    HandleCompressed(lodBytes, InternalFormat.CompressedRgbaS3tcDxt3Ext, width, height, bitm.MipMapCount2);
                    break;
                case TextureFormat.DXT45:
                    HandleCompressed(lodBytes, InternalFormat.CompressedRgbaS3tcDxt5Ext, width, height, bitm.MipMapCount2);
                    break;
                case TextureFormat.SixteenBit:
                    GL.TexStorage2D(TextureTarget2d.Texture2D, bitm.MipMapCount2, SizedInternalFormat.R16, width, height);
                    GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, OpenTK.Graphics.OpenGL.PixelFormat.R5G6B5IccSgix, PixelType.UnsignedByte, lodBytes);
                    break;
                case TextureFormat.ThirtyTwoBit:
                    GL.TexStorage2D(TextureTarget2d.Texture2D, bitm.MipMapCount2, SizedInternalFormat.Rgba8, width, height);
                    GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, lodBytes);
                    break;
                case TextureFormat.Monochrome:
                    GL.TexStorage2D(TextureTarget2d.Texture2D, bitm.MipMapCount2, SizedInternalFormat.R8, width, height);
                    GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, OpenTK.Graphics.OpenGL.PixelFormat.Red, PixelType.UnsignedByte, lodBytes);
                    break;
            }

            //GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);

            float maxAniso;
            GL.GetFloat((GetPName)ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt, out maxAniso);
            GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, maxAniso);

            var error1 = GL.GetError();
            if (error1 != ErrorCode.NoError)
            {
                Console.WriteLine("-- Error {0} occured at {1}", error1, "some place in texture loader");
            }

            return textureObject;
        }

        private void HandleCompressed(Memory<byte> data, InternalFormat format, int width, int height, int mipMaps)
        {
            int offset = 0;

            var blockSize = (format == InternalFormat.CompressedRgbS3tcDxt1Ext) ? 8 : 16;

            GL.TexStorage2D(TextureTarget2d.Texture2D, mipMaps, (SizedInternalFormat)format, width, height);

            var size = 0;

            int i = 0;
            for (i = 0; i < mipMaps; i++)
            {
                if (width == 0)
                    width = 1;
                if (height == 0)
                    height = 1;

                size = ((width + 3) / 4) * ((height + 3) / 4) * blockSize;

                if(data.Length < offset)
                {
                    break;
                }

                byte[] bytes = data.Slice(offset).ToArray();

                GL.CompressedTexSubImage2D(TextureTarget.Texture2D, i, 0, 0, width, height, (OpenTK.Graphics.OpenGL.PixelFormat)format, size, bytes);

                offset += size;
                width >>= 1;
                height >>= 1;
            }
        }

        public int Bind(BitmapData data, int width, int height, PixelFormat inputFormat = PixelFormat.Format24bppRgb)
        {
            int textureObject;

            GL.GenTextures(1, out textureObject);
            GL.BindTexture(TextureTarget.Texture2D, textureObject);

            var mipMapLevels = (int)Math.Floor(Math.Log(Math.Max(width, height), 2)) + 1;
            GL.TexStorage2D(TextureTarget2d.Texture2D, mipMapLevels, SizedInternalFormat.Rgba8, width, height);

            switch (inputFormat)
            {
                case PixelFormat.Format32bppArgb:
                    GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                    break;

                case PixelFormat.Format24bppRgb:
                default:
                    GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
                    break;
            }

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);

            float maxAniso;
            GL.GetFloat((GetPName)ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt, out maxAniso);
            GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, maxAniso);

            var error1 = GL.GetError();
            if (error1 != ErrorCode.NoError)
            {
                Console.WriteLine("-- Error {0} occured at {1}", error1, "some place in texture loader");
            }

            return textureObject;
        }

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
    }
}
