using OpenH2.Core.Extensions;
using OpenH2.Core.Offsets;
using OpenH2.Core.Parsing;
using OpenH2.Core.Maps;
using OpenH2.Core.Tags;
using OpenH2.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenH2.Core.Factories
{
    public class MapFactory
    {
        private const string MainMenuName = "mainmenu.map";
        private const string MultiPlayerSharedName = "shared.map";
        private const string SinglePlayerSharedName = "single_player_shared.map";
        private readonly IMaterialFactory materialFactory;
        private H2MapReader baseReader;
        private H2vLazyLoadingMap mainMenu;
        private H2vLazyLoadingMap spShared;
        private H2vLazyLoadingMap mpShared;

        // TODO: If sp maps only reference spShared and mp maps only reference mpShared, 
        // only load the relevant maps when a playable map is loaded
        public MapFactory(string mapRoot, IMaterialFactory materialFactory)
        {
            baseReader = GetBaseReader(mapRoot);

            mainMenu = LazyLoadingMapFromReader(new H2MapReader(baseReader.MainMenu, baseReader));
            spShared = LazyLoadingMapFromReader(new H2MapReader(baseReader.SpShared, baseReader));
            mpShared = LazyLoadingMapFromReader(new H2MapReader(baseReader.MpShared, baseReader));
            this.materialFactory = materialFactory;
        }

        private H2MapReader GetBaseReader(string mapRoot)
        {
            var bufferSize = 81000;
            var mm = new FileStream(Path.Combine(mapRoot, MainMenuName), FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize);
            var mp = new FileStream(Path.Combine(mapRoot, MultiPlayerSharedName), FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize);
            var sp = new FileStream(Path.Combine(mapRoot, SinglePlayerSharedName), FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize);

            var mmReader = new TrackingReader(mm);
            var mpReader = new TrackingReader(mp);
            var spReader = new TrackingReader(sp);

            return new H2MapReader(mmReader, mpReader, spReader);
        }

        public H2vMap FromFile(FileStream fileStream)
        {
            var reader = FromFileStream(fileStream);

            return this.InternalFromFile(reader);
        }

        public H2vMap FromFile(FileStream fileStream, out CoverageReport coverage)
        {
            var reader = FromFileStream(fileStream);

            var scene = this.InternalFromFile(reader);
            coverage = reader.MapReader.GenerateReport();
            return scene;
        }

        private H2MapReader FromFileStream(FileStream fileStream)
        {
            var ms = new MemoryStream();

            fileStream.CopyTo(ms);
            ms.Position = 0;

            fileStream.Dispose();

            var mapReader = new TrackingReader(ms);

            return new H2MapReader(mapReader, baseReader);
        }

        private H2vMap InternalFromFile(H2MapReader reader, bool lazyLoadTags = false)
        {
            var scene = new H2vMap(reader, mainMenu, mpShared, spShared);
            scene.UseMaterialFactory(this.materialFactory);

            this.LoadMetadata(scene, reader);

            if(lazyLoadTags == false)
            {
                LoadAllTags(scene, reader);
            }
            
            return scene;
        }

        private H2vLazyLoadingMap LazyLoadingMapFromReader(H2MapReader reader)
        {
            var scene = new H2vLazyLoadingMap(reader);

            this.LoadMetadata(scene, reader);

            return scene;
        }

        private void LoadMetadata(H2BaseMap scene, H2MapReader reader)
        {
            scene.Header = GetSceneHeader(scene, reader.MapReader);
            scene.IndexHeader = GetIndexHeader(scene, reader.MapReader);
            scene.PrimaryMagic = CalculatePrimaryMagic(scene.IndexHeader);
            scene.TagIndex = GetTagIndex(scene, reader.MapReader, out var firstOffset);
            scene.SecondaryMagic = CalculateSecondaryMagic(scene.Header, firstOffset);
            scene.InternedStrings = GetAllStrings(scene, reader);
            scene.TagNames = GetTagNames(scene, reader);
        }


        private Dictionary<int, string> GetAllStrings(H2BaseMap scene, H2MapReader reader)
        {
            var dict = new Dictionary<int, string>();

            var start = scene.Header.InternedStringIndexOffset;

            for(var i = 0; i < scene.Header.InternedStringCount; i++)
            {
                var offset = reader.MapReader.ReadInt32At(start + i * 4);
                var value = reader.MapReader.ReadStringStarting(scene.Header.InternedStringsOffset + offset);

                dict.Add(i, value);
            }

            return dict;
        }

        private Dictionary<uint, string> GetTagNames(H2BaseMap scene, H2MapReader reader)
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

        private void LoadAllTags(H2vMap scene, H2MapReader reader)
        {
            scene.SetTags(GetTags(scene, reader));
        }

        private Dictionary<uint, BaseTag> GetTags(H2vMap scene, H2MapReader reader)
        {
            var dict = new Dictionary<uint, BaseTag>();
            var index = scene.TagIndex.Values.OrderBy(i => i.Offset.Value);

            foreach(var item in index)
            {
                if (item.DataSize == 0 || item.Offset.OriginalValue == 0)
                    continue;

                dict[item.ID] = GetTag(scene, item, reader);
            }

            return dict;
        }

        public static BaseTag GetTag(H2BaseMap scene, TagIndexEntry entry, H2MapReader reader)
        {
            var name = scene.TagNames[entry.ID];

            return TagFactory.CreateTag(entry.ID, name, entry, scene, reader);
        }

        private H2vMapHeader GetSceneHeader(H2BaseMap scene, TrackingReader reader)
        {
            var head = BlamSerializer.Deserialize<H2vMapHeader>(reader.Data, instanceStart: 0);

            head.SecondaryOffset = scene.PrimaryOffset(head.RawSecondaryOffset);

            return head;
        }

        public IndexHeader GetIndexHeader(H2BaseMap scene, TrackingReader reader)
        {
            var header = scene.Header;

            var index = BlamSerializer.Deserialize<IndexHeader>(reader.Data, header.IndexOffset.Value);
            index.FileRawOffset = header.IndexOffset;
            index.TagIndexOffset = scene.PrimaryOffset(index.RawTagIndexOffset);

            return index;
        }

        public Dictionary<uint, TagIndexEntry> GetTagIndex(H2BaseMap scene, TrackingReader reader, out int firstEntryOffset)
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

                if(firstEntryOffset == -1)
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