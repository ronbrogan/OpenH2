using OpenH2.Core.Enums;
using OpenH2.Core.Meta;
using OpenH2.Core.Parsing;
using OpenH2.Core.Utilities;
using System;
using System.IO;
using System.IO.Compression;

namespace OpenH2.Core.Tags.Processors
{
    public static class BitmapTagProcessor
    {
        public static BitmapTagNode ProcessBitmapMeta(BaseMeta meta, TrackingReader reader)
        {
            var bitmMeta = (BitmapMeta)meta;

            var node = new BitmapTagNode();

            node.Meta = bitmMeta;
            node.Levels = new Memory<byte>[bitmMeta.LevelsOfDetail.Length];

            // Decompress and synthesize texture headers
            for (var i = 0; i < bitmMeta.LevelsOfDetail.Length; i++)
            {
                var lod = bitmMeta.LevelsOfDetail[i];

                if (lod.Offset.Value == 0 || lod.Offset.Value == int.MaxValue || lod.Size == 0)
                    continue;

                // TODO: Implement shared map retrieval
                if (lod.Offset.Location != DataFile.Local)
                    continue;

                var data = reader.Chunk(lod.Offset.Value, (int)lod.Size, "Bitmap");

                // Need to offset 2 bytes into data to bypass zlib header for compatibility with DeflateStream
                using (var inputStream = new MemoryStream(data.Span.Slice(2).ToArray()))
                using (var decompress = new DeflateStream(inputStream, CompressionMode.Decompress))
                using (var outputStream = new MemoryStream((int)inputStream.Length))
                {
                    BitmUtils.WriteTextureHeader(bitmMeta, outputStream);
                    decompress.CopyTo(outputStream);

                    node.Levels[i] = new Memory<byte>(outputStream.GetBuffer()).Slice(0, (int)outputStream.Length);
                }
            }

            return node;
        }
    }
}
