using System;
using OpenH2.Core.Tags;

namespace OpenH2.Core.Representations
{

    /// This class is the in-memory representation of a .map file
    public class Scene
    {
        public TagNode TreeRoot { get; set; }

        internal SceneMetadata Metadata { get; set; }

        

        public bool HasValidSignature => Metadata.CalculatedSignature == Metadata.StoredSignature;

        public string Name => this.Metadata.Name;

        internal Scene()
        {

        }
    }
}
