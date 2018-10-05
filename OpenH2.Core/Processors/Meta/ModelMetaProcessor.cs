using OpenH2.Core.Extensions;
using OpenH2.Core.Offsets;
using OpenH2.Core.Parsing;
using OpenH2.Core.Representations;
using OpenH2.Core.Representations.Meta;
using System;

namespace OpenH2.Core.Processors.Meta
{
    public static class ModelMetaProcessor
    {
        public static ModelMeta ProcessModel(string name, ObjectIndexEntry index, TrackingChunk chunk)
        {
            var span = chunk.Span;
            var meta = new ModelMeta
            {
                Name = name,
                NameId = span.ReadInt32At(0),
                BoundingBoxCount = span.ReadInt32At(20),
                BoundingBoxesOffset = new MetaOffset(index, span.ReadInt32At(24)),
                LodCount = span.ReadInt32At(28),
                LodsOffset = new MetaOffset(index, span.ReadInt32At(32)),
                PartCount = span.ReadInt32At(36),
                PartsOffset = new MetaOffset(index, span.ReadInt32At(40)),
                MarkerCount = span.ReadInt32At(72),
                MarkersOffset = new MetaOffset(index, span.ReadInt32At(76)),
                ShaderCount = span.ReadInt32At(96),
                ShadersOffset = new MetaOffset(index, span.ReadInt32At(100))
            };

            meta.BoundingBoxes = GetBoundingBoxes(span, meta.BoundingBoxCount, meta.BoundingBoxesOffset);

            return meta;
        }

        private static ModelMeta.BoundingBox[] GetBoundingBoxes(Span<byte> data, int count, MetaOffset offset)
        {
            var result = new ModelMeta.BoundingBox[count];
            var objLength = ModelMeta.BoundingBox.Length;

            for (var i = 0; i < count; i++)
            {
                var span = data.Slice(offset.Value + (i * objLength), objLength);

                var obj = new ModelMeta.BoundingBox()
                {
                    MinX = span.ReadFloatAt(0),
                    MaxX = span.ReadFloatAt(4),
                    MinY = span.ReadFloatAt(8),
                    MaxY = span.ReadFloatAt(12),
                    MinZ = span.ReadFloatAt(16),
                    MaxZ = span.ReadFloatAt(20),
                    MinU = span.ReadFloatAt(24),
                    MaxU = span.ReadFloatAt(28),
                    MinV = span.ReadFloatAt(32),
                    MaxV = span.ReadFloatAt(36),
                };

                result[i] = obj;
            }

            return result;
        }

        private static ModelMeta.LevelOfDetail[] GetLevelOfDetails(Span<byte> data, int count, MetaOffset offset)
        {
            var result = new ModelMeta.LevelOfDetail[count];
            var objLength = ModelMeta.LevelOfDetail.Length;

            for (var i = 0; i < count; i++)
            {
                var span = data.Slice(offset.Value + (count * objLength), objLength);

                var obj = new ModelMeta.LevelOfDetail()
                {

                };

                result[i] = obj;
            }

            return result;
        }

        private static ModelMeta.Part[] GetParts(Span<byte> data, int count, MetaOffset offset)
        {
            var result = new ModelMeta.Part[count];
            var objLength = ModelMeta.Part.Length;

            for (var i = 0; i < count; i++)
            {
                var span = data.Slice(offset.Value + (count * objLength), objLength);

                var obj = new ModelMeta.Part()
                {

                };

                result[i] = obj;
            }

            return result;
        }

        private static ModelMeta.Marker[] GetMarkers(Span<byte> data, int count, MetaOffset offset)
        {
            var result = new ModelMeta.Marker[count];
            var objLength = ModelMeta.Marker.Length;

            for (var i = 0; i < count; i++)
            {
                var span = data.Slice(offset.Value + (count * objLength), objLength);

                var obj = new ModelMeta.Marker()
                {

                };

                result[i] = obj;
            }

            return result;
        }

        private static ModelMeta.Shader[] GetShaders(Span<byte> data, int count, MetaOffset offset)
        {
            var result = new ModelMeta.Shader[count];
            var objLength = ModelMeta.Shader.Length;

            for (var i = 0; i < count; i++)
            {
                var span = data.Slice(offset.Value + (count * objLength), objLength);

                var obj = new ModelMeta.Shader()
                {

                };

                result[i] = obj;
            }

            return result;
        }

    }
}
