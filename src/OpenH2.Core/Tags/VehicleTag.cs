using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.vehi)]
    public class VehicleTag : BaseTag
    {
        public VehicleTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(56)]
        public TagRef<HaloModelTag> Hlmt { get; set; }

        [ReferenceArray(92)]
        public Obj92[] Obj92s { get; set; }

        [ReferenceArray(100)]
        public Obj100[] Obj100s { get; set; }

        [ReferenceArray(148)]
        public Obj148[] Obj148s { get; set; }

        [ReferenceArray(180)]
        public Obj180[] Obj180s { get; set; }

        [ReferenceArray(232)]
        public Obj232[] Obj232s { get; set; }

        [ReferenceArray(416)]
        public Obj416[] Obj416s { get; set; }

        [ReferenceArray(440)]
        public Obj440[] Obj440s { get; set; }

        [ReferenceArray(448)]
        public Obj448[] Obj448s { get; set; }

        [ReferenceArray(456)]
        public Obj456[] Obj456s { get; set; }


        [FixedLength(16)]
        public class Obj92
        {
        }

        [FixedLength(52)]
        public class Obj100
        {
        }

        [FixedLength(24)]
        public class Obj148
        {
        }

        [FixedLength(8)]
        public class Obj180
        {
        }

        [FixedLength(8)]
        public class Obj232
        {
        }

        [FixedLength(8)]
        public class Obj416
        {
        }

        [FixedLength(8)]
        public class Obj440
        {
        }

        [FixedLength(8)]
        public class Obj448
        {
        }

        [FixedLength(8)]
        public class Obj456
        {
        }
    }
}
