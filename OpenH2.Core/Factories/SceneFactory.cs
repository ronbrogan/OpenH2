using OpenH2.Core.Extensions;
using OpenH2.Core.Offsets;
using OpenH2.Core.Representations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenH2.Core.Factories
{
    public class SceneFactory
    {

        public SceneFactory()
        {

        }

        public Scene FromFile(Stream fileStream)
        {
            var data = new byte[fileStream.Length];

            fileStream.Read(data, 0, (int)fileStream.Length);

            var memory = new Memory<byte>(data);

            var headerData = memory.Slice(0, 2048).Span;

            var scene = new Scene();
            scene.RawData = memory;

            var head = this.GetMetadata(scene, headerData);

            scene.Header = head;
            scene.Files = GetFiles(memory.Span.Slice(head.FileTableOffset, head.FileTableSize));
            scene.IndexHeader = GetIndexHeader(scene, memory);
            scene.PrimaryMagic = CalculatePrimaryMagic(scene.IndexHeader);
            scene.TagList = GetTagList(scene.IndexHeader, memory);
            scene.ObjectList = GetObjectIndexList(scene, memory);
            scene.SecondaryMagic = CalculateSecondaryMagic(scene.Header, scene.ObjectList.First());

            return scene;
        }

        private SceneHeader GetMetadata(Scene scene, Span<byte> data)
        {
            var factory = new HeaderFactory();
            return factory.Create(scene, data);
        }

        private List<string> GetFiles(Span<byte> data)
        {
            var files = new List<string>();

            var lastStart = 0;

            for(var i = 0; i < data.Length; i++)
            {
                if(data[i] == 0x00)
                {
                    files.Add(data.StringFromSlice(lastStart, i - lastStart));

                    lastStart = i+1;
                }
            }

            return files;
        }

        public IndexHeader GetIndexHeader(Scene scene, Memory<byte> sceneData)
        {
            var header = scene.Header;
            var span = sceneData.Slice(header.IndexOffset.Value, IndexHeader.Length).Span;

            var index = new IndexHeader();

            index.FileRawOffset = header.IndexOffset;
            index.PrimaryMagicConstant = span.IntFromSlice(0);
            index.TagListCount = span.IntFromSlice(4);
            index.ObjectIndexOffset = new PrimaryOffset(scene, span.IntFromSlice(8));
            index.ScenarioID = span.IntFromSlice(12);
            index.TagIDStart = span.IntFromSlice(16);
            index.Unknown1 = span.IntFromSlice(20);
            index.ObjectCount = span.IntFromSlice(24);
            index.TagsLabel = span.StringFromSlice(28, 4);

            return index;
        }

        public List<TagListEntry> GetTagList(IndexHeader index, Memory<byte> scene)
        {
            var span = scene.Slice(index.FileRawOffset.Value + IndexHeader.Length, index.TagListCount * 12).Span;

            var list = new List<TagListEntry>();
            var nullVal = new string(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }.Select(b => (char)b).ToArray());

            for(var i = 0; i < index.TagListCount; i++)
            {
                var entry = new TagListEntry();
                var entryBase = i * 12;

                entry.Class = span.StringFromSlice(entryBase, 4).Reverse();
                entry.ParentClass = span.StringFromSlice(entryBase + 4, 4).Reverse();
                if (entry.ParentClass == nullVal)
                    entry.ParentClass = null;
                entry.GrandparentClass = span.StringFromSlice(entryBase + 8, 4).Reverse();
                if (entry.GrandparentClass == nullVal)
                    entry.GrandparentClass = null;

                list.Add(entry);
            }

            return list;
        }

        public List<ObjectIndexEntry> GetObjectIndexList(Scene scene, Memory<byte> sceneBytes)
        {
            var index = scene.IndexHeader;
            var listBytes = sceneBytes.Slice(index.ObjectIndexOffset.Value, index.ObjectCount * ObjectIndexEntry.Size).Span;
            var nullObjTag = new string(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }.Select(b => (char)b).ToArray());

            var list = new List<ObjectIndexEntry>();

            for(var i = 0; i < index.ObjectCount; i++)
            {
                var entryBase = i * 16;

                var tag = listBytes.StringFromSlice(entryBase, 4).Reverse();

                //if (tag == nullObjTag)
                //    continue;

                var entry = new ObjectIndexEntry();
                entry.Tag = tag;
                entry.ID = (uint)listBytes.IntFromSlice(entryBase + 4);
                entry.Offset = new SecondaryOffset(scene, listBytes.IntFromSlice(entryBase + 8));
                entry.MetaSize = listBytes.IntFromSlice(entryBase + 12);

                list.Add(entry);
            }

            return list;
        }

        public int CalculatePrimaryMagic(IndexHeader index)
        {
            return index.FileRawOffset.Value - index.PrimaryMagicConstant + IndexHeader.Length;
        }

        public int CalculateSecondaryMagic(SceneHeader header, ObjectIndexEntry firstObj)
        {
            return firstObj.Offset.OriginalValue - header.MetaOffset.Value;
        }
    }
}