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
    public class SceneFactory
    {
        public Scene FromFile(Stream fileStream)
        {
            var reader = new TrackingReader(fileStream.ToMemory());

            return this.InternalFromFile(reader);
        }

        public Scene FromFile(Stream fileStream, out CoverageReport coverage)
        {
            var reader = new TrackingReader(fileStream.ToMemory());

            var scene = this.InternalFromFile(reader);
            coverage = reader.GenerateReport();
            return scene;
        }

        private Scene InternalFromFile(TrackingReader reader)
        {
            var scene = new Scene();
            scene.RawData = reader.Memory;

            this.ExtractMetadata(scene, reader);
            
            return scene;
        }

        private void ExtractMetadata(Scene scene, TrackingReader reader)
        {
            scene.Header = GetSceneHeader(scene, reader);
            scene.IndexHeader = GetIndexHeader(scene, reader);
            scene.PrimaryMagic = CalculatePrimaryMagic(scene.IndexHeader);
            scene.TagIndex = GetObjectIndexList(scene, reader);
            scene.SecondaryMagic = CalculateSecondaryMagic(scene.Header, scene.TagIndex.First());

            scene.Tags = GetTags(scene, reader);
        }

        private Dictionary<uint, BaseTag> GetTags(Scene scene, TrackingReader reader)
        {
            var dict = new Dictionary<uint, BaseTag>();
            var index = scene.TagIndex;

            var fileIndex = reader.Chunk(scene.Header.FilesIndex, scene.Header.FileCount * 4, "FileIndex").Span;
            var fileTable = reader.Chunk(scene.Header.FileTableOffset, scene.Header.FileTableSize, "FileTable").Span;

            foreach(var item in index)
            {
                if (item == null || item.MetaSize == 0 || item.Offset.OriginalValue == 0)
                    continue;

                var chunk = reader.Chunk(item.Offset.Value, item.MetaSize, "Tag");

                var nameIndex = (short)(item.ID & 0x0000FFFF);
                var nameStart = fileIndex.ReadInt32At(4 * nameIndex);
                var name = fileTable.ReadStringStarting(nameStart);

                dict[item.ID] = TagFactory.CreateTag(item.ID, name, item, chunk, reader);
            }

            return dict;
        }

        private SceneHeader GetSceneHeader(Scene scene, TrackingReader reader)
        {
            var head = new SceneHeader();
            var span = reader.Chunk(SceneHeader.Layout.Offset, SceneHeader.Layout.Length, "Header").Span;

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

        public IndexHeader GetIndexHeader(Scene scene, TrackingReader reader)
        {
            var header = scene.Header;
            var span = reader.Chunk(header.IndexOffset.Value, IndexHeader.Length, "IndexHeader").Span;

            var index = new IndexHeader();

            index.FileRawOffset =         /**/  header.IndexOffset;
            index.PrimaryMagicConstant =  /**/  span.ReadInt32At(0);
            index.TagListCount =          /**/  span.ReadInt32At(4);
            index.ObjectIndexOffset =     /**/  scene.PrimaryOffset(span.ReadInt32At(8));
            index.ScenarioID =            /**/  span.ReadInt32At(12);
            index.TagIDStart =            /**/  span.ReadInt32At(16);
            index.Unknown1 =              /**/  span.ReadInt32At(20);
            index.ObjectCount =           /**/  span.ReadInt32At(24);
            index.TagsLabel =             /**/  span.ReadStringFrom(28, 4);

            return index;
        }

        public TagTree GetTagList(IndexHeader index, TrackingReader reader)
        {
            var span = reader.Chunk(index.FileRawOffset.Value + IndexHeader.Length, index.TagListCount * 12, "TagTree").Span;

            var tree = new TagTree();
            var nullVal = new string(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }.Select(b => (char)b).ToArray());

            for (var i = 0; i < index.TagListCount; i++)
            {
                var entryBase = i * 12;

                var c = span.ReadStringFrom(entryBase, 4).Reverse();
                var p = span.ReadStringFrom(entryBase + 4, 4).Reverse();
                if (p == nullVal)
                    p = null;
                var gp = span.ReadStringFrom(entryBase + 8, 4).Reverse();
                if (gp == nullVal)
                    gp = null;

                tree.Add(c, p, gp);
            }

            return tree;
        }

        public List<TagIndexEntry> GetObjectIndexList(Scene scene, TrackingReader reader)
        {
            var index = scene.IndexHeader;
            var listBytes = reader.Chunk(index.ObjectIndexOffset.Value, index.ObjectCount * TagIndexEntry.Size, "ObjectIndexList").Span;
            var nullObjTag = new string(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }.Select(b => (char)b).ToArray());

            var list = new List<TagIndexEntry>(index.ObjectCount);

            for (var i = 0; i < index.ObjectCount; i++)
            {
                var entryBase = i * 16;

                var tag = listBytes.ReadStringFrom(entryBase, 4).Reverse();

                if (tag == nullObjTag)
                    continue;

                var entry = new TagIndexEntry
                {
                    Tag = tag,
                    ID = listBytes.ReadUInt32At(entryBase + 4),
                    Offset = new SecondaryOffset(scene, listBytes.ReadInt32At(entryBase + 8)),
                    MetaSize = listBytes.ReadInt32At(entryBase + 12)
                };

                if (entry.MetaSize == 0)
                    continue;

                list.Add(entry);
            }

            return list;
        }

        public int CalculatePrimaryMagic(IndexHeader index)
        {
            return index.FileRawOffset.Value - index.PrimaryMagicConstant + IndexHeader.Length;
        }

        public int CalculateSecondaryMagic(SceneHeader header, TagIndexEntry firstObj)
        {
            return firstObj.Offset.OriginalValue - header.MetaOffset.Value;
        }
    }
}