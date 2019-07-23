using System;
using OpenH2.Core.Tags;
using OpenH2.Core.Types;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenH2.Core.Extensions;
using System.Collections.Concurrent;

namespace OpenH2.Translation.TagData.Processors
{
    public class BspTagDataProcessor
    {

        public static BspTagData ProcessTag(BaseTag tag)
        {
            var bsp = tag as Bsp;

            if (bsp == null)
                throw new ArgumentException("Tag must be a Bsp", nameof(tag));

            var tagData = new BspTagData(bsp);
            tagData.RenderModels = new BspTagData.RenderModel[bsp.RenderChunks.Length];

            for(var c = 0; c < bsp.RenderChunks.Length; c++)
            {
                var chunk = bsp.RenderChunks[c];

                var model = new BspTagData.RenderModel();
                model.Verticies = new Vertex[chunk.VertexCount];
                tagData.RenderModels[c] = model;

                if(chunk.Resources.Length < 9)
                {
                    // Skip if we don't have the right data setup
                    continue;
                }

                var matResource = chunk.Resources[1];
                var matData = matResource.Data.Span;

                // Process face data
                var faceResource = chunk.Resources[2];
                var faceData = faceResource.Data.Span;

                var tempGroups = new ConcurrentDictionary<int, List<Triangle>>();
                
                // Entry size is 8
                for (var i = 0; i < matResource.Size / 8; i++)
                {
                    var start = i * 8;
                    var triangleStart = matData.ReadUInt16At(start) / 3;
                    var triangleLength = matData.ReadUInt16At(start + 2) / 3;
                    var shaderId = matData.ReadInt16At(start + 6);

                    // PERF: growing lists
                    var faceGroup = tempGroups.GetOrAdd(shaderId, (s) => new List<Triangle>());

                    for(var s = 0; s < triangleLength; s++)
                    {
                        var indicesStart = (triangleStart * 6) + (s * 6);

                        var vert0 = faceData.ReadUInt16At(indicesStart);
                        var vert1 = faceData.ReadUInt16At(indicesStart + 2);
                        var vert2 = faceData.ReadUInt16At(indicesStart + 4);

                        faceGroup.Add(new Triangle { Indicies = (vert0, vert1, vert2), MaterialId = shaderId });
                    }
                }

                model.FaceGroups = tempGroups.Values.Select(l => l.ToArray()).ToArray();

                var posResouce = chunk.Resources[6];
                var posData = posResouce.Data.Span;

                for(var i = 0; i < chunk.VertexCount; i++)
                {
                    var vert = new Vertex();

                    vert.Position = posData.ReadVec3At(i * 12);

                    model.Verticies[i] = vert;
                }

                var texResouce = chunk.Resources[7];
                var texData = texResouce.Data.Span;

                for (var i = 0; i < chunk.VertexCount; i++)
                {
                    var vert = model.Verticies[i];

                    vert.Texture = texData.ReadVec2At(i * 8);

                    model.Verticies[i] = vert;
                }

                var tbnResouce = chunk.Resources[8];
                var tbnData = tbnResouce.Data.Span;

                for (var i = 0; i < chunk.VertexCount; i++)
                {
                    var vert = model.Verticies[i];

                    var start = i * 36;
                    vert.Tangent = tbnData.ReadVec3At(start);
                    vert.Bitangent = tbnData.ReadVec3At(start + 12);
                    vert.Normal = tbnData.ReadVec3At(start + 24);

                    model.Verticies[i] = vert;
                }

                
            }

            return tagData;
        }

        public BspTagData ProcessCollisionGeometry(Bsp bsp, BspTagData tagData)
        {
            var block = bsp.CollisionInfos.First();

            var faces = new List<int[]>();

            for (var i = 0; i < block.Faces.Length; i++)
            {
                var face = block.Faces[i];

                var faceVerts = new List<int>(8);

                var currentEdge = block.HalfEdges[face.FirstEdge];

                while (true)
                {
                    int fromVert;
                    int toVert;
                    int nextEdge;

                    if (currentEdge.Face0 == i)
                    {
                        fromVert = currentEdge.Vertex0;
                        toVert = currentEdge.Vertex1;
                        nextEdge = currentEdge.NextEdge;
                    }
                    else
                    {
                        fromVert = currentEdge.Vertex1;
                        toVert = currentEdge.Vertex0;
                        nextEdge = currentEdge.PrevEdge;
                    }

                    if (faceVerts.Count == 0)
                    {
                        faceVerts.Add(fromVert);
                    }

                    if (faceVerts[0] == toVert)
                        break;

                    faceVerts.Add(toVert);

                    currentEdge = block.HalfEdges[nextEdge];
                }

                faces.Add(faceVerts.ToArray());
            }

            //tagData.Faces = faces.ToArray();

            //tagData.Verticies = block.Verticies
            //    .Select(r => new Vector3(r.x, r.y, r.z))
            //    .Select(v => new Vertex() { Position = v })
            //    .ToArray();

            return tagData;
        }
    }
}
