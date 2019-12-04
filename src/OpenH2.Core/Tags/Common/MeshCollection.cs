using Newtonsoft.Json;
using OpenH2.Foundation;

namespace OpenH2.Core.Tags.Common
{
    public class MeshCollection
    {
        public MeshCollection(ModelMesh[] meshes)
        {
            this.Meshes = meshes;
        }

        [JsonIgnore]
        public ModelMesh[] Meshes { get; set; }
    }
}
