using OpenH2.Core.Tags.Layout;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.@char)]
    public class CharacterTag : BaseTag
    {
        public CharacterTag(uint id) : base(id)
        {
        }
    }
}
