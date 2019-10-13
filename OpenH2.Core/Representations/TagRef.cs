using OpenH2.Core.Tags;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Representations
{
    public readonly struct TagRef
    {
        public TagRef(uint id)
        {
            this.Id = id;
        }

        public uint Id { get; }

        public static implicit operator uint(TagRef tagref)
        {
            return tagref.Id;
        }
    }

    public readonly struct TagRef<TTag> where TTag : BaseTag
    {
        public TagRef(uint id)
        {
            this.Id = id;
        }

        public uint Id { get; }

        public static implicit operator uint(TagRef<TTag> tagref)
        {
            return tagref.Id;
        }

        public static implicit operator TagRef(TagRef<TTag> tagref)
        {
            return new TagRef(tagref.Id);
        }

        public static implicit operator TagRef<TTag>(TagRef tagref)
        {
            return new TagRef<TTag>(tagref.Id);
        }
    }
}
