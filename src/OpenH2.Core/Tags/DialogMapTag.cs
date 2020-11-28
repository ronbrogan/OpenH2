using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.mdlg)]
    public class DialogMapTag : BaseTag
    {
        public DialogMapTag(uint id) : base(id)
        {
        }

        [ReferenceArray(0)]
        public DialogLineInfo[] DiaglogLines { get; set; }

        [FixedLength(16)]
        public class DialogLineInfo
        {
            [InternedString(0)]
            public string Name { get; set; }

            [ReferenceArray(4)]
            public SoundTagInfo[] SoundTags { get; set; }

            [InternedString(12)]
            public string EffectName { get; set; }

            [FixedLength(16)]
            public class SoundTagInfo
            {
                [InternedString(0)]
                public string Name { get; set; }

                [PrimitiveValue(8)]
                public TagRef<SoundTag> Sound { get; set; }
            }
        }
    }
}
