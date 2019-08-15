namespace OpenH2.Foundation
{
    public class Mesh
    {
        public int[] Indicies { get; set; }
        public VertexFormat[] Verticies { get; set; }
        public MeshElementType ElementType { get; set; }
        public uint MaterialIdentifier { get; set; }
        public bool Compressed { get; set; }

        public byte[] RawData { get; set; }

        public string Note { get; set; }
    }
}
