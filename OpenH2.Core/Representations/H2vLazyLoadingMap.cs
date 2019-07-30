using OpenH2.Core.Factories;
using OpenH2.Core.Parsing;
using OpenH2.Core.Tags;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Representations
{
    public class H2vLazyLoadingMap : H2vBaseMap
    {
        private H2vReader reader;

        internal H2vLazyLoadingMap(H2vReader reader)
        {
            this.reader = reader;
        }

        public bool TryGetTag<T>(uint id, out T tag) where T : BaseTag
        {
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
                return true;
            }

            tag = null;
            return false;
        }
    }
}
