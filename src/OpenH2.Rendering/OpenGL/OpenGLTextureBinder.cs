using OpenBlam.Core.Texturing;
using OpenH2.Core.Enums.Texture;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ARB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using PixelFormat = Silk.NET.OpenGL.PixelFormat;


namespace OpenH2.Rendering.OpenGL
{
    public class OpenGLTextureBinder : ITextureBinder
    {
        private static Dictionary<TextureFormat, (SizedInternalFormat, PixelFormat)> FormatMappings = new Dictionary<TextureFormat, (SizedInternalFormat, PixelFormat)>
        {
            { TextureFormat.A8, (SizedInternalFormat.R8, PixelFormat.Red)},
            { TextureFormat.L8, (SizedInternalFormat.R8, PixelFormat.Red)},
            { TextureFormat.A8L8, (SizedInternalFormat.RG16, PixelFormat.RG)},
            { TextureFormat.U8V8, (SizedInternalFormat.RG16, PixelFormat.RG) },
            { TextureFormat.R5G6B5, ((SizedInternalFormat)InternalFormat.Rgb4, PixelFormat.Bgr) },
            { TextureFormat.A4R4G4B4, ((SizedInternalFormat)InternalFormat.Rgba4, PixelFormat.Bgra)},
            { TextureFormat.R8G8B8, (SizedInternalFormat.Rgba8, PixelFormat.Bgra)},
            { TextureFormat.A8R8G8B8, (SizedInternalFormat.Rgba8, PixelFormat.Bgra)},
            { TextureFormat.DXT1, ((SizedInternalFormat)InternalFormat.CompressedRgbS3TCDxt1Ext, (PixelFormat)InternalFormat.CompressedRgbS3TCDxt1Ext) },
            { TextureFormat.DXT23, ((SizedInternalFormat)InternalFormat.CompressedRgbaS3TCDxt3Ext, (PixelFormat)InternalFormat.CompressedRgbaS3TCDxt3Ext) },
            { TextureFormat.DXT45, ((SizedInternalFormat)InternalFormat.CompressedRgbaS3TCDxt5Ext, (PixelFormat)InternalFormat.CompressedRgbaS3TCDxt5Ext) },
        };

        private static Dictionary<TextureFormat, Func<int, int, int>> MipSizeFuncs = new Dictionary<TextureFormat, Func<int, int, int>>
        {
            { TextureFormat.A8, (w,h) => w*h},
            { TextureFormat.L8, (w,h) => w*h},
            { TextureFormat.A8L8, (w,h) => w*h*2},
            { TextureFormat.U8V8, (w,h) => w*h*2},
            { TextureFormat.R5G6B5, (w,h) => w*h*4 },
            { TextureFormat.A4R4G4B4, (w,h) => w*h*4},
            { TextureFormat.R8G8B8, (w,h) => w*h*4},
            { TextureFormat.A8R8G8B8, (w,h) => w*h*4},
            { TextureFormat.DXT1, (w,h) => ((w + 3) / 4) * ((h + 3) / 4) * 8 },
            { TextureFormat.DXT23, (w,h) => ((w + 3) / 4) * ((h + 3) / 4) * 16 },
            { TextureFormat.DXT45, (w,h) => ((w + 3) / 4) * ((h + 3) / 4) * 16 }
        };

        private Dictionary<uint, (uint, ulong)> BoundTextures = new Dictionary<uint, (uint, ulong)>();
        private readonly OpenGLHost host;
        private readonly GL api;

        private GL gl => host?.gl ?? api;
        private Lazy<ArbBindlessTexture> bindless;

        public OpenGLTextureBinder(OpenGLHost host)
        {
            this.host = host;
            this.bindless = new(() =>
            {
                Debug.Assert(gl.TryGetExtension<ArbBindlessTexture>(out var bindless));
                return bindless;
            });
        }

        public OpenGLTextureBinder(GL api)
        {
            this.api = api;
            this.bindless = new(() =>
            {
                Debug.Assert(gl.TryGetExtension<ArbBindlessTexture>(out var bindless));
                return bindless;
            });
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

        public int Bind(Stream textureData)
        {
            var bmp = new Bitmap(textureData);
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var texAddr = Bind(data, bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            bmp.UnlockBits(data);
            bmp.Dispose();
            return texAddr;
        }

        public int GetOrBind(Core.Tags.BitmapTag bitm, out long handle)
        {
            if(BoundTextures.TryGetValue(bitm.Id, out var ids))
            {
                handle = (long)ids.Item2;
                return (int)ids.Item1;
            }

            // HACK: hard coding texture 0
            var width = bitm.TextureInfos[0].Width;
            var height = bitm.TextureInfos[0].Height;

            if(width == 0 || height == 0)
            {
                handle = long.MaxValue;
                return int.MaxValue;
            }

            var topLod = bitm.TextureInfos[0].LevelsOfDetail[0];


            gl.GenTextures(1, out uint texId);
            gl.BindTexture((GLEnum)TextureTarget.Texture2D, texId);

            UploadMips(topLod.Data.Span, bitm.TextureFormat, bitm.TextureInfos[0].Format, width, height, bitm.MipMapCount == 0 ? (uint)bitm.TextureInfos[0].MipMapCount2 : (uint)bitm.MipMapCount);

            SetCommonTextureParams();

            var thandle = bindless.Value.GetTextureHandle(texId);
            bindless.Value.MakeTextureHandleResident(thandle);

            handle = (long)thandle;

            CheckTextureBindErrors();

            BoundTextures.Add(bitm.Id, (texId, thandle));
            return (int)texId;
        }

        private void UploadMips(Span<byte> data, TextureCompressionFormat format, TextureFormat format2, int width, int height, uint mipMaps)
        {
            int offset = 0;

            var (sizedFormat, pixelFormat) = FormatMappings[format2];
            var size = 0;

            if (mipMaps == 0)
                mipMaps = 1;

            gl.TexStorage2D(GLEnum.Texture2D, mipMaps, (GLEnum)sizedFormat, (uint)width, (uint)height);

            switch (format2)
            {
                case TextureFormat.A8:
                {
                    var alpha = (int)GLEnum.Alpha;
                    gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureSwizzleRgba,  new[] { alpha, alpha, alpha, alpha });
                    break;
                }
            
                case TextureFormat.L8:
                {
                    var red = (int)GLEnum.Red;
                    gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureSwizzleRgba, new[] { red, red, red, red });
                    break;
                }
            
                case TextureFormat.A8L8:
                {
                    var red = (int)GLEnum.Red;
                    var alpha = (int)GLEnum.Alpha;
                    gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureSwizzleRgba, new[] { red, red, red, alpha });
                    break;
                }
            }

            if (width == 0)
                width = 1;
            if (height == 0)
                height = 1;

            var maxSize = MipSizeFuncs[format2](width, height);

            Span<byte> bytes = new byte[maxSize];

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
                        gl.CompressedTexSubImage2D<byte>((GLEnum)TextureTarget.Texture2D, i, 0, 0, (uint)width, (uint)height, (GLEnum)pixelFormat, (uint)size, bytes);
                        break;
                    case TextureCompressionFormat.SixteenBit:
                    case TextureCompressionFormat.ThirtyTwoBit:
                    case TextureCompressionFormat.Monochrome:
                        gl.TexSubImage2D<byte>((GLEnum)TextureTarget.Texture2D, i, 0, 0, (uint)width, (uint)height, pixelFormat, PixelType.UnsignedByte, bytes);
                        break;
                }

                offset += size;
                width >>= 1;
                height >>= 1;
            }
        }

        public int Bind(BitmapData data, int width, int height, System.Drawing.Imaging.PixelFormat inputFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb)
        {
            gl.GenTextures(1, out uint texHandle);
            gl.BindTexture((GLEnum)TextureTarget.Texture2D, texHandle);

            var mipMapLevels = (uint)Math.Floor(Math.Log(Math.Max(width, height), 2)) + 1;
            gl.TexStorage2D(GLEnum.Texture2D, mipMapLevels, SizedInternalFormat.Rgba8, (uint)width, (uint)height);

            switch (inputFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    gl.TexSubImage2D((GLEnum)TextureTarget.Texture2D, 0, 0, 0, (uint)width, (uint)height, (GLEnum)PixelFormat.Bgra, (GLEnum)PixelType.UnsignedByte, data.Scan0);
                    break;

                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                default:
                    gl.TexSubImage2D((GLEnum)TextureTarget.Texture2D, 0, 0, 0, (uint)width, (uint)height, (GLEnum)PixelFormat.Bgr, (GLEnum)PixelType.UnsignedByte, data.Scan0);
                    break;
            }

            gl.GenerateMipmap(GLEnum.Texture2D);

            SetCommonTextureParams();
            CheckTextureBindErrors();

            return (int)texHandle;
        }

        private void SetCommonTextureParams()
        {
            gl.TexParameter((GLEnum)TextureTarget.Texture2D, (GLEnum)TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            gl.TexParameter((GLEnum)TextureTarget.Texture2D, (GLEnum)TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            gl.TexParameter((GLEnum)TextureTarget.Texture2D, (GLEnum)TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            gl.TexParameter((GLEnum)TextureTarget.Texture2D, (GLEnum)TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            float maxAniso;
            
            gl.GetFloat((GetPName)ARB.MaxTextureMaxAnisotropy, out maxAniso);
            gl.TexParameter(TextureTarget.Texture2D, (TextureParameterName)ARB.TextureMaxAnisotropy, maxAniso);
        }

        private void CheckTextureBindErrors()
        {
            var error1 = gl.GetError();
            if (error1 != GLEnum.NoError)
            {
                Console.WriteLine("-- Error {0} occured at {1}", error1, "some place in texture loader");
            }
        }
    }
}
