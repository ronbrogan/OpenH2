using OpenH2.Core.Offsets;
using OpenH2.Core.Maps;
using OpenH2.Core.Tags;
using OpenH2.Serialization.Materialization;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace OpenH2.Core.Extensions
{
    public static class StreamExtensions
    {
        public static Memory<byte> ToMemory(this Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (stream.Length > int.MaxValue)
                throw new Exception("The stream provided is greater than " + int.MaxValue + " bytes long.");

            var bytes = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(bytes, 0, (int)stream.Length);
            return new Memory<byte>(bytes);
        }

        public static void WriteInt32(this Stream stream, int value)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 24));
        }

        public static void WriteUInt32(this Stream stream, uint value)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 24));
        }

        public static void WriteInt16(this Stream stream, int value)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadStringFrom(this Stream data, int offset, int length)
        {
            var len = Math.Min(length, data.Length - offset);

            Span<byte> stringBytes = stackalloc byte[0];
            Span<char> stringChars = stackalloc char[0];

            if (length < 512)
            {
                stringBytes = stackalloc byte[length];
            }
            else
            {
                stringBytes = new byte[length];
            }

            if (data.Position != offset)
                data.Position = offset;

            var actualRead = data.Read(stringBytes);

            if (actualRead < 512)
            {
                stringChars = stackalloc char[actualRead];
            }
            else
            {
                stringChars = new char[actualRead];
            }

            var i = 0;
            for (; i < actualRead; i++)
            {
                if (stringBytes[i] == 0b0)
                {
                    break;
                }

                stringChars[i] = (char)stringBytes[i];
            }

            return new string(stringChars.Slice(0, i));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadStringStarting(this Stream data, int offset)
        {
            var builder = new StringBuilder(32);

            Span<byte> stringBytes = stackalloc byte[512];
            Span<char> stringChars = stackalloc char[512];

            if (data.Position != offset)
                data.Position = offset;

            while (true)
            {
                var actualRead = data.Read(stringBytes);

                if (actualRead == 0)
                    break;

                int foundNull = -1;

                for (var i = 0; i < actualRead; i++)
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

                if (foundNull >= 0)
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
            if (data.Position != offset)
                data.Position = offset;

            return (byte)data.ReadByte();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadInt16At(this Stream data, int offset)
        {
            Span<byte> bytes = stackalloc byte[2];

            if (data.Position != offset)
                data.Position = offset;

            data.Read(bytes);

            return BitConverter.ToInt16(bytes);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32At(this Stream data, int offset)
        {
            Span<byte> bytes = stackalloc byte[4];
            
            if(data.Position != offset)
                data.Position = offset;

            data.Read(bytes);

            return BitConverter.ToInt32(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUInt16At(this Stream data, int offset)
        {
            Span<byte> bytes = stackalloc byte[2];

            if (data.Position != offset)
                data.Position = offset;

            data.Read(bytes);

            return BitConverter.ToUInt16(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt16At(this Stream data, int offset, ushort value)
        {
            if (offset + 2 > data.Length)
            {
                return;
            }

            if (data.Position != offset)
                data.Position = offset;

            data.Write(BitConverter.GetBytes(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32At(this Stream data, int offset)
        {
            Span<byte> bytes = stackalloc byte[4];

            if (data.Position != offset)
                data.Position = offset;

            data.Read(bytes);

            return BitConverter.ToUInt32(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt32At(this Stream data, int offset, uint value)
        {
            if (offset + 4 > data.Length)
            {
                return;
            }

            if (data.Position != offset)
                data.Position = offset;

            data.Write(BitConverter.GetBytes(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ReadVec2At(this Stream data, int offset)
        {
            Span<byte> bytes = stackalloc byte[8];

            if (data.Position != offset)
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

            if (data.Position != offset)
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

            if (data.Position != offset)
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

            if (data.Position != offset)
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

            if (data.Position != offset)
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

            if (data.Position != offset)
                data.Position = offset;

            var totalRead = 0;
            var lastRead = -1;
            while (totalRead != length && lastRead != 0)
            {
                lastRead = data.Read(bytes, totalRead, length - totalRead);
                totalRead += lastRead;
            }

            return bytes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ReadFloatAt(this Stream data, int offset)
        {
            Span<byte> bytes = stackalloc byte[4];

            if (data.Position != offset)
                data.Position = offset;

            data.Read(bytes);

            return BitConverter.ToSingle(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToStringFromNullTerminated(this byte[] data)
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
        [PrimitiveValueMaterializer]
        public static TagRef ReadTagRefAt(this Stream data, int offset)
        {
            return new TagRef(data.ReadUInt32At(offset));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [PrimitiveValueMaterializer]
        public static TagRef<T> ReadTagRefAt<T>(this Stream data, int offset)
            where T : BaseTag
        {
            return ReadTagRefAt(data, offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [PrimitiveValueMaterializer]
        public static NormalOffset ReadNormalOffsetAt(this Stream data, int offset)
        {
            return new NormalOffset(data.ReadInt32At(offset));
        }
    }
}