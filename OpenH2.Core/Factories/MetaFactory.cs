using OpenH2.Core.Enums.Texture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Offsets;
using OpenH2.Core.Parsing;
using OpenH2.Core.Representations;
using OpenH2.Core.Representations.Meta;
using System.Collections.Generic;

namespace OpenH2.Core.Factories
{
    public static class MetaFactory
    {
        private delegate BaseMeta ProcessMeta(string name, ObjectIndexEntry entry, TrackingChunk chunk);
        private static readonly Dictionary<string, ProcessMeta> ProcessMap = new Dictionary<string, ProcessMeta>
        {
            { "bitm", ProcessBitm }
        };

        public static BaseMeta GetMeta(string name, ObjectIndexEntry index, TrackingChunk chunk)
        {
            if (ProcessMap.ContainsKey(index.Tag) == false)
                return null;

            return ProcessMap[index.Tag](name, index, chunk);
        }

        private static BitmMeta ProcessBitm(string name, ObjectIndexEntry index, TrackingChunk chunk)
        {
            var span = chunk.Span;
            var meta = new BitmMeta
            {
                Name = name,
                TextureType = (TextureType)span.ReadInt16At(0),
                TextureFormat = (TextureFormat)span.ReadInt16At(2),
                TextureUsage = (TextureUsage)span.ReadInt16At(4),
                MipMapCount = span.ReadInt16At(52),

                Tag = span.ReadStringFrom(80, 4),
                Width = span.ReadInt16At(84),
                Height = span.ReadInt16At(86),
                Depth = span.ReadInt16At(88),
                Type = span.ReadInt16At(90),
                Format = span.ReadInt16At(92),
                Properties = (TextureProperties)span.ReadInt16At(94),
                RegX = span.ReadInt16At(96),
                RegY = span.ReadInt16At(98),
                MipMapCount2 = span.ReadInt16At(100),
                PixelOffset = span.ReadInt16At(102),

                LevelsOfDetail = new BitmLevelOfDetail[6]
            };

            var offsetsStart = 108;
            var sizesStart = 132;

            for (int i = 0; i < 6; i++)
            {
                var lod = new BitmLevelOfDetail();

                lod.Offset = new NormalOffset((int)span.ReadUInt32At(offsetsStart + (i * 4)));
                lod.Size = span.ReadUInt32At(sizesStart + (i * 4));

                meta.LevelsOfDetail[i] = lod;
            }

            meta.ID = span.ReadUInt32At(156);

            return meta;
        }
    }
}
