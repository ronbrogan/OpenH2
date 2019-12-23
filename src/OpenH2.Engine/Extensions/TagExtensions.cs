using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Engine.Extensions
{
    public static class TagExtensions
    {
        public static BitmapTag GetBitmap(this ShaderTag.ShaderArguments args, H2vMap map, int index)
        {
            if (map.TryGetTag(args.ShaderMaps[index].Bitmap, out var bitm))
                return bitm;

            return null;
        }
    }
}
