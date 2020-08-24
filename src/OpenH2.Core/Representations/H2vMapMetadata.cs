using System.Collections.Generic;

namespace OpenH2.Core.Representations
{
    public abstract class H2vBaseMap
    {
        public string Name => this.Header.Name;

        public int PrimaryMagic { get; set; }

        public int SecondaryMagic { get; set; }

        public H2vMapHeader Header { get; set; }

        public IndexHeader IndexHeader { get; set; }

        public Dictionary<uint, TagIndexEntry> TagIndex { get; set; }

        public Dictionary<int, string> InternedStrings { get; set; }
        public Dictionary<uint, string> TagNames { get; set; }
    }
}
