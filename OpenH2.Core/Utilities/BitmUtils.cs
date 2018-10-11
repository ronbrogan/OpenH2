using OpenH2.Core.Formats;
using OpenH2.Core.Meta;
using System.IO;

namespace OpenH2.Core.Utilities
{
    public static class BitmUtils
    {
        public static void WriteTextureHeader(BitmapMeta bitmMeta, Stream destination)
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

            ddsHeader.HeaderData.CopyTo(destination);
        }
    }
}