using OpenH2.Core.Extensions;
using OpenH2.Core.Parsing;
using OpenH2.Core.Representations;
using System;
using System.Collections.Generic;
using OpenH2.Core.Offsets;

namespace OpenH2.Core.Tags.Processors
{
    public static class BspTagProcessor
    {
        public static BspTag ProcessBsp(uint id, string name, TagIndexEntry index, TrackingChunk chunk, TrackingReader sceneReader)
        {
            var data = chunk.Span;
            var meta = new BspTag(id)
            {
                Name = name,
                RawMeta = data.ToArray()
            };

            meta.Header = GetHeader(data, index);
            meta.RawBlocks = GetRawBlocks(data, meta.Header);

            return meta;
        }

        private static BspTag.BspHeader GetHeader(Span<byte> data, TagIndexEntry index)
        {
            var header = new BspTag.BspHeader()
            {
                Checksum = data.ReadInt32At(8),
                ShaderLocation = data.ReadMetaCaoAt(12, index),
                RawBlockLocation = data.ReadMetaCaoAt(20, index),
                // TODO: 48 byte block - collision data?
                MiscBlocks = ReadMiscBlocks(data, index),
            };

            header.Shaders = ReadShaders(data, header);
            header.RawBlocks = ReadRawBlocks(data, header, index);

            return header;
        }

        private static List<BspTag.RawBlock> GetRawBlocks(Span<byte> data, BspTag.BspHeader header)
        {
            var rawBlocks = new List<BspTag.RawBlock>();

            foreach(var rawSection in header.RawBlocks)
            {
                var raws = new BspTag.RawBlock();

                raws.RawObject1s = GetRawObject1s(data, rawSection.RawObject1Cao);
                raws.RawObject2s = GetRawObject2s(data, rawSection.RawObject2Cao);
                raws.RawObject3s = GetRawObject3s(data, rawSection.RawObject3Cao);
                raws.RawObject4s = GetRawObject4s(data, rawSection.RawObject4Cao);
                raws.RawObject5s = GetRawObject5s(data, rawSection.RawObject5Cao);
                raws.Faces = GetFaces(data, rawSection.FacesCao);
                raws.HalfEdges = GetHalfEdgeStructure(data, rawSection.HalfEdgeCao);
                raws.Verticies = GetVerticies(data, rawSection.VerticiesCao);

                rawBlocks.Add(raws);
            }

            return rawBlocks;
        }

        private static List<BspTag.RawObject1> GetRawObject1s(Span<byte> data, CountAndOffset cao)
        {
            var span = data.Slice(cao.Offset.Value, cao.Count * BspTag.RawObject1.Length);

            var raws = new List<BspTag.RawObject1>(cao.Count);

            for (var i = 0; i < cao.Count; i++)
            {
                var offset = i * BspTag.RawObject1.Length;

                var raw = new BspTag.RawObject1()
                {
                    val1 = span.ReadUInt16At(0 + offset),
                    val2 = span.ReadUInt16At(2 + offset),
                    unknown1 = span.ReadUInt16At(4 + offset),
                    unknown2 = span.ReadUInt16At(6 + offset)
                };

                raws.Add(raw);
            }

            return raws;
        }

        private static List<BspTag.RawObject2> GetRawObject2s(Span<byte> data, CountAndOffset cao)
        {
            var span = data.Slice(cao.Offset.Value, cao.Count * BspTag.RawObject2.Length);

            var raws = new List<BspTag.RawObject2>(cao.Count);

            for (var i = 0; i < cao.Count; i++)
            {
                var offset = i * BspTag.RawObject2.Length;

                var raw = new BspTag.RawObject2()
                {
                    x = span.ReadFloatAt(0 + offset),
                    y = span.ReadFloatAt(4 + offset),
                    z = span.ReadFloatAt(8 + offset),
                    w = span.ReadFloatAt(12 + offset)
                };

                raws.Add(raw);
            }

            return raws;
        }

        private static List<BspTag.RawObject3> GetRawObject3s(Span<byte> data, CountAndOffset cao)
        {
            var span = data.Slice(cao.Offset.Value, cao.Count * BspTag.RawObject3.Length);

            var raws = new List<BspTag.RawObject3>(cao.Count);

            for (var i = 0; i < cao.Count; i++)
            {
                var offset = i * BspTag.RawObject3.Length;

                var raw = new BspTag.RawObject3()
                {
                    val1 = span.ReadUInt16At(0 + offset),
                    val2 = span.ReadUInt16At(2 + offset),
                };

                raws.Add(raw);
            }

            return raws;
        }

        private static List<BspTag.RawObject4> GetRawObject4s(Span<byte> data, CountAndOffset cao)
        {
            var span = data.Slice(cao.Offset.Value, cao.Count * BspTag.RawObject4.Length);

            var raws = new List<BspTag.RawObject4>(cao.Count);

            for (var i = 0; i < cao.Count; i++)
            {
                var offset = i * BspTag.RawObject4.Length;

                var raw = new BspTag.RawObject4()
                {
                    val1 = span.ReadUInt16At(0 + offset),
                    val2 = span.ReadUInt16At(2 + offset),
                };

                raws.Add(raw);
            }

            return raws;
        }

        private static List<BspTag.RawObject5> GetRawObject5s(Span<byte> data, CountAndOffset cao)
        {
            var span = data.Slice(cao.Offset.Value, cao.Count * BspTag.RawObject5.Length);

            var raws = new List<BspTag.RawObject5>(cao.Count);

            for (var i = 0; i < cao.Count; i++)
            {
                var offset = i * BspTag.RawObject5.Length;

                var raw = new BspTag.RawObject5()
                {
                    x = span.ReadFloatAt(0 + offset),
                    y = span.ReadFloatAt(4 + offset),
                    z = span.ReadFloatAt(8 + offset),
                    u = span.ReadInt16At(12 + offset),
                    v = span.ReadInt16At(14 + offset)
                };

                raws.Add(raw);
            }

            return raws;
        }

        private static List<BspTag.Face> GetFaces(Span<byte> data, CountAndOffset cao)
        {
            var span = data.Slice(cao.Offset.Value, cao.Count * BspTag.Face.Length);

            var raws = new List<BspTag.Face>(cao.Count);

            for (var i = 0; i < cao.Count; i++)
            {
                var offset = i * BspTag.Face.Length;

                var raw = new BspTag.Face()
                {
                    val1 = span.ReadUInt16At(0 + offset),
                    FirstEdge = span.ReadUInt16At(2 + offset),
                    val3 = span.ReadUInt16At(4 + offset),
                    val4 = span.ReadUInt16At(6 + offset),
                };

                raws.Add(raw);
            }

            return raws;
        }

        private static List<BspTag.HalfEdgeContainer> GetHalfEdgeStructure(Span<byte> data, CountAndOffset cao)
        {
            var span = data.Slice(cao.Offset.Value, cao.Count * BspTag.HalfEdgeContainer.Length);

            var raws = new List<BspTag.HalfEdgeContainer>(cao.Count);

            for (var i = 0; i < cao.Count; i++)
            {
                var offset = i * BspTag.HalfEdgeContainer.Length;

                var raw = new BspTag.HalfEdgeContainer()
                {
                    Vertex0 = span.ReadUInt16At(0 + offset),
                    Vertex1 = span.ReadUInt16At(2 + offset),
                    NextEdge = span.ReadUInt16At(4 + offset),
                    PrevEdge = span.ReadUInt16At(6 + offset),
                    Face0 = span.ReadUInt16At(8 + offset),
                    Face1 = span.ReadUInt16At(10 + offset)
                };

                raws.Add(raw);
            }

            return raws;
        }
        private static List<BspTag.Vertex> GetVerticies(Span<byte> data, CountAndOffset cao)
        {
            var span = data.Slice(cao.Offset.Value, cao.Count * BspTag.Vertex.Length);

            var raws = new List<BspTag.Vertex>(cao.Count);

            for (var i = 0; i < cao.Count; i++)
            {
                var offset = i * BspTag.Vertex.Length;

                var raw = new BspTag.Vertex()
                {
                    x = span.ReadFloatAt(0 + offset),
                    y = span.ReadFloatAt(4 + offset),
                    z = span.ReadFloatAt(8 + offset),
                    edge = span.ReadInt32At(12 + offset)
                };

                raws.Add(raw);
            }

            return raws;
        }

        private static BspTag.MiscBlockCaos ReadMiscBlocks(Span<byte> data, TagIndexEntry index)
        {
            var caosData = data.Slice(76, 496);

            return new BspTag.MiscBlockCaos()
            {
                MiscObject1Cao = caosData.ReadMetaCaoAt(0, index),
                MiscObject2Cao = caosData.ReadMetaCaoAt(8, index),
                MiscObject3Cao = caosData.ReadMetaCaoAt(16, index),
                MiscObject4Cao = caosData.ReadMetaCaoAt(24, index),
                // 5 cao gap
                MiscObject5Cao = caosData.ReadMetaCaoAt(72, index),
                MiscObject6Cao = caosData.ReadMetaCaoAt(80, index),
                MiscObject7Cao = caosData.ReadMetaCaoAt(88, index),
                MiscObject8Cao = caosData.ReadMetaCaoAt(96, index),
                // 4 cao gap
                MiscObject9Cao = caosData.ReadMetaCaoAt(136, index),
                MiscObject10Cao = caosData.ReadMetaCaoAt(144, index),
                // 3 cao gap
                MiscObject11Cao = caosData.ReadMetaCaoAt(176, index),
                MiscObject12Cao = caosData.ReadMetaCaoAt(184, index),
                // 4 cao gap + 4 bytes?
                MiscObject13Cao = caosData.ReadMetaCaoAt(236, index),
                MiscObject14Cao = caosData.ReadMetaCaoAt(244, index),
                MiscObject15Cao = caosData.ReadMetaCaoAt(252, index),
                MiscObject16Cao = caosData.ReadMetaCaoAt(260, index),
                MiscObject17Cao = caosData.ReadMetaCaoAt(268, index),
                // 14 cao gap
                MiscObject18Cao = caosData.ReadMetaCaoAt(388, index),
                // 8 bytes unknown
                MiscObject19Cao = caosData.ReadMetaCaoAt(404, index),
                // 52 bytes unknown
                MiscObject20Cao = caosData.ReadMetaCaoAt(464, index),
                MiscObject21Cao = caosData.ReadMetaCaoAt(472, index),
                MiscObject22Cao = caosData.ReadMetaCaoAt(480, index),
                MiscObject23Cao = caosData.ReadMetaCaoAt(488, index),
            };
        }

        private static List<BspTag.ShaderInfo> ReadShaders(Span<byte> data, BspTag.BspHeader header)
        {
            var cao = header.ShaderLocation;

            var shaders = new List<BspTag.ShaderInfo>(cao.Count);

            for (var i = 0; i < cao.Count; i++)
            {
                var shaderData = data.Slice(
                    cao.Offset.Value + i * BspTag.ShaderInfo.Length,
                    BspTag.ShaderInfo.Length);

                shaders.Add(new BspTag.ShaderInfo
                {
                    Tag = shaderData.ReadStringFrom(0, 4),
                    Unknown = shaderData.ReadInt32At(4),
                    Value1 = shaderData.ReadInt32At(8),
                    OldTag = shaderData.ReadStringFrom(12, 4),
                    Value2 = shaderData.ReadInt32At(16)
                });
            }

            return shaders;
        }

        private static List<BspTag.RawBlockCaos> ReadRawBlocks(
            Span<byte> data,
            BspTag.BspHeader header,
            TagIndexEntry index)
        {
            var cao = header.RawBlockLocation;
            var rawBlocks = new List<BspTag.RawBlockCaos>(cao.Count);

            for (int i = 0; i < cao.Count; i++)
            {
                var blockData = data.Slice(
                    cao.Offset.Value + i * BspTag.RawBlockCaos.Length,
                    BspTag.RawBlockCaos.Length);

                rawBlocks.Add(new BspTag.RawBlockCaos()
                {
                    RawObject1Cao = blockData.ReadMetaCaoAt(0, index),
                    RawObject2Cao = blockData.ReadMetaCaoAt(8, index),
                    RawObject3Cao = blockData.ReadMetaCaoAt(16, index),
                    RawObject4Cao = blockData.ReadMetaCaoAt(24, index),
                    RawObject5Cao = blockData.ReadMetaCaoAt(32, index),
                    FacesCao = blockData.ReadMetaCaoAt(40, index),
                    HalfEdgeCao = blockData.ReadMetaCaoAt(48, index),
                    VerticiesCao = blockData.ReadMetaCaoAt(56, index),
                    Unknown = blockData.ReadInt32At(64)
                });
            }

            return rawBlocks;
        }
    }
}
