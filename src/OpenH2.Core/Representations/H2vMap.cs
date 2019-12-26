using OpenH2.Core.Enums;
using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.Core.Offsets;
using OpenH2.Core.Parsing;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Common;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenH2.Core.Representations
{
    /// This class is the in-memory representation of a .map file
    public class H2vMap : H2vBaseMap
    {
        private readonly H2vReader reader;
        private readonly H2vLazyLoadingMap mainMenu;
        private readonly H2vLazyLoadingMap mpShared;
        private readonly H2vLazyLoadingMap spShared;
        private Dictionary<uint, BaseTag> Tags = new Dictionary<uint, BaseTag>();
        private MaterialFactory materialFactory;

        internal H2vMap(H2vReader reader, H2vLazyLoadingMap mainMenu, H2vLazyLoadingMap mpShared, H2vLazyLoadingMap spShared)
        {
            this.reader = reader;
            this.mainMenu = mainMenu;
            this.mpShared = mpShared;
            this.spShared = spShared;
        }

        internal void SetTags(Dictionary<uint, BaseTag> tags)
        {
            this.Tags = tags;
        }

        // TODO: consider if material construction belongs here
        internal void UseMaterialFactory(MaterialFactory materialFactory)
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

            Console.WriteLine("TryGetTag miss");

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