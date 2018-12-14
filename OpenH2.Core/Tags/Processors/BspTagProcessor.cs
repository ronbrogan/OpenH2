using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using OpenH2.Core.Meta;
using OpenH2.Core.Parsing;
using OpenH2.Core.Types;

namespace OpenH2.Core.Tags.Processors
{
    public class BspTagProcessor
    {
        public static BspNode ProcessBspMeta(BaseMeta meta, TrackingReader reader)
        {
            if (meta is BspMeta == false)
                throw new ArgumentException("Meta must be of type BspMeta");

            var bsp = new BspNode();
            bsp.Meta = (BspMeta)meta;

            var block = bsp.Meta.RawBlocks.First();

            var faces = new List<int[]>();
            
            for(var i = 0; i < block.Faces.Count; i++)
            {
                var face = block.Faces[i];

                var faceVerts = new List<int>(8);

                var currentEdge = block.HalfEdges[face.FirstEdge];

                while (true)
                {
                    int fromVert;
                    int toVert;
                    int nextEdge;

                    if(currentEdge.Face0 == i)
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

            bsp.Faces = faces.ToArray();

            bsp.Verticies = block.Verticies
                .Select(r => new Vector3(r.x, r.y, r.z))
                .Select(v => new Vertex(){Position = v})
                .ToArray();

            return bsp;
        }
    }
}
