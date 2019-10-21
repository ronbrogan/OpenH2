using OpenH2.Core.Factories;
using OpenH2.Core.Parsing;
using OpenH2.Core.Tags;
using System.Collections.Generic;

namespace OpenH2.Core.Representations
{
    public class H2vLazyLoadingMap : H2vBaseMap
    {
        private H2vReader reader;
        private Dictionary<uint, BaseTag> Tags = new Dictionary<uint, BaseTag>();

        internal H2vLazyLoadingMap(H2vReader reader)
        {
            this.reader = reader;
        }

        public bool TryGetTag<T>(uint id, out T tag) where T : BaseTag
        {
            if(Tags.TryGetValue(id, out var baseTag))
            {
                tag = (T)baseTag;
                return true;
            }

            TagIndexEntry entry = null;

            foreach (var e in this.TagIndex)
            {
                if (e.ID == id)
                {
                    entry = e;
                    break;
                }
            }

            if (entry != null)
            {
                tag = MapFactory.GetTag(this, entry, this.reader) as T;
                Tags[id] = tag;
                return true;
            }

            tag = null;
            return false;
        }
    }
}
