using OpenH2.Core.Offsets;

namespace OpenH2.Core.Representations
{
    public class IndexHeader
    {
        public NormalOffset FileRawOffset { get; set; }
        public int PrimaryMagicConstant { get; set; }
        public int TagListCount { get; set; }
        public PrimaryOffset TagIndexOffset { get; set; }
        public int ScenarioID { get; set; }
        public int TagIDStart { get; set; }
        public int Unknown1 { get; set; }
        public int TagIndexCount { get; set; }
        public string TagsLabel { get; set; }

        public static int Length = 32;
    }
}