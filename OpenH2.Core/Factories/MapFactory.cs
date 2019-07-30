using OpenH2.Core.Enums;
using OpenH2.Core.Extensions;
using OpenH2.Core.Offsets;
using OpenH2.Core.Parsing;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using System;
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

        private H2vReader baseReader;
        private H2vLazyLoadingMap mainMenu;
        private H2vLazyLoadingMap spShared;
        private H2vLazyLoadingMap mpShared;

        // TODO: If sp maps only reference spShared and mp maps only reference mpShared, 
        // only load the relevant maps when a playable map is loaded
        public MapFactory(string mapRoot)
        {
            baseReader = GetBaseReader(mapRoot);

            mainMenu = LazyLoadingMapFromReader(new H2vReader(baseReader.MainMenu, baseReader));
            spShared = LazyLoadingMapFromReader(new H2vReader(baseReader.SpShared, baseReader));
            mpShared = LazyLoadingMapFromReader(new H2vReader(baseReader.MpShared, baseReader));
        }

        private H2vReader GetBaseReader(string mapRoot)
        {
            using (var mm = File.OpenRead(Path.Combine(mapRoot, MainMenuName)))
            using (var mp = File.OpenRead(Path.Combine(mapRoot, MultiPlayerSharedName)))
            using (var sp = File.OpenRead(Path.Combine(mapRoot, SinglePlayerSharedName)))
            {
                var mmReader = new TrackingReader(mm.ToMemory());
                var mpReader = new TrackingReader(mp.ToMemory());
                var spReader = new TrackingReader(sp.ToMemory());

                return new H2vReader(mmReader, mpReader, spReader);
            }
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

        private H2vReader FromFileStream(FileStream fileStream)
        {
            var mapReader = new TrackingReader(fileStream.ToMemory());

            return new H2vReader(mapReader, baseReader);
        }

        private H2vMap InternalFromFile(H2vReader reader, bool lazyLoadTags = false)
        {
            var scene = new H2vMap(reader, mainMenu, mpShared, spShared);

            this.LoadMetadata(scene, reader);

            if(lazyLoadTags == false)
            {
                LoadAllTags(scene, reader);
            }
            
            return scene;
        }

        private H2vLazyLoadingMap LazyLoadingMapFromReader(H2vReader reader)
        {
            var scene = new H2vLazyLoadingMap(reader);

            this.LoadMetadata(scene, reader);

            return scene;
        }

        private void LoadMetadata(H2vBaseMap scene, H2vReader reader)
        {
            scene.Header = GetSceneHeader(scene, reader.MapReader);
            scene.IndexHeader = GetIndexHeader(scene, reader.MapReader);
            scene.PrimaryMagic = CalculatePrimaryMagic(scene.IndexHeader);
            scene.TagIndex = GetTagIndex(scene, reader.MapReader);
            scene.SecondaryMagic = CalculateSecondaryMagic(scene.Header, scene.TagIndex.First());
        }

        private void LoadAllTags(H2vMap scene, H2vReader reader)
        {
            scene.SetTags(GetTags(scene, reader));
        }

        private Dictionary<uint, BaseTag> GetTags(H2vMap scene, H2vReader reader)
        {
            var dict = new Dictionary<uint, BaseTag>();
            var index = scene.TagIndex;

            foreach(var item in index)
            {
                if (item == null || item.DataSize == 0 || item.Offset.OriginalValue == 0)
                    continue;

                dict[item.ID] = GetTag(scene, item, reader);
            }

            return dict;
        }

        public static BaseTag GetTag(H2vBaseMap scene, TagIndexEntry entry, H2vReader reader)
        {
            var nameIndexOffset = (short)(entry.ID & 0x0000FFFF) * 4;
            var nameStart = reader.MapReader.Span.ReadInt32At(scene.Header.FilesIndex + nameIndexOffset);
            var name = reader.MapReader.Span.ReadStringStarting(scene.Header.FileTableOffset + nameStart);

            return TagFactory.CreateTag(entry.ID, name, entry, scene.SecondaryMagic, reader);
        }

        private H2vMapHeader GetSceneHeader(H2vBaseMap scene, TrackingReader reader)
        {
            var head = new H2vMapHeader();
            var span = reader.Chunk(H2vMapHeader.Layout.Offset, H2vMapHeader.Layout.Length, "Header").Span;

            head.FileHead =                        /**/  span.ReadStringFrom(0, 4);
            head.Version =                         /**/  span.ReadInt32At(4);
            head.TotalBytes =                      /**/  span.ReadInt32At(8);
            head.IndexOffset =                     /**/  new NormalOffset(span.ReadInt32At(16));
            head.MetaOffset =                      /**/  scene.PrimaryOffset(span.ReadInt32At(20));
            head.MapOrigin =                       /**/  span.ReadStringFrom(32, 32);
            head.Build =                           /**/  span.ReadStringFrom(300, 32);
            head.OffsetToUnknownSection =          /**/  span.ReadInt32At(364);
            head.ScriptReferenceCount =            /**/  span.ReadInt32At(368);
            head.SizeOfScriptReference =           /**/  span.ReadInt32At(372);
            head.OffsetToScriptReferenceIndex =    /**/  span.ReadInt32At(376);
            head.OffsetToScriptReferenceStrings =  /**/  span.ReadInt32At(380);
            head.Name =                            /**/  span.ReadStringFrom(420, 32);
            head.ScenarioPath =                    /**/  span.ReadStringFrom(456, 256);
            head.FileCount =                       /**/  span.ReadInt32At(716);
            head.FileTableOffset =                 /**/  span.ReadInt32At(720);
            head.FileTableSize =                   /**/  span.ReadInt32At(724);
            head.FilesIndex =                      /**/  span.ReadInt32At(728);
            head.StoredSignature =                 /**/  span.ReadInt32At(752);
            head.Footer =                          /**/  span.ReadStringFrom(2044, 4);

            return head;
        }

        public IndexHeader GetIndexHeader(H2vBaseMap scene, TrackingReader reader)
        {
            var header = scene.Header;
            var span = reader.Chunk(header.IndexOffset.Value, IndexHeader.Length, "IndexHeader").Span;

            var index = new IndexHeader();

            index.FileRawOffset =         /**/  header.IndexOffset;
            index.PrimaryMagicConstant =  /**/  span.ReadInt32At(0);
            index.TagListCount =          /**/  span.ReadInt32At(4);
            index.TagIndexOffset =     /**/  scene.PrimaryOffset(span.ReadInt32At(8));
            index.ScenarioID =            /**/  span.ReadInt32At(12);
            index.TagIDStart =            /**/  span.ReadInt32At(16);
            index.Unknown1 =              /**/  span.ReadInt32At(20);
            index.TagIndexCount =           /**/  span.ReadInt32At(24);
            index.TagsLabel =             /**/  span.ReadStringFrom(28, 4);

            return index;
        }

        public TagIndexEntry[] GetTagIndex(H2vBaseMap scene, TrackingReader reader)
        {
            var index = scene.IndexHeader;
            var listBytes = reader.Chunk(index.TagIndexOffset.Value, index.TagIndexCount * TagIndexEntry.Size, "TagIndex").Span;
            var nullObjTag = new string(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }.Select(b => (char)b).ToArray());

            var list = new List<TagIndexEntry>(index.TagIndexCount);

            for (var i = 0; i < index.TagIndexCount; i++)
            {
                var entryBase = i * 16;

                // PERF: determine how frequent nullTags are, this could be a waste
                // It could be a waste no matter what, 2 string allocs when only one is needed
                var tag = listBytes.ReadStringFrom(entryBase, 4).Reverse();

                if (tag == nullObjTag)
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

                list.Add(entry);
            }

            return list.ToArray();
        }

        public int CalculatePrimaryMagic(IndexHeader index)
        {
            return index.FileRawOffset.Value - index.PrimaryMagicConstant + IndexHeader.Length;
        }

        public int CalculateSecondaryMagic(H2vMapHeader header, TagIndexEntry firstObj)
        {
            return firstObj.Offset.OriginalValue - header.MetaOffset.Value;
        }
    }
}