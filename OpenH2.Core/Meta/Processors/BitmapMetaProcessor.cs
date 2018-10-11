using OpenH2.Core.Enums.Texture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Offsets;
using OpenH2.Core.Parsing;
using OpenH2.Core.Representations;

namespace OpenH2.Core.Meta.Processors
{
    public static class BitmapMetaProcessor
    {
        public static BitmapMeta ProcessBitm(string name, ObjectIndexEntry index, TrackingChunk chunk)
        {
            var span = chunk.Span;
            var meta = new BitmapMeta
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

                LevelsOfDetail = new BitmapMeta.BitmapLevelOfDetail[6]
            };

            var offsetsStart = 108;
            var sizesStart = 132;

            for (int i = 0; i < 6; i++)
            {
                var lod = new BitmapMeta.BitmapLevelOfDetail();

                lod.Offset = new NormalOffset((int)span.ReadUInt32At(offsetsStart + (i * 4)));
                lod.Size = span.ReadUInt32At(sizesStart + (i * 4));

                meta.LevelsOfDetail[i] = lod;
            }

            meta.ID = span.ReadUInt32At(156);

            return meta;
        }
    }
}
