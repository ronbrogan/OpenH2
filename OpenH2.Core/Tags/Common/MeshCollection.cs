using Newtonsoft.Json;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Tags.Common
{
    public class MeshCollection
    {
        public MeshCollection(Mesh[] meshes)
        {
            this.Meshes = meshes;
        }

        [JsonIgnore]
        public Mesh[] Meshes { get; set; }
    }
}
