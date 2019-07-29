using System;
using OpenH2.Core.Enums.Texture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Offsets;
using OpenH2.Core.Parsing;
using OpenH2.Core.Representations;

namespace OpenH2.Core.Tags.Serialization
{
    public static class BitmapTagProcessor
    {
        public static BitmapTag ProcessBitm(uint id, string name, TagIndexEntry index, TrackingChunk chunk, TrackingReader sceneReader)
        {
            var span = chunk.Span;
            var tag = new BitmapTag(id)
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

                LevelsOfDetail = new BitmapTag.BitmapLevelOfDetail[6]
            };

            var offsetsStart = 108;
            var sizesStart = 132;

            

            tag.ID = span.ReadUInt32At(156);

            return tag;
        }
    }
}
