﻿using System.Runtime.CompilerServices;

namespace OpenH2.Foundation.Extensions
{
    public static class UnsafeExtensions
    {
        public unsafe static int Offset<TObj, TField>(ref TObj obj, ref TField field) where TObj : unmanaged where TField : unmanaged
            => (int)((byte*)Unsafe.AsPointer(ref field) - (byte*)Unsafe.AsPointer(ref obj));
    }
}
