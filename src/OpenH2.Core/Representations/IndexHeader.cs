using OpenH2.Core.Offsets;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Scenario;

namespace OpenH2.Core.Representations
{
    public class IndexHeader
    {
        public NormalOffset FileRawOffset { get; set; }
        public int PrimaryMagicConstant { get; set; }
        public int TagListCount { get; set; }
        public PrimaryOffset TagIndexOffset { get; set; }
        public TagRef<ScenarioTag> Scenario { get; set; }
        public TagRef<GlobalsTag> Globals { get; set; }
        public int Unknown1 { get; set; }
        public int TagIndexCount { get; set; }
        public string TagsLabel { get; set; }

        public static int Length = 32;
    }
}