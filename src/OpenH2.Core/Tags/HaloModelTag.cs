using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.hlmt)]
    public class HaloModelTag : BaseTag
    {
        public override string Name { get; set; }

        public HaloModelTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(4)]
        public TagRef<RenderModelTag> RenderModel { get; set; }

        [PrimitiveValue(12)]
        public TagRef<ColliderTag> ColliderId { get; set; }

        [PrimitiveValue(20)]
        public TagRef<AnimationGraphTag> AnimationGraph { get; set; }

        [PrimitiveValue(28)]
        public uint PhysicsInfoId { get; set; }

        [PrimitiveValue(36)]
        public TagRef<PhysicsModelTag> PhysicsModel { get; set; }

        [PrimitiveArray(40, 8)]
        public float[] Params { get; set; }


        [ReferenceArray(80)]
        public VariantDescriptor[] Variants { get; set; }

        [FixedLength(56)]
        public class VariantDescriptor
        {
            [PrimitiveValue(0)]
            public uint Id { get; set; }


            [ReferenceArray(20)]
            public VariantEffects[] Effects { get; set; }

            [ReferenceArray(28)]
            public VariantChildren[] Children { get; set; }


            [FixedLength(120)]
            public class VariantEffects
            {

            }

            [FixedLength(16)]
            public class VariantChildren
            {
                [PrimitiveValue(0)]
                public uint Id { get; set; }

                [PrimitiveValue(8)]
                public TagRef Child { get; set; }
            }
        }
    }
}
