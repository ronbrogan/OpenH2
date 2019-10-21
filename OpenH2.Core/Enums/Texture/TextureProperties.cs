using System;

namespace OpenH2.Core.Enums.Texture
{
    [Flags]
    public enum TextureProperties : short
    {
        PowerOfTwo = 1 << 0,
        Compressed = 1 << 1,
        Palette = 1 << 2,
        Swizzled = 1 << 3,
        Linear = 1 << 4,
        v16u16 = 1 << 5
    }
}
