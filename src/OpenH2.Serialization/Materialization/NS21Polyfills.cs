using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenH2.Serialization.Materialization
{
    internal static class PBitConverter
    {
        public static int ToInt32(this Span<byte> data) => GetConverter(data, 4).Int32;
        public static uint ToUInt32(this Span<byte> data) => GetConverter(data, 4).UInt32;
        public static short ToInt16(this Span<byte> data) => GetConverter(data, 4).Int16;
        public static ushort ToUInt16(this Span<byte> data) => GetConverter(data, 2).UInt16;
        public static float ToSingle(this Span<byte> data) => GetConverter(data, 4).Single;

        public static int Read(this Stream stream, Span<byte> data)
        {
            var arr = new byte[data.Length];

            var actual = stream.Read(arr, 0, data.Length);

            // We'll make two attempts to read the desired data
            if(actual != data.Length)
            {
                actual += stream.Read(arr, actual, data.Length - actual);
            }

            new Span<byte>(arr, 0, actual).CopyTo(data);

            return actual;
        }

        public static void Append(this StringBuilder builder, ReadOnlySpan<char> chars)
        {

        }

        private static Converter GetConverter(Span<byte> data, int length)
        {
            var c = new Converter();

            if (data.Length >= length)
            {
                uint accum = 0;
                for (var i = 0; i < length; i++)
                {
                    accum |= (uint)(data[i] << i*8);
                }

                c.UInt32 = accum;
            }

            return c;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct Converter
        {
            [FieldOffset(0)]
            public uint UInt32;

            [FieldOffset(0)]
            public int Int32;

            [FieldOffset(0)]
            public short Int16;

            [FieldOffset(0)]
            public ushort UInt16;

            [FieldOffset(0)]
            public float Single;
        }
    }
}
