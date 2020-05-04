using System;
using System.Numerics;
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
        public static byte ReadByteAt(this Span<byte> data, int offset)
        {
            return data[offset];
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagRef ReadTagRefAt(this Span<byte> data, int offset)
        {
            return new TagRef(data.ReadUInt32At(offset));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static InternedString ReadInternedStringAt(this Span<byte> data, int offset)
        {
            var val = data.ReadUInt32At(offset);

            // top byte is length
            return new InternedString(val & 0xFFFFFF, val >> 24);
        }

        public static CountAndOffset ReadMetaCaoAt(this Span<byte> data, int offset, TagIndexEntry index)
        {
            return ReadMetaCaoAt(data, offset, index.Offset.Value);
        }

        public static CountAndOffset ReadMetaCaoAt(this Span<byte> data, int offset, int magic)
        {
            return new CountAndOffset(data.ReadInt32At(offset), new SecondaryOffset(magic, data.ReadInt32At(offset + 4)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ReadVec2At(this Span<byte> data, int offset)
        {
            data.Slice(offset, 8).CopyTo(vectorConverterBytes);
            vectorConverter.Guid = new Guid(vectorConverterBytes);

            return vectorConverter.Vec2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ReadVec3At(this Span<byte> data, int offset)
        {
            data.Slice(offset, 12).CopyTo(vectorConverterBytes);
            vectorConverter.Guid = new Guid(vectorConverterBytes);

            return vectorConverter.Vec3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 ReadVec4At(this Span<byte> data, int offset)
        {
            data.Slice(offset, 16).CopyTo(vectorConverterBytes);
            vectorConverter.Guid = new Guid(vectorConverterBytes);

            return vectorConverter.Vec4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 ReadMatrix4x4At(this Span<byte> data, int offset)
        {
            var matrixBytes = data.Slice(offset, 64);

            return new Matrix4x4(
                matrixBytes.ReadFloatAt(4 * 0), //1
                matrixBytes.ReadFloatAt(4 * 4), //1
                matrixBytes.ReadFloatAt(4 * 8), //1
                matrixBytes.ReadFloatAt(4 * 12),//1
                matrixBytes.ReadFloatAt(4 * 1), //2
                matrixBytes.ReadFloatAt(4 * 5), //2
                matrixBytes.ReadFloatAt(4 * 9), //2
                matrixBytes.ReadFloatAt(4 * 13),//2
                matrixBytes.ReadFloatAt(4 * 2), //3
                matrixBytes.ReadFloatAt(4 * 6), //3
                matrixBytes.ReadFloatAt(4 * 10),//3
                matrixBytes.ReadFloatAt(4 * 14),//3
                matrixBytes.ReadFloatAt(4 * 3), //4
                matrixBytes.ReadFloatAt(4 * 7), //4
                matrixBytes.ReadFloatAt(4 * 11),//4
                matrixBytes.ReadFloatAt(4 * 15) //4
            );
        }

        public static byte[] ReadArray(this Span<byte> data, int offset, int length)
        {
            return data.Slice(offset, length).ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ReadFloatAt(this Span<byte> data, int offset)
        {
            floatConverter.UInt32 = data.ReadUInt32At(offset);

            return floatConverter.Single;
        }

        private static UInt32ToSingle floatConverter = new UInt32ToSingle();
        private static VecConverter vectorConverter = new VecConverter();
        private static byte[] vectorConverterBytes = new byte[16];


        [StructLayout(LayoutKind.Explicit)]
        private struct VecConverter
        {
            [FieldOffset(0)]
            public Guid Guid;

            [FieldOffset(0)]
            public Vector2 Vec2;

            [FieldOffset(0)]
            public Vector3 Vec3;

            [FieldOffset(0)]
            public Vector4 Vec4;
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