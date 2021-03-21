using OpenBlam.Core.MapLoading;
using OpenBlam.Core.Maps;
using OpenBlam.Serialization.Layout;

namespace OpenH2.Core.Maps.Vista
{
    [ArbitraryLength]
    public class H2vMapInfo : IH2MapInfo
    {
        public string Name => Header.Name;

        public int PrimaryMagic { get; set; }
        public int SecondaryMagic { get; set; }

        [InPlaceObject(0)]
        public H2vMapHeader Header { get; set; }
        IH2MapHeader IH2MapInfo.Header => this.Header;

        public void Load(byte selfIdentifier, MapStream mapStream)
        { 
        }

        public void UseAncillaryMap(byte identifier, IMap ancillaryMap)
        {
        }
    }
}
