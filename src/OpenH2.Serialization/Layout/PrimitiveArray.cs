namespace OpenH2.Serialization.Layout
{
    public sealed class PrimitiveArrayAttribute : SerializableMemberAttribute
    {
        public PrimitiveArrayAttribute(int offset, int count) : base(offset)
        {
            Count = count;
        }

        public int Count { get; }
    }
}
