using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;
using System.Numerics;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.eqip)]
    public class EquipmentTag : BaseTag
    {
        public override string Name { get; set; }
        public EquipmentTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(56)]
        public TagRef<HaloModelTag> Hlmt { get; set; }


        //[InternalReferenceValue(260)]
        //public BitmapWrapper[] Bitmaps { get; set; }

        //[InternalReferenceValue(680)]
        //public FirstPersonAnimation[] FirstPersonAnimations { get; set; }

        //[FixedLength(8)]
        //public class BitmapWrapper
        //{
        //    [PrimitiveValue(4)]
        //    public TagRef<BitmapTag> Bitmap { get; set; }
        //}

        //[FixedLength(16)]
        //public class FirstPersonAnimation
        //{
        //    [PrimitiveValue(4)]
        //    public TagRef<ModelTag> Model { get; set; }

        //    [PrimitiveValue(12)]
        //    public TagRef<AnimationGraphTag> Animation { get; set; }
        //}
    }
}
