using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;
using OpenH2.Core.Tags.Common;
using System.Numerics;

namespace OpenH2.Core.Tags
{

    [TagLabel(TagName.bipd)]
    public class BipedTag : BaseTag
    {
        public override string Name { get; set; }
        public BipedTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(2)]
        public GameObjectFlags Flags { get; set; }

        [PrimitiveValue(4)]
        public float BoundingRadius { get; set; }

        [PrimitiveValue(8)]
        public Vector3 BoundingOffset { get; set; }

        [PrimitiveValue(24)]
        public float AccelerationScale { get; set; }

        //[InternedString(52)]
        public string DefaultModelVariant { get; set; }

        [PrimitiveValue(56)]
        public TagRef<HaloModelTag> Model { get; set; }


        [ReferenceArray(92)]
        public AiProperty[] AiProperties { get; set; }

        [ReferenceArray(148)]
        public Obj148[] obj148s { get; set; }

        [ReferenceArray(180)]
        public Obj180[] obj180s { get; set; }

        [ReferenceArray(232)]
        public Obj232[] obj232s { get; set; }


        [FixedLength(16)]
        public class AiProperty
        {
            [PrimitiveValue(12)]
            public uint Value1 { get; set; }
        }

        [FixedLength(24)]
        public class Obj148
        {
            [PrimitiveValue(4)]
            public TagRef SomeTagMaybe { get; set; }
        }


        [FixedLength(8)]
        public class Obj180
        {
            [PrimitiveValue(0)]
            public ushort Value1 { get; set; }

            [PrimitiveValue(2)]
            public ushort Value2 { get; set; }

            [PrimitiveValue(4)]
            public TagRef SomeTagMaybe { get; set; }
        }

        [FixedLength(12)]
        public class Obj232
        {
            [PrimitiveValue(0)]
            public ushort Kart { get; set; }

            [PrimitiveValue(4)]
            public ushort Two { get; set; }

            [PrimitiveValue(8)]
            public uint CDCD { get; set; }
        }
    }
}
