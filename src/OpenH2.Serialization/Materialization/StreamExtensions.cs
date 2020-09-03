using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenH2.Serialization.Materialization
{
    public static class StreamExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadStringFrom(this Stream data, int offset, int length)
        {
            var len = Math.Min(length, data.Length - offset);

            Span<byte> stringBytes = stackalloc byte[0];
            Span<char> stringChars = stackalloc char[0];

            if(length < 512)
            {
                stringBytes = stackalloc byte[length];
            }
            else
            {
                stringBytes = new byte[length];
            }

            data.Position = offset;
            var actualRead = data.Read(stringBytes);

            if(actualRead < 512)
            {
                stringChars = stackalloc char[actualRead];
            }
            else
            {
                stringChars = new char[actualRead];
            }

            for(var i = 0; i < actualRead; i++)
            {
                stringChars[i] = (char)stringBytes[i];
            }

            return new string(stringChars);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadStringStarting(this Stream data, int offset)
        {
            var builder = new StringBuilder(32);

            Span<byte> stringBytes = stackalloc byte[512];
            Span<char> stringChars = stackalloc char[512];

            data.Position = offset;
            while (true)
            {
                var actualRead = data.Read(stringBytes);

                int foundNull = -1;

                for(var i = 0; i < actualRead; i++)
                {
                    if (stringBytes[i] == 0b0)
                    {
                        foundNull = i;
                        break;
                    }
                    else
                    {
                        stringChars[i] = (char)stringBytes[i];
                    }
                }

                if(foundNull >= 0)
                {
                    builder.Append(stringChars.Slice(0, foundNull));
                    break;
                }
                else
                {
                    builder.Append(stringChars.Slice(0, actualRead));
                }
            }

            return builder.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadByteAt(this Stream data, int offset)
        {
            data.Seek(offset, SeekOrigin.Begin);
            return (byte)data.ReadByte();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadInt16At(this Stream data, int offset)
        {
            if(offset + 2 > data.Length)
            {
                return 0;
            }

            Span<byte> bytes = stackalloc byte[2];
            data.Position = offset;
            data.Read(bytes);

            return BitConverter.ToInt16(bytes);
            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32At(this Stream data, int offset)
        {
            if (offset + 4 > data.Length)
            {
                return 0;
            }

            Span<byte> bytes = stackalloc byte[4];
            data.Position = offset;
            data.Read(bytes);

            return BitConverter.ToInt32(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUInt16At(this Stream data, int offset)
        {
            if (offset + 2 > data.Length)
            {
                return 0;
            }

            Span<byte> bytes = stackalloc byte[2];
            data.Position = offset;
            data.Read(bytes);

            return BitConverter.ToUInt16(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32At(this Stream data, int offset)
        {
            if (offset + 4 > data.Length)
            {
                return 0;
            }

            Span<byte> bytes = stackalloc byte[4];
            data.Position = offset;
            data.Read(bytes);

            return BitConverter.ToUInt32(bytes);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static TagRef ReadTagRefAt(this Stream data, int offset)
        //{
        //    return new TagRef(data.ReadUInt32At(offset));
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static InternedString ReadInternedStringAt(this Stream data, int offset)
        //{
        //    var val = data.ReadUInt32At(offset);

        //    // top byte is length
        //    var i = new InternedString(val & 0xFFFFFF, val >> 24);

        //    //if(map.InternedStrings.TryGetValue((int)i.Id, out var s))
        //    //{
        //    //    i.Value = s;
        //    //}

        //    return i;
        //}

        //public static CountAndOffset ReadMetaCaoAt(this Stream data, int offset, TagIndexEntry index)
        //{
        //    return ReadMetaCaoAt(data, offset, index.Offset.Value);
        //}

        //public static CountAndOffset ReadMetaCaoAt(this Stream data, int offset, int magic)
        //{
        //    return new CountAndOffset(data.ReadInt32At(offset), new SecondaryOffset(magic, data.ReadInt32At(offset + 4)));
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ReadVec2At(this Stream data, int offset)
        {
            Span<byte> bytes = stackalloc byte[8];
            data.Position = offset;
            data.Read(bytes);

            return new Vector2(
                BitConverter.ToSingle(bytes.Slice(0, 4)),
                BitConverter.ToSingle(bytes.Slice(4, 4))
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ReadVec3At(this Stream data, int offset)
        {
            Span<byte> bytes = stackalloc byte[12];
            data.Position = offset;
            data.Read(bytes);

            return new Vector3(
                BitConverter.ToSingle(bytes.Slice(0, 4)),
                BitConverter.ToSingle(bytes.Slice(4, 4)),
                BitConverter.ToSingle(bytes.Slice(8, 4))
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 ReadVec4At(this Stream data, int offset)
        {
            Span<byte> bytes = stackalloc byte[16];
            data.Position = offset;
            data.Read(bytes);

            return new Vector4(
                BitConverter.ToSingle(bytes.Slice(0, 4)),
                BitConverter.ToSingle(bytes.Slice(4, 4)),
                BitConverter.ToSingle(bytes.Slice(8, 4)),
                BitConverter.ToSingle(bytes.Slice(12, 4))
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion ReadQuaternionAt(this Stream data, int offset)
        {
            Span<byte> bytes = stackalloc byte[16];
            data.Position = offset;
            data.Read(bytes);

            return new Quaternion(
                BitConverter.ToSingle(bytes.Slice(0, 4)),
                BitConverter.ToSingle(bytes.Slice(4, 4)),
                BitConverter.ToSingle(bytes.Slice(8, 4)),
                BitConverter.ToSingle(bytes.Slice(12, 4))
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 ReadMatrix4x4At(this Stream data, int offset)
        {
            Span<byte> bytes = stackalloc byte[64];
            data.Position = offset;
            data.Read(bytes);

            return new Matrix4x4(
                bytes.ReadFloatAt(4 * 0), //1
                bytes.ReadFloatAt(4 * 4), //1
                bytes.ReadFloatAt(4 * 8), //1
                bytes.ReadFloatAt(4 * 12),//1
                bytes.ReadFloatAt(4 * 1), //2
                bytes.ReadFloatAt(4 * 5), //2
                bytes.ReadFloatAt(4 * 9), //2
                bytes.ReadFloatAt(4 * 13),//2
                bytes.ReadFloatAt(4 * 2), //3
                bytes.ReadFloatAt(4 * 6), //3
                bytes.ReadFloatAt(4 * 10),//3
                bytes.ReadFloatAt(4 * 14),//3
                bytes.ReadFloatAt(4 * 3), //4
                bytes.ReadFloatAt(4 * 7), //4
                bytes.ReadFloatAt(4 * 11),//4
                bytes.ReadFloatAt(4 * 15) //4
            );
        }

        public static byte[] ReadArray(this Stream data, int offset, int length)
        {
            var bytes = new byte[length];
            data.Position = offset;

            var totalRead = 0;
            var lastRead = -1;
            while (totalRead != length && lastRead != 0) {
                lastRead = data.Read(bytes, totalRead, length-totalRead);
                totalRead += lastRead;
            }

            return bytes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ReadFloatAt(this Stream data, int offset)
        {
            Span<byte> bytes = stackalloc byte[4];
            data.Position = offset;
            data.Read(bytes);

            return BitConverter.ToSingle(bytes);
        }

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

            [FieldOffset(0)]
            public Quaternion Quaternion;
        }
    }
}