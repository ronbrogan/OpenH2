using OpenH2.Core.Representations;
using OpenH2.Core.Tags.Layout;
using System.Numerics;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.lens)]
    public class LensFlareTag : BaseTag
    {
        public override string Name { get; set; }
        public LensFlareTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(36)]
        public TagRef<BitmapTag> Bitmap { get; set; }

        [InternalReferenceValue(64)]
        public Obj64[] obj64s { get; set; }

        [FixedLength(48)]
        public class Obj64
        {
            [PrimitiveValue(0)]
            public uint Value1 { get; set; }

            [PrimitiveValue(4)]
            public uint Value2 { get; set; }

            [PrimitiveValue(8)]
            public Vector3 VecA { get; set; }

            [PrimitiveValue(20)]
            public Vector3 VecB { get; set; }

            [PrimitiveValue(32)]
            public Vector3 VecC { get; set; }

            [PrimitiveValue(44)]
            public uint Value3 { get; set; }
        }
    }
}
