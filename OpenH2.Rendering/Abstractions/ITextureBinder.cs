using OpenH2.Core.Tags;
using System.Drawing.Imaging;
using System.IO;

namespace OpenH2.Rendering.Abstractions
{
    public interface ITextureBinder
    {
        int Bind(Stream data);
        int Bind(string path);
        int Bind(BitmapTag bitm, out long handle);
        int Bind(BitmapData data, int width, int height, PixelFormat inputFormat = PixelFormat.Format24bppRgb);
        int Bind3D<TData>(TData[] data, int width, int height, int depth, bool genMipMaps, out long handle) where TData : struct;
    }
}
