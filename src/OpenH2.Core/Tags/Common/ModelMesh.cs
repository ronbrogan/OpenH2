using OpenH2.Core.Representations;
using OpenH2.Foundation;

namespace OpenH2.Core.Tags.Common
{
    public class ModelMesh
    {
        public int[] Indicies { get; set; }
        public VertexFormat[] Verticies { get; set; }
        public MeshElementType ElementType { get; set; }
        public TagRef<ShaderTag> Shader { get; set; }
        public bool Compressed { get; set; }

        public byte[] RawData { get; set; }

        public string Note { get; set; }
    }
}
