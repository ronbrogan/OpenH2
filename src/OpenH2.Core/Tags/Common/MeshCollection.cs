namespace OpenH2.Core.Tags.Common
{
    public class MeshCollection
    {
        public MeshCollection(ModelMesh[] meshes)
        {
            this.Meshes = meshes;
        }

        public ModelMesh[] Meshes { get; set; }
    }
}
