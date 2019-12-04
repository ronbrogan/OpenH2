using OpenH2.Core.Tags;

namespace OpenH2.Core.Representations
{
    public interface ITagRef
    {
        uint Id { get; }

        bool IsInvalid { get; }
    }

    public readonly struct TagRef : ITagRef
    {
        public TagRef(uint id)
        {
            this.Id = id;
        }

        public uint Id { get; }
        public bool IsInvalid => this.Id == uint.MaxValue;

        public static implicit operator uint(TagRef tagref)
        {
            return tagref.Id;
        }
    }

    public readonly struct TagRef<TTag> : ITagRef where TTag : BaseTag 
    {
        public TagRef(uint id)
        {
            this.Id = id;
        }

        public uint Id { get; }
        public bool IsInvalid => this.Id == uint.MaxValue;

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



        public override string ToString()
        {
            return string.Format("{0}[{1}]", typeof(TTag).Name, Id);
        }
    }
}
