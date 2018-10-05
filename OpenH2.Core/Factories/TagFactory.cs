using Ionic.Zlib;
using OpenH2.Core.Enums;
using OpenH2.Core.Formats;
using OpenH2.Core.Parsing;
using OpenH2.Core.Representations.Meta;
using OpenH2.Core.Tags;
using OpenH2.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenH2.Core.Factories
{
    public static class TagFactory
    {
        private delegate TagNode ProcessMeta(BaseMeta meta, TrackingReader reader);
        private static Dictionary<Type, ProcessMeta> Processors = new Dictionary<Type, ProcessMeta>
        {
            { typeof(BitmapMeta),  ProcessBitmMeta }
        };

        public static TagNode CreateTag(BaseMeta meta, TrackingReader reader)
        {
            var metaType = meta.GetType();

            if (Processors.ContainsKey(metaType) == false)
                return null;

            return Processors[metaType](meta, reader);
        }

        private static BitmapTagNode ProcessBitmMeta(BaseMeta meta, TrackingReader reader)
        {
            var bitmMeta = (BitmapMeta)meta;

            var node = new BitmapTagNode();

            node.Meta = bitmMeta;
            node.Levels = new Memory<byte>[bitmMeta.LevelsOfDetail.Length];

            // Decompress and synthesize texture headers
            for(var i = 0; i < bitmMeta.LevelsOfDetail.Length; i++)
            {
                var lod = bitmMeta.LevelsOfDetail[i];

                if (lod.Offset.Value == 0 || lod.Offset.Value == int.MaxValue || lod.Size == 0)
                    continue;

                // TODO: Implement shared map retrieval
                if (lod.Offset.Location != DataFile.Local)
                    continue;

                var data = reader.Chunk(lod.Offset.Value, (int)lod.Size, "Bitmap");

                using (var inputStream = new MemoryStream(data.Span.ToArray()))
                using (var decompress = new ZlibStream(inputStream, CompressionMode.Decompress))
                using (var outputStream = new MemoryStream((int)inputStream.Length))
                {
                    decompress.CopyTo(outputStream);
                    outputStream.Seek(0, SeekOrigin.Begin);
                    var header = BitmUtils.GetTextureHeader(bitmMeta, (int)outputStream.Length);

                    var output = new byte[DdsHeader.Length + outputStream.Length];

                    header.CopyTo(output, 0);
                    outputStream.Read(output, DdsHeader.Length, (int)outputStream.Length);

                    node.Levels[i] = new Memory<byte>(output);
                }
            }

            return node;
        }
    }
}
