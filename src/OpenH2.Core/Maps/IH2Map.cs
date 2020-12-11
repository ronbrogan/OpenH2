using OpenBlam.Core.MapLoading;
using OpenBlam.Core.Maps;
using OpenBlam.Serialization.Materialization;
using OpenH2.Core.Enums;
using OpenH2.Core.Tags;
using System.Collections.Generic;
using System.IO;

namespace OpenH2.Core.Maps
{
    public interface IH2Map : IMap, IInternedStringProvider
    {
        IndexHeader IndexHeader { get; set; }
        string Name { get; }
        int PrimaryMagic { get; set; }
        int SecondaryMagic { get; set; }
        IH2MapHeader Header { get; }
        Dictionary<uint, TagIndexEntry> TagIndex { get; set; }
        DataFile OriginFile { get; }
        void LoadWellKnownTags();

        bool TryGetTag<T>(uint id, out T? tag) where T : BaseTag;
        bool TryGetTag<T>(TagRef<T> tagref, out T? tag) where T : BaseTag;
        T GetTag<T>(TagRef<T> tagref) where T : BaseTag;
        bool TryFindTagId(TagName tag, string fullName, out uint id);
    }

    public class NullH2Map : IH2Map
    {
        public static NullH2Map Instance { get; } = new NullH2Map();

        public IndexHeader IndexHeader { get; set; }

        public string Name => "NullMap";

        public int PrimaryMagic { get; set; }
        public int SecondaryMagic { get; set; }

        public IH2MapHeader Header => null;

        public int IndexOffset => 0;
        public int DataOffset => 0;

        public Dictionary<uint, TagIndexEntry> TagIndex { get; set; } = new();

        public DataFile OriginFile => DataFile.Local;

        public void Load(MapStream mapStream)
        {
        }

        public bool TryGetTag<T>(uint id, out T? tag) where T : BaseTag
        {
            tag = default;
            return false;
        }

        public T GetTag<T>(TagRef<T> tagref) where T : BaseTag
        {
            return default;
        }

        public bool TryGetTag<T>(TagRef<T> tagref, out T? tag) where T : BaseTag
        {
            tag = default;
            return false;
        }

        public bool TryFindTagId(TagName tag, string fullName, out uint id)
        {
            id = default;
            return false;
        }

        public void Load(byte selfIdentifier, MapStream mapStream)
        {
        }

        public void UseAncillaryMap(byte identifier, IMap ancillaryMap)
        {
        }

        public void LoadWellKnownTags()
        {
        }
    }
}