using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Common.Collision;
using OpenH2.Foundation;

namespace OpenH2.Rendering
{
    public struct DrawCommand
    {
        public DrawCommand(Mesh<BitmapTag> mesh)
        {
            this.ElementType = mesh.ElementType;
            this.VaoHandle = -1;
            this.IndiciesCount = mesh.Indicies.Length;
            this.ShaderUniformHandle = -1;
            this.Mesh = mesh;

            this.VertexBase = 0;
            this.IndexBase = 0;
        }

        public MeshElementType ElementType;
        public int VaoHandle;
        public int IndiciesCount;
        public int VertexBase;
        public int IndexBase;
        public int ShaderUniformHandle;
        public Mesh<BitmapTag> Mesh;
    }
}
