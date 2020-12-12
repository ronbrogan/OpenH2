using OpenH2.Core.Extensions;
using OpenBlam.Serialization;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace OpenH2.Core.Maps.MCC
{
    public static class H2mccCompression
    {
        public const int CompressionChunkSize = 1 << 18;
        public const uint DecompressedFourCC = 843608143u; // OpH2
        public const uint RealFourCC = 1751474532u; // daeh

        private static ArrayPool<byte> copyPool = ArrayPool<byte>.Create();

        /// <summary>
        /// Decompresses the provided stream into the other stream, if the map has not already been decompressed.
        /// If the provided stream has already been decompressed, it will simply copy and return false
        /// </summary>
        /// <returns>A boolean indicating if decompression was performed</returns>
        public static bool Decompress(Stream compressed, Stream decompressTo)
        {
            if (compressed.CanSeek == false) throw new NotSupportedException("Must be able to seek on compressed");
            
            var fourCC = compressed.ReadUInt32At(0);

            if(fourCC == DecompressedFourCC)
            {
                compressed.Position = 0;
                compressed.CopyTo(decompressTo);
                return false;
            }

            if(fourCC != RealFourCC)
            {
                throw new Exception("Provided map stream was not valid");
            }

            var info = BlamSerializer.Deserialize<H2mccCompressionSections>(compressed);

            var header = new Span<byte>(new byte[4096]);

            compressed.Position = 0;
            compressed.Read(header);
            decompressTo.WriteUInt32(DecompressedFourCC);
            decompressTo.Write(header.Slice(4));

            foreach (var section in info.Sections)
            {
                if (section.Offset == 0 || section.Count == 0)
                    continue;

                // Looks like compression is optional and count is negative when that happens
                if(section.Count < 0)
                {
                    var realCount = -section.Count;
                    var buf = copyPool.Rent(realCount);
                    compressed.Position = section.Offset;
                    var readCount = compressed.Read(buf);
                    
                    if(readCount != realCount)
                    {
                        copyPool.Return(buf);
                        throw new Exception("Unable to read the amount of data required");
                    }

                    decompressTo.Write(((Span<byte>)buf).Slice(0, readCount));
                    copyPool.Return(buf);
                }
                else
                {
                    compressed.Seek(section.Offset + 2, SeekOrigin.Begin);
                    using var deflate = new DeflateStream(compressed, CompressionMode.Decompress, leaveOpen: true);
                    deflate.CopyTo(decompressTo);
                }
            }

            return true;
        }

        /// <summary>
        /// Decompresses the provided stream into the other stream, if the map has not already been decompressed.
        /// If the provided stream has already been decompressed, it will simply return the original stream
        /// </summary>
        public static Stream DecompressInline(Stream input)
        {
            using var compressed = input;
            if (compressed.CanSeek == false) throw new NotSupportedException("Must be able to seek on compressed");

            var fourCC = compressed.ReadUInt32At(0);

            if (fourCC == DecompressedFourCC)
            {
                compressed.Position = 0;
                return compressed;
            }

            if (fourCC != RealFourCC)
            {
                throw new Exception("Provided map stream was not valid");
            }

            var decompressTo = new MemoryStream();

            var info = BlamSerializer.Deserialize<H2mccCompressionSections>(compressed);

            var header = new Span<byte>(new byte[4096]);

            compressed.Position = 0;
            compressed.Read(header);
            decompressTo.WriteUInt32(DecompressedFourCC);
            decompressTo.Write(header.Slice(4));

            foreach (var section in info.Sections)
            {
                if (section.Offset == 0 || section.Count == 0)
                    continue;

                // Looks like compression is optional and count is negative when that happens
                if (section.Count < 0)
                {
                    var realCount = -section.Count;
                    var buf = copyPool.Rent(realCount);
                    compressed.Position = section.Offset;
                    var readCount = compressed.Read(buf);

                    if (readCount != realCount)
                    {
                        copyPool.Return(buf);
                        throw new Exception("Unable to read the amount of data required");
                    }

                    decompressTo.Write(((Span<byte>)buf).Slice(0, readCount));
                    copyPool.Return(buf);
                }
                else
                {
                    compressed.Seek(section.Offset + 2, SeekOrigin.Begin);
                    using var deflate = new DeflateStream(compressed, CompressionMode.Decompress, leaveOpen: true);
                    deflate.CopyTo(decompressTo);
                }
            }

            decompressTo.Position = 0;

            return decompressTo;
        }

        /// <summary>
        /// Compresses the provided stream into the other stream, if the map was previously decompressed.
        /// If the provided stream was not previously decompressed, it will simply copy and return false
        /// </summary>
        /// <returns>A boolean indicating if compression was performed</returns>
        public static bool Compress(Stream decompressed, Stream compressTo)
        {
            if (decompressed.CanSeek == false) throw new NotSupportedException("Must be able to seek on decompressed");
            if (compressTo.CanSeek == false) throw new NotSupportedException("Must be able to seek on compressTo");

            var fourCC = decompressed.ReadUInt32At(0);

            if (fourCC == RealFourCC)
            {
                decompressed.Position = 0;
                decompressed.CopyTo(compressTo);
                return false;
            }

            if (fourCC != DecompressedFourCC)
            {
                throw new Exception("Provided decompressed map stream was not valid for compression");
            }

            decompressed.Position = 0;
            compressTo.Position = 0;

            var header = new Span<byte>(new byte[BlamSerializer.SizeOf<H2mccMapHeader>()]);

            // Copy header
            decompressed.Read(header);
            compressTo.WriteUInt32(RealFourCC);
            compressTo.Write(header.Slice(4));

            // Write empty compression info until we're done
            var compressionSections = new Span<byte>(new byte[8192]);
            compressTo.Write(compressionSections);

            var sections = new List<H2mccCompressionSections.CompressionSection>();

            var chunk = new Span<byte>(new byte[CompressionChunkSize]);

            // Compress chunks, write to compressTo
            while (decompressed.Position < decompressed.Length - 1)
            {
                var bytesToTake = Math.Min(CompressionChunkSize, decompressed.Length - decompressed.Position);
                var readBytes = decompressed.Read(chunk);

                Debug.Assert(readBytes == bytesToTake);

                using var compressed = new MemoryStream();
                using (var compressor = new DeflateStream(compressed, CompressionLevel.Optimal, true))
                {
                    compressor.Write(chunk.ToArray(), 0, readBytes);
                }

                compressed.Seek(0, SeekOrigin.Begin);

                var section = new H2mccCompressionSections.CompressionSection((int)compressed.Length + 2, (uint)compressTo.Position);
                sections.Add(section);

                // Write magic bytes
                compressTo.Write(BitConverter.GetBytes((ushort)5416));

                compressed.CopyTo(compressTo);
            }

            // Go back and write compression section info
            compressTo.Seek(BlamSerializer.SizeOf<H2mccMapHeader>(), SeekOrigin.Begin);
            foreach (var section in sections)
            {
                compressTo.WriteInt32(section.Count);
                compressTo.WriteUInt32(section.Offset);
            }

            return true;
        }
    }
}
