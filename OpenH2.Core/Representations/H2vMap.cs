using OpenH2.Core.Extensions;
using OpenH2.Core.Tags;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenH2.Core.Representations
{
    /// This class is the in-memory representation of a .map file
    public class H2vMap
    {
        private readonly H2vMap mainMenu;
        private readonly H2vMap mpShared;
        private readonly H2vMap spShared;

        public Memory<byte> RawData { get; set; }

        public H2vMapHeader Header { get; set; }

        public IndexHeader IndexHeader { get; set; }

        public TagTree TagTree { get; set; }

        public List<TagIndexEntry> TagIndex { get; set; }

        internal Dictionary<uint, BaseTag> Tags { get; set; }

        public string Name => this.Header.Name;

        public int PrimaryMagic { get; set; }

        public int SecondaryMagic { get; set; }

        // TODO: replace map refs with callbacks that lazy load the tags?
        internal H2vMap(H2vMap mainMenu, H2vMap mpShared, H2vMap spShared)
        {
            this.mainMenu = mainMenu;
            this.mpShared = mpShared;
            this.spShared = spShared;
        }

        public IEnumerable<T> GetLocalTagsOfType<T>() where T: BaseTag
        {
            return Tags.Select(t => t.Value as T).Where(t => t != null);
        }

        public bool TryGetTag<T>(uint id, out T tag) where T: BaseTag
        {
            if(this.Tags.TryGetValue(id, out var t))
            {
                tag = (T)t;
                return true;
            }

            if (mpShared.Tags.TryGetValue(id, out t))
            {
                tag = (T)t;
                return true;
            }

            if (spShared.Tags.TryGetValue(id, out t))
            {
                tag = (T)t;
                return true;
            }

            if (mainMenu.Tags.TryGetValue(id, out t))
            {
                tag = (T)t;
                return true;
            }

            tag = null;
            return false;
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