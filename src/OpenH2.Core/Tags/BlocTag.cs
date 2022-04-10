using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;
using System.Numerics;
using OpenH2.Core.Tags.Common;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.bloc)]
    public class BlocTag : BaseTag
    {
        public override string Name { get; set; }
        public BlocTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(0)]
        public GameObjectFlags Flags { get; set; }

        [PrimitiveValue(4)]
        public float BoundingRadius { get; set; }

        [PrimitiveValue(8)]
        public Vector3 BoudingOffset { get; set; }

        [PrimitiveValue(56)]
        public TagRef<HaloModelTag> PhysicalModel { get; set; }

        [PrimitiveValue(64)]
        public uint BlocId { get; set; }

        [PrimitiveValue(80)]
        public uint EffectId { get; set; }

        [PrimitiveValue(88)]
        public uint FootId { get; set; }
    }
}
