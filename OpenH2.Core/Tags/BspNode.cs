using System.Collections.Generic;
using OpenH2.Core.Meta;
using OpenH2.Core.Types;

namespace OpenH2.Core.Tags
{
    public class BspNode : TagNode
    {
        public BspMeta Meta { get; set; }

        public int[][] Faces { get; set; }

        public Vertex[] Verticies { get; set; }
    }
}
