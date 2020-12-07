using OpenBlam.Serialization.Materialization;
using OpenH2.Core.Maps;
using OpenH2.Core.Offsets;
using OpenH2.Core.Tags;
using System;
using System.Runtime.CompilerServices;

namespace OpenH2.Core.Extensions
{
    public static class SpanByteExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [PrimitiveValueMaterializer]
        public static TagRef ReadTagRefAt(this Span<byte> data, int offset)
        {
            return new TagRef(data.ReadUInt32At(offset));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [PrimitiveValueMaterializer]
        public static TagRef<T> ReadTagRefAt<T>(this Span<byte> data, int offset)
            where T : BaseTag
        {
            return ReadTagRefAt(data, offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [PrimitiveValueMaterializer]
        public static NormalOffset ReadNormalOffsetAt(this Span<byte> data, int offset)
        {
            return new NormalOffset(data.ReadInt32At(offset));
        }
    }
}