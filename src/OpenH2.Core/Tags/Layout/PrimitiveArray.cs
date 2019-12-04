namespace OpenH2.Core.Tags.Layout
{
    public sealed class PrimitiveArrayAttribute : TagValueAttribute
    {
        public PrimitiveArrayAttribute(int offset, int count) : base(offset)
        {
            Count = count;
        }

        public int Count { get; }
    }
}
