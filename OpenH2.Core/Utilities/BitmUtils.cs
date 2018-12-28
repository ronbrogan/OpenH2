using OpenH2.Core.Formats;
using OpenH2.Core.Tags;
using System.IO;

namespace OpenH2.Core.Utilities
{
    public static class BitmUtils
    {
        public static void WriteTextureHeader(Bitmap bitm, Stream destination)
        {
            var ddsHeader = new DdsHeader(
                bitm.TextureFormat,
                bitm.TextureType,
                bitm.Width,
                bitm.Height,
                bitm.Depth,
                bitm.MipMapCount,
                null,
                null);

            ddsHeader.HeaderData.CopyTo(destination);
        }
    }
}