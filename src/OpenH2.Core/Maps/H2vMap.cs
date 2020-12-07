using OpenBlam.Core.Extensions;
using OpenH2.Core.Enums;
using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.Core.Offsets;
using OpenH2.Core.Parsing;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Common.Models;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenH2.Core.Maps
{
    /// This class is the in-memory representation of a .map file
    public class H2vMap : H2BaseMap
    {
        private readonly H2MapReader reader;
        private readonly H2vLazyLoadingMap mainMenu;
        private readonly H2vLazyLoadingMap mpShared;
        private readonly H2vLazyLoadingMap spShared;
        private Dictionary<uint, BaseTag> Tags = new Dictionary<uint, BaseTag>();
        private IMaterialFactory materialFactory;

        public ScenarioTag Scenario { get; private set; }
        public SoundMappingTag LocalSounds { get; set; }
        public GlobalsTag Globals { get; private set; }

        internal H2vMap(H2MapReader reader, H2vLazyLoadingMap mainMenu, H2vLazyLoadingMap mpShared, H2vLazyLoadingMap spShared)
        {
            this.reader = reader;
            this.mainMenu = mainMenu;
            this.mpShared = mpShared;
            this.spShared = spShared;
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

        // TODO: consider if material construction belongs here
        internal void UseMaterialFactory(IMaterialFactory materialFactory)
        {
            this.materialFactory = materialFactory;
        }

        public Material<BitmapTag> CreateMaterial(ModelMesh mesh)
        {
            return this.materialFactory.CreateMaterial(this, mesh);
        }

        public IEnumerable<T> GetLocalTagsOfType<T>() where T : BaseTag
        {
            return Tags.Select(t => t.Value as T).Where(t => t != null);
        }

        public T GetTag<T>(uint id) where T: BaseTag
        {
            if(TryGetTag<T>(id, out var t))
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

            if (mpShared.TryGetTag(id, out t))
            {
                tag = (T)t;
                return true;
            }

            if (spShared.TryGetTag(id, out t))
            {
                tag = (T)t;
                return true;
            }

            if (mainMenu.TryGetTag(id, out t))
            {
                tag = (T)t;
                return true;
            }

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

            if(TryGetTag(tagref.Id, out tag))
            {
                return true;
            }

            Console.WriteLine("Couldn't find " + tagref);
            return false;
        }

        public SecondaryOffset GetSecondaryOffset(DataFile source, int rawOffset)
        {
            return source switch {
                DataFile.Local => new SecondaryOffset(this, rawOffset),
                DataFile.MainMenu => new SecondaryOffset(this.mainMenu, rawOffset),
                DataFile.Shared => new SecondaryOffset(this.mpShared, rawOffset),
                DataFile.SinglePlayerShared => new SecondaryOffset(this.spShared, rawOffset),
                _ => throw new NotSupportedException()
            };
        }

        public Memory<byte> ReadData(DataFile source, IOffset offset, int length)
        {
            var reader = this.reader.GetReader(source);

            var chunk = reader.Chunk(offset.Value, length);

            return chunk.ReadArray(0, length);
        }

        public static int CalculateSignature(Memory<byte> sceneData)
        {
            var sig = 0;
            var span = sceneData.Span;

            for (var i = 2048; i < sceneData.Length; i += 4)
            {
                sig ^= span.ReadInt32At(i);
            }

            return sig;
        }
    }
}