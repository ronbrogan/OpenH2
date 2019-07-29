using OpenH2.Core.Tags;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace OpenH2.Rendering.Abstractions
{
    public interface ITextureBinder
    {
        int Bind(Stream data);
        int Bind(string path);
        int Bind(Bitmap bitm, out long handle);
        int Bind(BitmapData data, int width, int height, PixelFormat inputFormat = PixelFormat.Format24bppRgb);
    }
}
