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
        public static Bsp ProcessBsp(uint id, string name, TagIndexEntry index, TrackingChunk chunk, TrackingReader sceneReader)
        {
            var data = chunk.Span;
            var meta = new Bsp(id)
            {
                Name = name,
                RawMeta = data.ToArray()
            };

            meta.Header = GetHeader(data, index);
            meta.RawBlocks = GetRawBlocks(data, meta.Header);

            return meta;
        }

        private static Bsp.BspHeader GetHeader(Span<byte> data, TagIndexEntry index)
        {
            var header = new Bsp.BspHeader()
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

        private static List<Bsp.RawBlock> GetRawBlocks(Span<byte> data, Bsp.BspHeader header)
        {
            var rawBlocks = new List<Bsp.RawBlock>();

            foreach(var rawSection in header.RawBlocks)
            {
                var raws = new Bsp.RawBlock();

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

        private static List<Bsp.RawObject1> GetRawObject1s(Span<byte> data, CountAndOffset cao)
        {
            var span = data.Slice(cao.Offset.Value, cao.Count * Bsp.RawObject1.Length);

            var raws = new List<Bsp.RawObject1>(cao.Count);

            for (var i = 0; i < cao.Count; i++)
            {
                var offset = i * Bsp.RawObject1.Length;

                var raw = new Bsp.RawObject1()
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

        private static List<Bsp.RawObject2> GetRawObject2s(Span<byte> data, CountAndOffset cao)
        {
            var span = data.Slice(cao.Offset.Value, cao.Count * Bsp.RawObject2.Length);

            var raws = new List<Bsp.RawObject2>(cao.Count);

            for (var i = 0; i < cao.Count; i++)
            {
                var offset = i * Bsp.RawObject2.Length;

                var raw = new Bsp.RawObject2()
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

        private static List<Bsp.RawObject3> GetRawObject3s(Span<byte> data, CountAndOffset cao)
        {
            var span = data.Slice(cao.Offset.Value, cao.Count * Bsp.RawObject3.Length);

            var raws = new List<Bsp.RawObject3>(cao.Count);

            for (var i = 0; i < cao.Count; i++)
            {
                var offset = i * Bsp.RawObject3.Length;

                var raw = new Bsp.RawObject3()
                {
                    val1 = span.ReadUInt16At(0 + offset),
                    val2 = span.ReadUInt16At(2 + offset),
                };

                raws.Add(raw);
            }

            return raws;
        }

        private static List<Bsp.RawObject4> GetRawObject4s(Span<byte> data, CountAndOffset cao)
        {
            var span = data.Slice(cao.Offset.Value, cao.Count * Bsp.RawObject4.Length);

            var raws = new List<Bsp.RawObject4>(cao.Count);

            for (var i = 0; i < cao.Count; i++)
            {
                var offset = i * Bsp.RawObject4.Length;

                var raw = new Bsp.RawObject4()
                {
                    val1 = span.ReadUInt16At(0 + offset),
                    val2 = span.ReadUInt16At(2 + offset),
                };

                raws.Add(raw);
            }

            return raws;
        }

        private static List<Bsp.RawObject5> GetRawObject5s(Span<byte> data, CountAndOffset cao)
        {
            var span = data.Slice(cao.Offset.Value, cao.Count * Bsp.RawObject5.Length);

            var raws = new List<Bsp.RawObject5>(cao.Count);

            for (var i = 0; i < cao.Count; i++)
            {
                var offset = i * Bsp.RawObject5.Length;

                var raw = new Bsp.RawObject5()
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

        private static List<Bsp.Face> GetFaces(Span<byte> data, CountAndOffset cao)
        {
            var span = data.Slice(cao.Offset.Value, cao.Count * Bsp.Face.Length);

            var raws = new List<Bsp.Face>(cao.Count);

            for (var i = 0; i < cao.Count; i++)
            {
                var offset = i * Bsp.Face.Length;

                var raw = new Bsp.Face()
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

        private static List<Bsp.HalfEdgeContainer> GetHalfEdgeStructure(Span<byte> data, CountAndOffset cao)
        {
            var span = data.Slice(cao.Offset.Value, cao.Count * Bsp.HalfEdgeContainer.Length);

            var raws = new List<Bsp.HalfEdgeContainer>(cao.Count);

            for (var i = 0; i < cao.Count; i++)
            {
                var offset = i * Bsp.HalfEdgeContainer.Length;

                var raw = new Bsp.HalfEdgeContainer()
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
        private static List<Bsp.Vertex> GetVerticies(Span<byte> data, CountAndOffset cao)
        {
            var span = data.Slice(cao.Offset.Value, cao.Count * Bsp.Vertex.Length);

            var raws = new List<Bsp.Vertex>(cao.Count);

            for (var i = 0; i < cao.Count; i++)
            {
                var offset = i * Bsp.Vertex.Length;

                var raw = new Bsp.Vertex()
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

        private static Bsp.MiscBlockCaos ReadMiscBlocks(Span<byte> data, TagIndexEntry index)
        {
            var caosData = data.Slice(76, 496);

            return new Bsp.MiscBlockCaos()
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

        private static List<Bsp.ShaderInfo> ReadShaders(Span<byte> data, Bsp.BspHeader header)
        {
            var cao = header.ShaderLocation;

            var shaders = new List<Bsp.ShaderInfo>(cao.Count);

            for (var i = 0; i < cao.Count; i++)
            {
                var shaderData = data.Slice(
                    cao.Offset.Value + i * Bsp.ShaderInfo.Length,
                    Bsp.ShaderInfo.Length);

                shaders.Add(new Bsp.ShaderInfo
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

        private static List<Bsp.RawBlockCaos> ReadRawBlocks(
            Span<byte> data,
            Bsp.BspHeader header,
            TagIndexEntry index)
        {
            var cao = header.RawBlockLocation;
            var rawBlocks = new List<Bsp.RawBlockCaos>(cao.Count);

            for (int i = 0; i < cao.Count; i++)
            {
                var blockData = data.Slice(
                    cao.Offset.Value + i * Bsp.RawBlockCaos.Length,
                    Bsp.RawBlockCaos.Length);

                rawBlocks.Add(new Bsp.RawBlockCaos()
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
