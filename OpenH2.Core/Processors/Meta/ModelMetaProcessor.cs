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
            meta.Lods = GetLevelOfDetails(span, meta.LodCount, meta.LodsOffset, index);
            meta.Parts = GetParts(span, meta.PartCount, meta.PartsOffset);
            meta.Bones = GetBones(span, meta.PartCount, meta.PartsOffset);
            meta.Markers = GetMarkers(span, meta.MarkerCount, meta.MarkersOffset);
            meta.Shaders = GetShaders(span, meta.ShaderCount, meta.ShadersOffset);

            return meta;
        }

        private static ModelMeta.BoundingBox[] GetBoundingBoxes(
            Span<byte> data, 
            int count, 
            MetaOffset offset)
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

        private static ModelMeta.LevelOfDetail[] GetLevelOfDetails(
            Span<byte> data, 
            int count, 
            MetaOffset offset, 
            ObjectIndexEntry index)
        {
            var result = new ModelMeta.LevelOfDetail[count];
            var objLength = ModelMeta.LevelOfDetail.Length;

            for (var i = 0; i < count; i++)
            {
                var span = data.Slice(offset.Value + (i * objLength), objLength);

                var obj = new ModelMeta.LevelOfDetail()
                {
                    PartNameId = span.ReadInt32At(0),
                    PermutationCount = span.ReadInt32At(8),
                    PermutationsOffset = new MetaOffset(index, span.ReadInt32At(12))
                };

                obj.Permutations = GetPermutations(data, obj.PermutationCount, obj.PermutationsOffset);

                result[i] = obj;
            }

            return result;
        }

        private static ModelMeta.Permutation[] GetPermutations(Span<byte> data, int count, MetaOffset offset)
        {
            var result = new ModelMeta.Permutation[count];
            var objLength = ModelMeta.Permutation.Length;

            for (var i = 0; i < count; i++)
            {
                var span = data.Slice(offset.Value + (i * objLength), objLength);

                var obj = new ModelMeta.Permutation()
                {
                    PermutationNameId = span.ReadInt32At(0),
                    LowestPieceIndex = span.ReadInt16At(4),
                    LowPieceIndex = span.ReadInt16At(6),
                    MediumLowPieceIndex = span.ReadInt16At(8),
                    MediumHighPieceIndex = span.ReadInt16At(10),
                    HighPieceIndex = span.ReadInt16At(12),
                    HighestPieceIndex = span.ReadInt16At(14),
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
                var span = data.Slice(offset.Value + (i * objLength), objLength);

                var obj = new ModelMeta.Part()
                {
                    Type = span.ReadUInt32At(0),
                    VertexCount = span.ReadUInt16At(4),
                    BoneCount = span.ReadUInt16At(20),
                    DataOffset = span.ReadUInt32At(56),
                    DataSize = span.ReadUInt32At(60),
                    DataHeaderSize = span.ReadUInt32At(64),
                    DataBodySize = span.ReadUInt32At(68),
                    ResourceSize = span.ReadUInt32At(80),
                    ResouceOffset = span.ReadUInt32At(84)
                };

                result[i] = obj;
            }

            return result;
        }

        /// <param name="offset">This offset is the offset of the preceeding Parts section</param>
        private static ModelMeta.Bone[] GetBones(Span<byte> data, int count, MetaOffset offset)
        {
            var result = new ModelMeta.Bone[count];
            var boneSectionStart = offset.Value + (count * ModelMeta.Part.Length);
            var objLength = ModelMeta.Bone.Length;

            for (var i = 0; i < count; i++)
            {
                var span = data.Slice(boneSectionStart + (i * objLength), objLength);

                var obj = new ModelMeta.Bone()
                {
                    BoneNameId = span.ReadInt32At(0),
                    ParentIndex = span.ReadInt16At(4),
                    FirstChildIndex = span.ReadInt16At(6),
                    NextSiblingIndex = span.ReadInt16At(8),
                    X = span.ReadFloatAt(12),
                    Y = span.ReadFloatAt(16),
                    Z = span.ReadFloatAt(20),
                    Yaw  = span.ReadFloatAt(24),
                    Pitch = span.ReadFloatAt(28),
                    Roll = span.ReadFloatAt(32),
                    SingleBoneX = span.ReadFloatAt(70),
                    SingleBoneY = span.ReadFloatAt(74),
                    SingleBoneZ = span.ReadFloatAt(78),
                    DistanceFromParent = span.ReadFloatAt(82)
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
                var span = data.Slice(offset.Value + (i * objLength), objLength);

                var obj = new ModelMeta.Marker()
                {
                    MarkerNameId = span.ReadInt32At(0),
                    X = span.ReadFloatAt(36),
                    Y = span.ReadFloatAt(40),
                    Z = span.ReadFloatAt(44),
                    Yaw = span.ReadFloatAt(48),
                    Pitch = span.ReadFloatAt(52),
                    Roll = span.ReadFloatAt(56),
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
                var span = data.Slice(offset.Value + (i * objLength), objLength);

                var obj = new ModelMeta.Shader()
                {
                    ShaderId = span.ReadUInt32At(12)
                };

                result[i] = obj;
            }

            return result;
        }

    }
}
