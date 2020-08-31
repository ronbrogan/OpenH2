using OpenH2.Core.Representations;
using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;
using System.Numerics;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.sky)]
    public class SkyboxTag : BaseTag
    {
        public override string Name { get; set; }

        public SkyboxTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(4)]
        public TagRef<RenderModelTag> Model { get; set; }

        [ReferenceArray(28)]
        public Obj28[] Obj28s { get; set; }

        [ReferenceArray(120)]
        public Obj120[] Obj120s { get; set; }


        [FixedLength(12)]
        public class Obj28
        {
            [PrimitiveValue(4)]
            public TagRef<BitmapTag> Bitmap { get; set; }

            [PrimitiveValue(8)]
            public float Param { get; set; }
        }

        [FixedLength(52)]
        public class Obj120
        {
            [PrimitiveValue(0)]
            public Vector3 Something1 { get; set; }

            [PrimitiveValue(8)]
            public Vector2 Position { get; set; }

            [PrimitiveValue(24)]
            public TagRef<LensFlareTag> LensFlare { get; set; }

            [ReferenceArray(44)]
            public Obj120_44[] obj120_44s { get; set; }
        }

        [FixedLength(40)]
        public class Obj120_44
        {
            [PrimitiveValue(0)]
            public uint Something { get; set; }

            [PrimitiveValue(4)]
            public Vector3 Something2 { get; set; }

            [PrimitiveValue(16)]
            public Vector2 Something3 { get; set; }
        }
    }
}
