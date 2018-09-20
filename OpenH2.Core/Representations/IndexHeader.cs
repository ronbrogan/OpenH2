using OpenH2.Core.Offsets;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Representations
{
    public class IndexHeader
    {
        public NormalOffset FileRawOffset { get; set; }
        public int PrimaryMagicConstant { get; set; }
        public int TagListCount { get; set; }
        public PrimaryOffset ObjectIndexOffset { get; set; }
        public int ScenarioID { get; set; }
        public int TagIDStart { get; set; }
        public int Unknown1 { get; set; }
        public int ObjectCount { get; set; }
        public string TagsLabel { get; set; }

        public static int Length = 32;
    }
}
