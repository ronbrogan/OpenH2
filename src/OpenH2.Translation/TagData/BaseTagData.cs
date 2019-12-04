using System.Collections.Generic;
using OpenH2.Core.Tags;

namespace OpenH2.Translation.TagData
{
    public abstract class BaseTagData
    {
        public uint Id { get; private set; }

        public BaseTagData(BaseTag tag)
        {
            this.Id = tag.Id;
        }
    }
}