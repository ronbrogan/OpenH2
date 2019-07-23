using OpenH2.Core.Tags;
using OpenH2.Core.Types;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OpenH2.Translation.TagData
{
    public class BspTagData : BaseTagData
    {
        public BspTagData(Bsp tag) : base(tag)
        {
        }

        public RenderModel[] RenderModels { get;set;}

        public class RenderModel
        {
            public Triangle[][] FaceGroups { get; set; }

            public Vertex[] Verticies { get; set; }
        }

        public class PartInfo
        {
            public uint ModelId { get; set; }

            public int Value1 { get; set; }

            public int Value2 { get; set; }

            public int Value3 { get; set; }

            // length 14
            public float[] Data { get; set; }
        }
    }
}
