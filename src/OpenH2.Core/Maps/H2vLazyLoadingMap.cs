using OpenH2.Core.Factories;
using OpenH2.Core.Parsing;
using OpenH2.Core.Tags;
using System.Collections.Generic;

namespace OpenH2.Core.Maps
{
    public class H2vLazyLoadingMap : H2BaseMap
    {
        private H2MapReader reader;
        private Dictionary<uint, BaseTag> Tags = new Dictionary<uint, BaseTag>();

        internal H2vLazyLoadingMap(H2MapReader reader)
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

            if(this.TagIndex.TryGetValue(id, out var entry))
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
