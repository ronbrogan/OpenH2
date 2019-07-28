namespace OpenH2.Foundation
{
    public class Mesh
    {
        public int[] Indicies { get; set; }
        public VertexFormat[] Verticies { get; set; }
        public MeshElementType ElementType { get; set; }

        // TODO: Material
        public int MaterialIdentifier { get; set; }
    }
}
