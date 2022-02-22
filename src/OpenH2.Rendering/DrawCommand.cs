using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Common.Collision;
using OpenH2.Foundation;
using OpenH2.Rendering.Shaders;
using System.Numerics;

namespace OpenH2.Rendering
{
    public unsafe struct DrawCommand
    {
        public DrawCommand(Mesh<BitmapTag> mesh)
        {
            this.ElementType = mesh.ElementType;
            this.VaoHandle = -1;
            this.IndiciesCount = mesh.Indicies.Length;

            for (var i = 0; i < (int)Shader.MAX_VALUE; i++)
                this.ShaderUniformHandle[i] = -1;

            this.Mesh = mesh;

            this.VertexBase = 0;
            this.IndexBase = 0;
            this.ColorChangeData = Vector4.Zero;
        }

        public MeshElementType ElementType;
        public int VaoHandle;
        public int IndiciesCount;
        public int VertexBase;
        public int IndexBase;
        public fixed int ShaderUniformHandle[(int)Shader.MAX_VALUE];
        public Vector4 ColorChangeData;
        public Mesh<BitmapTag> Mesh;
    }
}
