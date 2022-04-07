using OpenBlam.Core.MapLoading;
using OpenBlam.Serialization.Layout;
using OpenH2.Core.Enums;
using OpenH2.Core.Maps;

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

        public virtual void PopulateExternalData(MapStream reader) { }
    }
}
