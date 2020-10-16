using OpenH2.Core.Maps;
using OpenH2.Core.Tags;
using System.Numerics;

namespace OpenH2.Core.Extensions
{
    public static class TagExtensions
    {
        public static BitmapTag GetBitmap(this ShaderTag.ShaderTemplateArguments args, H2vMap map, int? index)
        {
            if (index.HasValue == false)
                return null;

            if (map.TryGetTag(args.BitmapArguments[index.Value].Bitmap, out var bitm))
                return bitm;

            return null;
        }

        public static Vector4 GetInput(this ShaderTag.ShaderTemplateArguments args, int? index, Vector4 defaultValue = default)
        {
            if (defaultValue == default)
                defaultValue = new Vector4(1, 1, 0, 0);

            if (index.HasValue == false)
                return defaultValue;

            if (index.Value >= args.ShaderInputs.Length)
                return defaultValue;

            return args.ShaderInputs[index.Value];
        }
    }
}
