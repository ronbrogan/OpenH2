namespace OpenH2.Foundation
{
    public class Mesh<TTexture>
    {
        public int[] Indicies { get; set; }
        public VertexFormat[] Verticies { get; set; }
        public MeshElementType ElementType { get; set; }
        public IMaterial<TTexture> Material { get; set; }
        public bool Compressed { get; set; }

        public byte[] RawData { get; set; }

        public string Note { get; set; }

        /// <summary>
        /// Set to true after modification of internal data to ensure caches are updated
        /// </summary>
        public bool Dirty { get; set; }
    }
}
