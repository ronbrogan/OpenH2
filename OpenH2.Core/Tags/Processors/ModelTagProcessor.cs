using OpenH2.Core.Extensions;
using OpenH2.Core.Offsets;
using OpenH2.Core.Parsing;
using OpenH2.Core.Representations;
using System;

namespace OpenH2.Core.Tags.Processors
{
    public static class ModelTagProcessor
    {
        public static Model ProcessModel(uint id, string name, TagIndexEntry index, TrackingChunk chunk, TrackingReader sceneReader)
        {
            var span = chunk.Span;
            var tag = new Model(id)
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

            tag.BoundingBoxes = GetBoundingBoxes(span, tag.BoundingBoxCount, tag.BoundingBoxesOffset);
            tag.Lods = GetLevelOfDetails(span, tag.LodCount, tag.LodsOffset, index);
            tag.Parts = GetParts(span, tag.PartCount, tag.PartsOffset, sceneReader);
            tag.Bones = GetBones(span, tag.PartCount, tag.PartsOffset);
            tag.Markers = GetMarkers(span, tag.MarkerCount, tag.MarkersOffset);
            tag.Shaders = GetShaders(span, tag.ShaderCount, tag.ShadersOffset);

            return tag;
        }

        private static Model.BoundingBox[] GetBoundingBoxes(
            Span<byte> data, 
            int count, 
            MetaOffset offset)
        {
            var result = new Model.BoundingBox[count];
            var objLength = Model.BoundingBox.Length;

            for (var i = 0; i < count; i++)
            {
                var span = data.Slice(offset.Value + (i * objLength), objLength);

                var obj = new Model.BoundingBox()
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

        private static Model.LevelOfDetail[] GetLevelOfDetails(
            Span<byte> data, 
            int count, 
            MetaOffset offset, 
            TagIndexEntry index)
        {
            var result = new Model.LevelOfDetail[count];
            var objLength = Model.LevelOfDetail.Length;

            for (var i = 0; i < count; i++)
            {
                var span = data.Slice(offset.Value + (i * objLength), objLength);

                var obj = new Model.LevelOfDetail()
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

        private static Model.Permutation[] GetPermutations(Span<byte> data, int count, MetaOffset offset)
        {
            var result = new Model.Permutation[count];
            var objLength = Model.Permutation.Length;

            for (var i = 0; i < count; i++)
            {
                var span = data.Slice(offset.Value + (i * objLength), objLength);

                var obj = new Model.Permutation()
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

        private static Model.Part[] GetParts(Span<byte> data, int count, MetaOffset offset, TrackingReader sceneReader)
        {
            var result = new Model.Part[count];
            var objLength = Model.Part.Length;

            for (var i = 0; i < count; i++)
            {
                var span = data.Slice(offset.Value + (i * objLength), objLength);

                var obj = new Model.Part()
                {
                    Type = span.ReadUInt32At(0),
                    VertexCount = span.ReadUInt16At(4),
                    BoneCount = span.ReadUInt16At(20),
                    Data = sceneReader.Slice(
                        (int)span.ReadUInt32At(56), 
                        (int)span.ReadUInt32At(60)).ToArray(),
                    DataHeaderSize = span.ReadUInt32At(64),
                    DataBodySize = span.ReadUInt32At(68),
                    ResourceSize = span.ReadUInt32At(80),
                    ResouceOffset = span.ReadUInt32At(84),
                    
                };

                result[i] = obj;
            }

            return result;
        }

        /// <param name="offset">This offset is the offset of the preceeding Parts section</param>
        private static Model.Bone[] GetBones(Span<byte> data, int count, MetaOffset offset)
        {
            var result = new Model.Bone[count];
            var boneSectionStart = offset.Value + (count * Model.Part.Length);
            var objLength = Model.Bone.Length;

            for (var i = 0; i < count; i++)
            {
                var span = data.Slice(boneSectionStart + (i * objLength), objLength);

                var obj = new Model.Bone()
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

        private static Model.Marker[] GetMarkers(Span<byte> data, int count, MetaOffset offset)
        {
            var result = new Model.Marker[count];
            var objLength = Model.Marker.Length;

            for (var i = 0; i < count; i++)
            {
                var span = data.Slice(offset.Value + (i * objLength), objLength);

                var obj = new Model.Marker()
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

        private static Model.Shader[] GetShaders(Span<byte> data, int count, MetaOffset offset)
        {
            var result = new Model.Shader[count];
            var objLength = Model.Shader.Length;

            for (var i = 0; i < count; i++)
            {
                var span = data.Slice(offset.Value + (i * objLength), objLength);

                var obj = new Model.Shader()
                {
                    ShaderId = span.ReadUInt32At(12)
                };

                result[i] = obj;
            }

            return result;
        }

    }
}
