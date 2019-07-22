using OpenH2.Core.Tags;
using OpenH2.Core.Types;

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
            public (int,int,int)[] Faces { get; set; }

            public Vertex[] Verticies { get; set; }
        }

    }
}
