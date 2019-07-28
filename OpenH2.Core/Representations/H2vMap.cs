using OpenH2.Core.Extensions;
using OpenH2.Core.Tags;
using System;
using System.Collections.Generic;

namespace OpenH2.Core.Representations
{
    /// This class is the in-memory representation of a .map file
    public class H2vMap
    {
        public Memory<byte> RawData { get; set; }

        public H2vMapHeader Header { get; set; }

        public IndexHeader IndexHeader { get; set; }

        public TagTree TagTree { get; set; }

        public List<TagIndexEntry> TagIndex { get; set; }

        public Dictionary<uint, BaseTag> Tags { get; set; }

        public string Name => this.Header.Name;

        public int PrimaryMagic { get; set; }

        public int SecondaryMagic { get; set; }

        internal H2vMap()
        {
        }

        public int CalculateSignature()
        {
            var sig = 0;
            var span = RawData.Span;

            for(var i = 2048; i < RawData.Length; i+=4)
            {
                sig ^= span.ReadInt32At(i);
            }

            return sig;
        }
    }
}