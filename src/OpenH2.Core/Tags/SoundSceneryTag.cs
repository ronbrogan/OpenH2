using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;
using System.Numerics;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.ssce)]
    public class SoundSceneryTag : BaseTag
    {
        public override string Name { get; set; }
        public SoundSceneryTag(uint id) : base(id)
        {
        }

        [PrimitiveArray(108, 9)]
        public float[] Params { get; set; }

        [ReferenceArray(148)]
        public SoundReference[] SoundReferences { get; set; }

        [FixedLength(48)]
        public class SoundReference
        {
            [PrimitiveValue(4)]
            public TagRef<LoopingSoundTag> Sound { get; set; }
        }
    }
}
