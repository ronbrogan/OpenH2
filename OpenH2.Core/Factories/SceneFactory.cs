using OpenH2.Core.Extensions;
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

            var head = this.GetMetadata(headerData);

            var scene = new Scene();
            scene.RawData = memory;
            scene.Header = head;
            scene.Files = GetFiles(memory.Span.Slice(head.FileTableOffset, head.FileTableSize));
            scene.IndexHeader = GetIndexHeader(head, memory);
            scene.TagList = GetTagList(scene.IndexHeader, memory);
            scene.ObjectList = GetObjectIndexList(scene.IndexHeader, memory);

            return scene;
        }

        private SceneHeader GetMetadata(Span<byte> data)
        {
            var factory = new HeaderFactory();
            return factory.Create(data);
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

        public IndexHeader GetIndexHeader(SceneHeader header, Memory<byte> scene)
        {
            var span = scene.Slice(header.IndexOffset, 32).Span;

            var index = new IndexHeader();

            index.Offset = header.IndexOffset;
            index.PrimaryMagicConstant = span.IntFromSlice(0);
            index.TagListCount = span.IntFromSlice(4);
            index.ObjectIndexOffset = span.IntFromSlice(8);
            index.ScenarioID = span.IntFromSlice(12);
            index.TagIDStart = span.IntFromSlice(16);
            index.Unknown1 = span.IntFromSlice(20);
            index.ObjectCount = span.IntFromSlice(24);
            index.TagsLabel = span.StringFromSlice(28, 4);

            return index;
        }

        public List<TagListEntry> GetTagList(IndexHeader indexHeader, Memory<byte> scene)
        {
            var span = scene.Slice(indexHeader.Offset + 32, indexHeader.TagListCount * 12).Span;

            var list = new List<TagListEntry>();
            var nullVal = new string(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }.Select(b => (char)b).ToArray());

            for(var i = 0; i < indexHeader.TagListCount; i++)
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

        public List<ObjectIndexEntry> GetObjectIndexList(IndexHeader header, Memory<byte> scene)
        {
            var span = scene.Slice(header.Offset + header.ObjectIndexOffset, header.ObjectCount * 16).Span;
            var nullObjTag = new string(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }.Select(b => (char)b).ToArray());

            var list = new List<ObjectIndexEntry>();

            for(var i = 0; i < header.ObjectCount; i++)
            {
                var entryBase = i * 16;

                var tag = span.StringFromSlice(entryBase, 4).Reverse();

                //if (tag == nullObjTag)
                //    continue;

                var entry = new ObjectIndexEntry();
                entry.Tag = tag;
                entry.ID = (uint)span.IntFromSlice(entryBase + 4);
                entry.Offset = span.IntFromSlice(entryBase + 8);
                entry.MetaSize = span.IntFromSlice(entryBase + 12);

                list.Add(entry);
            }

            return list;
        }
    }
}