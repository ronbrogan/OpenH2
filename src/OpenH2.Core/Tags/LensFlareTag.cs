using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;
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

        [PrimitiveValue(0)]
        public float FalloffAngle { get; set; }

        [PrimitiveValue(4)]
        public float CutoffAngle { get; set; }

        [PrimitiveValue(8)]
        public float OcclusionRadius { get; set; }

        [PrimitiveValue(36)]
        public TagRef<BitmapTag> Bitmap { get; set; }

        [ReferenceArray(64)]
        public ReflectionInfo[] Reflections { get; set; }

        [FixedLength(48)]
        public class ReflectionInfo
        {
            [PrimitiveValue(0)]
            public uint Flags { get; set; }

            [PrimitiveValue(4)]
            public uint BitmapIndex { get; set; }

            [PrimitiveValue(8)]
            public float PositionAlongFlareAxis { get; set; }

            [PrimitiveValue(12)]
            public float RotationOffset { get; set; }

            [PrimitiveValue(16)]
            public Vector2 Radius { get; set; }

            [PrimitiveValue(24)]
            public Vector2 Brightness { get; set; }

            [PrimitiveValue(28)]
            public float ModulationFactor { get; set; }

            [PrimitiveValue(32)]
            public Vector3 TintColor { get; set; }
        }
    }
}
