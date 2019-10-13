using OpenH2.Core.Representations;
using OpenH2.Core.Tags.Layout;

namespace OpenH2.Core.Tags
{
    [TagLabel("sky ")]
    public class SkyboxTag : BaseTag
    {
        public override string Name { get; set; }

        public SkyboxTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(4)]
        public TagRef<ModelTag> Model { get; set; }
    }
}
