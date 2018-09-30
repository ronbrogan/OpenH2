using System.Collections.Generic;

namespace OpenH2.Core.Tags
{
    public abstract class TagNode
    {
        public List<TagNode> Children { get; set; }

        public TagNode()
        {
        }
    }
}