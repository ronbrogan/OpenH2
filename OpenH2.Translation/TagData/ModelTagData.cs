using OpenH2.Core.Tags;
using OpenH2.Core.Types;
using System;

namespace OpenH2.Translation.TagData
{
    public class ModelTagData : BaseTagData
    {
        public ModelTagData(ModelTag tag) : base(tag)
        {
            Name = tag.Name;
        }

        public string Name { get; set; }

        public Mesh[] Parts { get; set; }
    }
}
