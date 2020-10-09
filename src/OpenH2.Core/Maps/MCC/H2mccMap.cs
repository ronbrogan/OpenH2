using OpenH2.Core.Extensions;
using OpenH2.Core.Parsing;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenH2.Core.Maps.MCC
{
    public class H2mccMap : H2BaseMap
    {
        private Dictionary<uint, BaseTag> Tags = new Dictionary<uint, BaseTag>();
        private readonly H2MapReader reader;
        public ScenarioTag Scenario { get; internal set; }
        public SoundMappingTag LocalSounds { get; internal set; }
        public GlobalsTag Globals { get; internal set; }

        public H2mccMap(H2MapReader reader)
        {
            this.reader = reader;
        }

        internal void SetTags(Dictionary<uint, BaseTag> tags)
        {
            this.Tags = tags;

            if (this.Tags.TryGetValue(this.IndexHeader.Scenario.Id, out var scnr))
            {
                this.Scenario = (ScenarioTag)scnr;
            }

            if (this.Tags.TryGetValue(this.Header.LocalSounds.Id, out var ugh))
            {
                this.LocalSounds = (SoundMappingTag)ugh;
            }

            // Globals tag isn't always local
            if (this.TryGetTag(this.IndexHeader.Globals, out var globals))
            {
                this.Globals = globals;
            }
        }

        public IEnumerable<T> GetLocalTagsOfType<T>() where T : BaseTag
        {
            return Tags.Select(t => t.Value as T).Where(t => t != null);
        }

        public T GetTag<T>(uint id) where T : BaseTag
        {
            if (TryGetTag<T>(id, out var t))
            {
                return t;
            }

            throw new Exception($"Unable to find tag {id}");
        }

        public T GetTag<T>(TagRef<T> tagref) where T : BaseTag
        {
            if (TryGetTag(tagref, out var t))
            {
                return t;
            }

            throw new Exception($"Unable to find tag {tagref.Id}");
        }

        public bool TryGetTag<T>(uint id, out T tag) where T : BaseTag
        {
            if (id == uint.MaxValue)
            {
                tag = null;
                return false;
            }

            if (this.Tags.TryGetValue(id, out var t))
            {
                tag = (T)t;
                return true;
            }

            //if (mpShared.TryGetTag(id, out t))
            //{
            //    tag = (T)t;
            //    return true;
            //}

            //if (spShared.TryGetTag(id, out t))
            //{
            //    tag = (T)t;
            //    return true;
            //}

            //if (mainMenu.TryGetTag(id, out t))
            //{
            //    tag = (T)t;
            //    return true;
            //}

            Console.WriteLine($"TryGetTag miss [{id}]");

            tag = null;
            return false;
        }

        public bool TryGetTag<T>(TagRef<T> tagref, out T tag) where T : BaseTag
        {
            if (tagref.IsInvalid)
            {
                tag = null;
                return false;
            }

            if (TryGetTag(tagref.Id, out tag))
            {
                return true;
            }

            Console.WriteLine("Couldn't find " + tagref);
            return false;
        }

        public static int CalculateSignature(Memory<byte> sceneData)
        {
            var sig = 0;
            var span = sceneData.Span;

            for (var i = BlamSerializer.SizeOf<H2mccMapHeader>(); i < sceneData.Length; i += 4)
            {
                sig ^= span.ReadInt32At(i);
            }

            return sig;
        }
    }
}
