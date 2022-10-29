using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.crea)]
    public class CreatureTag : BaseTag
    {
        public CreatureTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(56)]
        public TagRef<HaloModelTag> Model { get; set; }
    }
}
