using OpenH2.Core.Formats;
using OpenH2.Core.Tags;
using System.IO;

namespace OpenH2.Core.Utilities
{
    public static class BitmUtils
    {
        public static void WriteTextureHeader(BitmapTag bitmTag, Stream destination)
        {
            var ddsHeader = new DdsHeader(
                bitmTag.TextureFormat,
                bitmTag.TextureType,
                bitmTag.Width,
                bitmTag.Height,
                bitmTag.Depth,
                bitmTag.MipMapCount,
                null,
                null);

            ddsHeader.HeaderData.CopyTo(destination);
        }
    }
}