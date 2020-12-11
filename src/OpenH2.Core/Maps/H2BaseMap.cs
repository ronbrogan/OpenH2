using OpenH2.Core.Tags;
using OpenBlam.Serialization.Materialization;
using System.Collections.Generic;
using OpenBlam.Core.Maps;
using OpenH2.Core.Factories;
using OpenBlam.Core.MapLoading;
using System.Linq;
using OpenH2.Core.Enums;
using System;
using OpenH2.Core.Offsets;
using System.Diagnostics;
using System.Reflection;
using OpenH2.Core.Tags.Layout;
using System.IO;

namespace OpenH2.Core.Maps
{
    public static class H2BaseMap
    {
        public static int CalculateSignature(Stream mapStream)
        {
            var sig = 0;

            for (var i = 2048; i < mapStream.Length; i += 4)
            {
                sig ^= mapStream.ReadInt32At(i);
            }

            return sig;
        }
    }

    public abstract class H2BaseMap<THeader> : HaloMap<THeader>, IH2Map where THeader : IH2MapHeader
    {
        public string Name => this.Header.Name;
        private IH2Map mainMenu = NullH2Map.Instance;
        private IH2Map spShared = NullH2Map.Instance;
        private IH2Map mpShared = NullH2Map.Instance;

        public DataFile OriginFile { get; private set; }
        public int PrimaryMagic { get; set; }
        public int SecondaryMagic { get; set; }
        public IndexHeader IndexHeader { get; set; }
        public Dictionary<uint, TagIndexEntry> TagIndex { get; set; }

        int IInternedStringProvider.IndexOffset => Header.InternedStringIndexOffset;
        int IInternedStringProvider.DataOffset => Header.InternedStringsOffset;
        IH2MapHeader IH2Map.Header => this.Header;
        

        public override void UseAncillaryMap(byte identifier, IMap ancillaryMap)
        {
            if (ancillaryMap is not IH2Map h2ancillaryMap)
            {
                return;
            }

            switch ((DataFile)identifier)
            {
                case DataFile.MainMenu: this.mainMenu = h2ancillaryMap; break;
                case DataFile.Shared: this.mpShared = h2ancillaryMap; break;
                case DataFile.SinglePlayerShared: this.spShared = h2ancillaryMap; break;
                default: throw new NotSupportedException();
            };

            base.UseAncillaryMap(identifier, ancillaryMap);
        }

        public override void Load(byte selfIdentifier, MapStream mapStream)
        {
            OriginFile = (DataFile)selfIdentifier;
            base.Load(selfIdentifier, mapStream);
        }

        public IEnumerable<T> GetLocalTagsOfType<T>() where T : BaseTag
        {
            var tagType = typeof(T).GetCustomAttribute<TagLabelAttribute>();

            if (tagType == null) return Array.Empty<T>();

            return this.TagIndex.Values.Where(i => i.Tag == tagType.Label)
                .Select(e => this.GetTag(e))
                .OfType<T>();
        }

        public T GetTag<T>(TagRef<T> tagref) where T : BaseTag
        {
            if (TryGetTag(tagref, out var t))
            {
                return t;
            }

            throw new Exception($"Unable to find tag {tagref.Id}");
        }

        public bool TryGetTag<T>(TagRef<T> tagref, out T tag) where T : BaseTag
        {
            if (tagref.IsInvalid)
            {
                tag = null;
                return false;
            }

            if (TryGetTag(tagref.Id, out tag))
            {
                return true;
            }

            Console.WriteLine("Couldn't find " + tagref);
            return false;
        }

        public T GetTag<T>(uint id) where T : BaseTag
        {
            if (TryGetTag<T>(id, out var t))
            {
                return t;
            }

            throw new Exception($"Unable to find tag {id}");
        }

        public bool TryGetTag<T>(uint id, out T tag) where T : BaseTag
        {
            if (id == uint.MaxValue)
            {
                tag = null;
                return false;
            }

            if (this.tags.TryGetValue(id, out var t))
            {
                tag = (T)t;
                return true;
            }

            if (this.TagIndex.TryGetValue(id, out var indexEntry))
            {
                tag = (T)this.GetTag(indexEntry);
                return true;
            }

            if (mpShared.TryGetTag(id, out t))
            {
                tag = (T)t;
                return true;
            }

            if (spShared.TryGetTag(id, out t))
            {
                tag = (T)t;
                return true;
            }

            if (mainMenu.TryGetTag(id, out t))
            {
                tag = (T)t;
                return true;
            }

            Console.WriteLine($"TryGetTag miss [{id}]");

            tag = null;
            return false;
        }

        public bool TryFindTagId(TagName tag, string fullName, out uint id)
        {
            id = default;

            if (Enum.IsDefined(typeof(TagName), tag) == false)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                return false;
            }

            foreach(var entry in this.TagIndex.Values)
            {
                if(entry.Tag == tag)
                {
                    var name = GetTagName(entry.ID);
                    if(name == fullName)
                    {
                        id = entry.ID;
                        return true;
                    }
                }
            }

            return false;
        }

        public SecondaryOffset GetSecondaryOffset(DataFile source, int rawOffset)
        {
            return source switch
            {
                DataFile.Local => new SecondaryOffset(this, rawOffset),
                DataFile.MainMenu => new SecondaryOffset(this.mainMenu, rawOffset),
                DataFile.Shared => new SecondaryOffset(this.mpShared, rawOffset),
                DataFile.SinglePlayerShared => new SecondaryOffset(this.spShared, rawOffset),
                _ => throw new NotSupportedException()
            };
        }

        public Memory<byte> ReadData(DataFile source, IOffset offset, int length)
        {
            var reader = this.mapStream.GetStream((byte)source);
            reader.Position = offset.Value;

            var data = new byte[length];
            var read = reader.Read(data);

            Debug.Assert(read == length);

            return data;
        }

        public int CalculateSignature()
        {
            return H2BaseMap.CalculateSignature(this.localStream);
        }

        protected Dictionary<uint, BaseTag> tags = new();
        protected BaseTag GetTag(TagIndexEntry entry)
        {
            if(tags.TryGetValue(entry.ID, out var tag))
            {
                return tag;
            }

            var name = GetTagName(entry.ID);

            tag = TagFactory.CreateTag(entry.ID, name, entry, this, this.mapStream);
            tags[entry.ID] = tag;

            return tag;
        }

        private Dictionary<uint, string> tagNames = new();
        protected string GetTagName(uint id)
        {
            if(tagNames.TryGetValue(id, out var name))
            {
                return name;
            }

            var nameIndexOffset = (short)(id & 0x0000FFFF) * 4;
            var nameStart = this.localStream.ReadInt32At(this.Header.FilesIndex + nameIndexOffset);
            name = this.localStream.ReadStringFrom(this.Header.FileTableOffset + nameStart, 128);

            tagNames[id] = name;

            return name;
        }

        public abstract void LoadWellKnownTags();
    }
}
