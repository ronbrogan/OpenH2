using OpenH2.Core.Offsets;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Serialization.Layout;

namespace OpenH2.Core.Maps
{
    [FixedLength(32)]
    public class IndexHeader
    {
        public NormalOffset FileRawOffset { get; set; }

        [PrimitiveValue(0)]
        public int PrimaryMagicConstant { get; set; }

        [PrimitiveValue(4)]
        public int TagListCount { get; set; }

        [PrimitiveValue(8)]
        public int RawTagIndexOffset { get; set; }

        public PrimaryOffset TagIndexOffset { get; set; }

        [PrimitiveValue(12)]
        public TagRef<ScenarioTag> Scenario { get; set; }

        [PrimitiveValue(16)]
        public TagRef<GlobalsTag> Globals { get; set; }

        [PrimitiveValue(24)]
        public int TagIndexCount { get; set; }

        [StringValue(28, 4)]
        public string TagsLabel { get; set; }

        public static int Length => 32;
    }
}