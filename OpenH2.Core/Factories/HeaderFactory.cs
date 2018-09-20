using OpenH2.Core.Extensions;
using OpenH2.Core.Offsets;
using OpenH2.Core.Representations;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Factories
{
    public class HeaderFactory
    {
        public SceneHeader Create(Scene scene, Span<byte> data)
        {
            var head = new SceneHeader();

            head.FileHead = GetFileHeadValue(data);
            head.Version = GetVersion(data);
            head.TotalBytes = GetTotalSize(data);
            head.IndexOffset = new NormalOffset(GetIndexOffset(data));
            head.MetaOffset = new PrimaryOffset(scene, GetMetaOffset(data));
            head.MapOrigin = GetOrigin(data);
            head.Build = GetBuild(data);
            head.OffsetToUnknownSection = GetUnknownSection(data);
            head.ScriptReferenceCount = GetScriptRefCount(data);
            head.SizeOfScriptReference = GetSizeOfScriptRef(data);
            head.OffsetToScriptReferenceIndex = GetOffsetToScriptRefIndex(data);
            head.OffsetToScriptReferenceStrings = GetOffsetToScriptRefStrings(data);
            head.Name = GetName(data);
            head.ScenarioPath = GetScenarioPath(data);
            head.FileCount = GetFileCount(data);
            head.FileTableOffset = GetFileTableOffset(data);
            head.FileTableSize = GetFileTableSize(data);
            head.FilesIndex = GetFilesIndex(data);
            head.StoredSignature = GetSignature(data);
            head.Footer = GetFooter(data);

            return head;
        }

        private string GetFooter(Span<byte> data)
        {
            return data.StringFromSlice(2044, 4);
        }

        private int GetSignature(Span<byte> data)
        {
            return data.IntFromSlice(752);
        }

        private int GetFilesIndex(Span<byte> data)
        {
            return data.IntFromSlice(728);
        }

        private int GetFileTableSize(Span<byte> data)
        {
            return data.IntFromSlice(724);
        }

        private int GetFileTableOffset(Span<byte> data)
        {
            return data.IntFromSlice(720);
        }

        private int GetFileCount(Span<byte> data)
        {
            return data.IntFromSlice(716);
        }

        public string GetScenarioPath(Span<byte> data)
        {
            return data.StringFromSlice(456, 256);
        }

        public int GetOffsetToScriptRefStrings(Span<byte> data)
        {
            return data.IntFromSlice(380);
        }

        public int GetOffsetToScriptRefIndex(Span<byte> data)
        {
            return data.IntFromSlice(376);
        }

        public int GetSizeOfScriptRef(Span<byte> data)
        {
            return data.IntFromSlice(372);
        }

        public int GetScriptRefCount(Span<byte> data)
        {
            return data.IntFromSlice(368);
        }

        public int GetUnknownSection(Span<byte> data)
        {
            return data.IntFromSlice(364);
        }

        public string GetBuild(Span<byte> data)
        {
            return data.StringFromSlice(300, 32);
        }

        public string GetOrigin(Span<byte> data)
        {
            return data.StringFromSlice(32, 32);
        }

        public int GetMetaOffset(Span<byte> data)
        {
            return data.IntFromSlice(20);
        }

        public int GetIndexOffset(Span<byte> data)
        {
            return data.IntFromSlice(16);
        }

        public int GetTotalSize(Span<byte> data)
        {
            return data.IntFromSlice(8);
        }

        public string GetFileHeadValue(Span<byte> data)
        {
            var offset = 0;
            var length = 4;

            return data.StringFromSlice(offset, length);
        }

        public int GetVersion(Span<byte> data)
        {
            return data.IntFromSlice(4);
        }

        public string GetName(Span<byte> data)
        {
            var offset = 420;
            var length = 32;

            return data.StringFromSlice(offset, length);
        }

        
    }
}
