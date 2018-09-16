using System;
using System.Collections.Generic;
using OpenH2.Core.Extensions;
using OpenH2.Core.Tags;

namespace OpenH2.Core.Representations
{

    /// This class is the in-memory representation of a .map file
    public class Scene
    {
        public Memory<byte> RawData { get; set; }

        public TagNode TreeRoot { get; set; }

        public SceneHeader Header { get; set; }

        public List<string> Files { get; set; }

        public IndexHeader IndexHeader { get; set; }

        public List<TagListEntry> TagList { get; set; }

        public List<ObjectIndexEntry> ObjectList { get; set; }

        public string Name => this.Header.Name;


        internal Scene()
        {

        }

        public int CalculateSignature()
        {
            var sig = 0;
            var span = RawData.Span;

            for(var i = 2048; i < RawData.Length; i+=4)
            {
                sig ^= span.IntFromSlice(i);
            }

            return sig;
        }

    }
}
