using OpenH2.Core.Enums;
using OpenH2.Core.Parsing;
using OpenH2.Core.Maps;
using OpenH2.Serialization.Layout;

namespace OpenH2.Core.Tags
{
    [ArbitraryLength]
    public abstract class BaseTag
    {
        public virtual string Name { get; set; }

        public uint Id { get; private set; }

        public uint Offset { get; set; }
        
        public uint Length { get; set; }

        public TagIndexEntry TagIndexEntry { get; set; }

        public DataFile DataFile { get; set; }

        public int InternalSecondaryMagic { get; set; }

        public BaseTag(uint id)
        {
            this.Id = id;
        }

        public virtual void PopulateExternalData(H2MapReader reader) { }
    }
}
