using System;

namespace OpenH2.Core.Enums
{
    [Flags]
    public enum ColorChangeFlags : ushort
    {
        Primary     = 1 << 0,
        Secondary   = 1 << 1,
        Tertiary    = 1 << 2,
        Quaternary  = 1 << 3
    }
}
