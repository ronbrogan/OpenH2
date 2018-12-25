using System;
using OpenH2.Core.Tags;
using OpenH2.Core.Types;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenH2.Translation.TagData.Processors
{
    public class BspTagDataProcessor
    {
        public static BspTagData ProcessTag(BaseTag tag)
        {
            var bsp = tag as BspTag;

            if (bsp == null)
                throw new ArgumentException("Tag must be a BspTag", nameof(tag));

            var tagData = new BspTagData(bsp);

            var block = bsp.RawBlocks.First();

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

            tagData.Faces = faces.ToArray();

            tagData.Verticies = block.Verticies
                .Select(r => new Vector3(r.x, r.y, r.z))
                .Select(v => new Vertex(){Position = v})
                .ToArray();

            return tagData;
        }
    }
}
