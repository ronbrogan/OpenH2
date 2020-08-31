using OpenH2.Core.Representations;
using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;
using System.Numerics;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.weap)]
    public class WeaponTag : BaseTag
    {
        public override string Name { get; set; }
        public WeaponTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(56)]
        public TagRef<HaloModelTag> Hlmt { get; set; }

        [PrimitiveArray(108, 9)]
        public float[] RotationMatrix { get; set; }

        [ReferenceArray(260)]
        public BitmapWrapper[] Bitmaps { get; set; }

        [ReferenceArray(680)]
        public FirstPersonAnimation[] FirstPersonAnimations { get; set; }

        [FixedLength(8)]
        public class BitmapWrapper
        {
            [PrimitiveValue(4)]
            public TagRef<BitmapTag> Bitmap { get; set; }
        }

        [FixedLength(16)]
        public class FirstPersonAnimation
        {
            [PrimitiveValue(4)]
            public TagRef<RenderModelTag> Model { get; set; }

            [PrimitiveValue(12)]
            public TagRef<AnimationGraphTag> Animation { get; set; }
        }
    }
}
