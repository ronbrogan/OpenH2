using OpenH2.Core.Representations;
using OpenH2.Core.Tags.Layout;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.hlmt)]
    public class PhysicalModelTag : BaseTag
    {
        public override string Name { get; set; }

        public PhysicalModelTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(4)]
        public TagRef<ModelTag> Model { get; set; }

        [PrimitiveValue(12)]
        public TagRef<ColliderTag> ColliderId { get; set; }

        [PrimitiveValue(20)]
        public TagRef<AnimationGraphTag> AnimationGraph { get; set; }

        [PrimitiveValue(28)]
        public uint PhysicsInfoId { get; set; }

        [PrimitiveValue(36)]
        public uint PhmoId { get; set; }

        [PrimitiveArray(40, 8)]
        public float[] Params { get; set; }
    }
}
