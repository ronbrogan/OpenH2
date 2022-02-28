using OpenH2.Core.Tags;
using System.Drawing.Imaging;
using System.IO;

namespace OpenH2.Rendering.OpenGL
{
    public interface ITextureBinder
    {
        int Bind(Stream data);
        int Bind(string path);
        int GetOrBind(BitmapTag bitm, out long handle);
        int Bind(BitmapData data, int width, int height, PixelFormat inputFormat = PixelFormat.Format24bppRgb);
    }
}
