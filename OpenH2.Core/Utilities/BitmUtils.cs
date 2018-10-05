using OpenH2.Core.Enums.Texture;
using OpenH2.Core.Formats;
using OpenH2.Core.Representations.Meta;
using System.Collections.Generic;

namespace OpenH2.Core.Utilities
{
    public static class BitmUtils
    {
        public static byte[] GetTextureHeader(BitmapMeta bitmMeta, int dataSize)
        {
            var ddsHeader = new DdsHeader(
                bitmMeta.TextureFormat,
                bitmMeta.TextureType,
                bitmMeta.Width,
                bitmMeta.Height,
                bitmMeta.Depth,
                bitmMeta.MipMapCount,
                null,
                null);

            var bytes = new byte[128];

            ddsHeader.HeaderData.Read(bytes, 0, 128);

            return bytes;
        }
    }
}