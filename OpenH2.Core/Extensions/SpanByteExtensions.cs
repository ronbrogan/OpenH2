using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using OpenH2.Core.Offsets;
using OpenH2.Core.Representations;

namespace OpenH2.Core.Extensions
{
    public static class SpanByteExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToStringFromNullTerminated(this Span<byte> data)
        {
            var builder = new StringBuilder(data.Length);

            var current = 0;
            while (true)
            {
                if (current == data.Length || data[current] == 0b0)
                {
                    break;
                }

                builder.Append((char)data[current]);
                current++;
            }

            return builder.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadStringFrom(this Span<byte> data, int offset, int length)
        {
            var len = Math.Min(length, data.Length - offset);

            return data.Slice(offset, len).ToStringFromNullTerminated();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadStringStarting(this Span<byte> data, int offset)
        {
            var builder = new StringBuilder(32);

            var current = offset;
            while (true)
            {
                if (data[current] == 0b0)
                {
                    break;
                }

                builder.Append((char)data[current]);
                current++;
            }

            return builder.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadInt16At(this Span<byte> data, int offset)
        {
            if(offset + 2 > data.Length)
            {
                return 0;
            }

            var bytes = data.Slice(offset, 2);

            short value = 0;
            var shift = 0;

            foreach (short b in bytes)
            {
                // Shift bits into correct position and add into value
                value = (short)(value | (b << (shift * 8)));

                shift++;
            }

            return value;
            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32At(this Span<byte> data, int offset)
        {
            if (offset + 4 > data.Length)
            {
                return 0;
            }

            var bytes = data.Slice(offset, 4);

            var value = 0;
            var shift = 0;

            foreach (int b in bytes)
            {
                // Shift bits into correct position and add into value
                value = value | (b << (shift * 8));

                shift++;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUInt16At(this Span<byte> data, int offset)
        {
            if (offset + 2 > data.Length)
            {
                return 0;
            }

            ushort value = data[offset];
            value = (ushort)(value | (data[offset + 1] << 8));

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32At(this Span<byte> data, int offset)
        {
            if (offset + 4 > data.Length)
            {
                return 0;
            }

            var bytes = data.Slice(offset, 4);

            uint value = 0;
            var shift = 0;

            foreach (uint b in bytes)
            {
                // Shift bits into correct position and add into value
                value = value | (b << (shift * 8));

                shift++;
            }

            return value;
        }

        public static CountAndOffset ReadMetaCaoAt(this Span<byte> data, int offset, TagIndexEntry index)
        {
            return new CountAndOffset(data.ReadInt32At(offset), new TagInternalOffset(index, data.ReadInt32At(offset + 4)));
        }

        private static UInt32ToSingle floatConverter = new UInt32ToSingle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ReadFloatAt(this Span<byte> data, int offset)
        {
            floatConverter.UInt32 = data.ReadUInt32At(offset);

            return floatConverter.Single;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct UInt32ToSingle
        {
            [FieldOffset(0)]
            public uint UInt32;

            [FieldOffset(0)]
            public float Single;
        }
    }
}