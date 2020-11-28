using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;
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

        [PrimitiveValue(300)]
        public WeaponFlags Flags { get; set; }

        [PrimitiveValue(520)]
        // When set to zero, sword reticle never changed to red
        public float ReticleRange { get; set; }

        [PrimitiveValue(528)]
        public float AutoAimAmount { get; set; }

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

        public enum WeaponFlags : uint
        {
            KillConsumesDurability = 1 << 26
        }
    }
}
