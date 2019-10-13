using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.Core.Parsing;
using OpenH2.Core.Tags;
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

        public IEnumerable<T> GetLocalTagsOfType<T>() where T : BaseTag
        {
            return Tags.Select(t => t.Value as T).Where(t => t != null);
        }

        public bool TryGetTag<T>(uint id, out T tag) where T : BaseTag
        {
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

            tag = null;
            return false;
        }

        public bool TryGetTag<T>(TagRef<T> tagref, out T tag) where T : BaseTag
        {
            return TryGetTag(tagref.Id, out tag);
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