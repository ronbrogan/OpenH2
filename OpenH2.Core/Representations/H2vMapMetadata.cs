using System.Collections.Generic;

namespace OpenH2.Core.Representations
{
    public abstract class H2vBaseMap
    {
        public string Name => this.Header.Name;

        public int PrimaryMagic { get; internal set; }

        public int SecondaryMagic { get; internal set; }

        public H2vMapHeader Header { get; internal set; }

        public IndexHeader IndexHeader { get; internal set; }

        public Dictionary<uint, TagIndexEntry> TagIndex { get; internal set; }

        public Dictionary<uint, string> TagNames { get; set; }
    }
}
