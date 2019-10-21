using OpenH2.Core.Representations;
using OpenH2.Core.Tags.Layout;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.mach)]
    public class MachineryTag : BaseTag
    {
        public override string Name { get; set; }
        public MachineryTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(0)]
        public int Type { get; set; }

        [PrimitiveValue(4)]
        public float UniformScale { get; set; }

        [PrimitiveValue(16)]
        public float Unknown { get; set; }

        [PrimitiveValue(56)]
        public TagRef<PhysicalModelTag> PhysicalModel { get; set; }

        [PrimitiveValue(64)]
        public TagRef<BlocTag> Bloc { get; set; }

        [PrimitiveValue(80)]
        public uint EffectId { get; set; }

        [PrimitiveValue(88)]
        public uint FootId { get; set; }
    }
}
