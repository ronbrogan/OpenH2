using OpenH2.Core.Tags;
using OpenBlam.Serialization.Materialization;
using System.Collections.Generic;

namespace OpenH2.Core.Maps
{
    public abstract class H2BaseMap : IInternedStringProvider
    {
        public string Name => this.Header.Name;

        public int PrimaryMagic { get; set; }

        public int SecondaryMagic { get; set; }

        public IH2MapHeader Header { get; set; }

        public IndexHeader IndexHeader { get; set; }

        public Dictionary<uint, TagIndexEntry> TagIndex { get; set; }

        public Dictionary<int, string> InternedStrings { get; set; }
        public Dictionary<uint, string> TagNames { get; set; }
        public Dictionary<(TagName, string), uint> TagNameLookup { get; set; }

        int IInternedStringProvider.IndexOffset => Header.InternedStringIndexOffset;

        int IInternedStringProvider.DataOffset => Header.InternedStringsOffset;
    }
}
