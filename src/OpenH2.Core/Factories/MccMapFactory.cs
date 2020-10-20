using OpenH2.Core.Extensions;
using OpenH2.Core.Maps;
using OpenH2.Core.Maps.MCC;
using OpenH2.Core.Offsets;
using OpenH2.Core.Parsing;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenH2.Core.Factories
{
    // TODO: merge with Map factory, auto detect version
    public class MccMapFactory
    {
        public H2mccMap FromStream(Stream stream)
        {
            var fourCC = stream.ReadUInt32At(0);
            if(fourCC != H2mccCompression.DecompressedFourCC)
            {
                throw new System.Exception("Cannot load a map that hasn't been decompressed first");
            }

            var reader = ReaderFromStream(stream);

            return this.InternalFromFile(reader);
        }

        private H2MapReader ReaderFromStream(Stream stream)
        {
            stream.Position = 0;

            var mapReader = new TrackingReader(stream);

            return new H2MapReader(mapReader, new H2MapReader(null, null, null));
        }

        private H2mccMap InternalFromFile(H2MapReader reader, bool lazyLoadTags = false)
        {
            var scene = new H2mccMap(reader);

            this.LoadMetadata(scene, reader);

            // Tags are different enough to break things, disabling until versioned deserialization is implemented
            //LoadAllTags(scene, reader);

            scene.Scenario = (ScenarioTag)GetTag(scene, scene.TagIndex[scene.IndexHeader.Scenario.Id], reader);

            return scene;
        }

        private void LoadMetadata(H2mccMap scene, H2MapReader reader)
        {
            scene.Header = GetSceneHeader(scene, reader.MapReader);
            scene.IndexHeader = GetIndexHeader(scene, reader.MapReader);
            scene.PrimaryMagic = CalculatePrimaryMagic(scene.IndexHeader);
            scene.TagIndex = GetTagIndex(scene, reader.MapReader, out var firstOffset);
            scene.SecondaryMagic = CalculateSecondaryMagic(scene.Header, firstOffset);
            scene.InternedStrings = GetAllStrings(scene, reader);
            scene.TagNames = GetTagNames(scene, reader);
            scene.TagNameLookup = scene.TagNames.ToDictionary(kv => (scene.TagIndex[kv.Key].Tag, kv.Value), kv => kv.Key);
        }


        private Dictionary<int, string> GetAllStrings(H2mccMap scene, H2MapReader reader)
        {
            var dict = new Dictionary<int, string>();

            var start = scene.Header.InternedStringIndexOffset;

            for (var i = 0; i < scene.Header.InternedStringCount; i++)
            {
                var offset = reader.MapReader.ReadInt32At(start + i * 4);
                var value = reader.MapReader.ReadStringStarting(scene.Header.InternedStringsOffset + offset);

                dict.Add(i, value);
            }

            return dict;
        }

        private Dictionary<uint, string> GetTagNames(H2mccMap scene, H2MapReader reader)
        {
            var dict = new Dictionary<uint, string>();
            var index = scene.TagIndex.Values.OrderBy(i => i.Offset.Value);

            reader.MapReader.Preload(scene.Header.FileTableOffset, scene.Header.FileTableSize);

            foreach (var item in index)
            {
                if (item.DataSize == 0 || item.Offset.OriginalValue == 0)
                    continue;

                var nameIndexOffset = (short)(item.ID & 0x0000FFFF) * 4;
                var nameStart = reader.MapReader.ReadInt32At(scene.Header.FilesIndex + nameIndexOffset);
                var name = reader.MapReader.ReadStringStarting(scene.Header.FileTableOffset + nameStart);

                dict[item.ID] = name;
            }

            return dict;
        }

        private void LoadAllTags(H2mccMap scene, H2MapReader reader)
        {
            scene.SetTags(GetTags(scene, reader));
        }

        private Dictionary<uint, BaseTag> GetTags(H2mccMap scene, H2MapReader reader)
        {
            var dict = new Dictionary<uint, BaseTag>();
            var index = scene.TagIndex.Values.OrderBy(i => i.Offset.Value);

            foreach (var item in index)
            {
                if (item.DataSize == 0 || item.Offset.OriginalValue == 0)
                    continue;

                dict[item.ID] = GetTag(scene, item, reader);
            }

            return dict;
        }

        public static BaseTag GetTag(H2mccMap scene, TagIndexEntry entry, H2MapReader reader)
        {
            var name = scene.TagNames[entry.ID];

            return TagFactory.CreateTag(entry.ID, name, entry, scene, reader);
        }

        private H2mccMapHeader GetSceneHeader(H2mccMap scene, TrackingReader reader)
        {
            var head = BlamSerializer.Deserialize<H2mccMapHeader>(reader.Data, instanceStart: 0);

            head.SecondaryOffset = scene.PrimaryOffset(head.RawSecondaryOffset);

            return head;
        }

        public IndexHeader GetIndexHeader(H2mccMap scene, TrackingReader reader)
        {
            var header = scene.Header;

            var index = BlamSerializer.Deserialize<IndexHeader>(reader.Data, header.IndexOffset.Value);
            index.FileRawOffset = header.IndexOffset;
            index.TagIndexOffset = scene.PrimaryOffset(index.RawTagIndexOffset);

            return index;
        }

        public Dictionary<uint, TagIndexEntry> GetTagIndex(H2mccMap scene, TrackingReader reader, out int firstEntryOffset)
        {
            firstEntryOffset = -1;
            var index = scene.IndexHeader;
            var listBytes = reader.Chunk(index.TagIndexOffset.Value, index.TagIndexCount * TagIndexEntry.Size, "TagIndex");

            var entries = new Dictionary<uint, TagIndexEntry>(index.TagIndexCount);

            for (var i = 0; i < index.TagIndexCount; i++)
            {
                var entryBase = i * 16;

                var tag = (TagName)listBytes.ReadUInt32At(entryBase);

                if (tag == TagName.NULL)
                    continue;

                var entry = new TagIndexEntry
                {
                    Tag = tag,
                    ID = listBytes.ReadUInt32At(entryBase + 4),
                    Offset = new SecondaryOffset(scene, listBytes.ReadInt32At(entryBase + 8)),
                    DataSize = listBytes.ReadInt32At(entryBase + 12)
                };

                if (entry.DataSize == 0)
                    continue;

                if (firstEntryOffset == -1)
                    firstEntryOffset = entry.Offset.OriginalValue;

                entries[entry.ID] = entry;
            }

            return entries;
        }

        public int CalculatePrimaryMagic(IndexHeader index)
        {
            return index.FileRawOffset.Value - index.PrimaryMagicConstant + IndexHeader.Length;
        }

        public int CalculateSecondaryMagic(IH2MapHeader header, int firstObjOffset)
        {
            return firstObjOffset - header.SecondaryOffset.Value;
        }
    }
}
