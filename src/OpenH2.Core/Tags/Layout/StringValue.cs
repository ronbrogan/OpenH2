namespace OpenH2.Core.Tags.Layout
{

    public sealed class StringValueAttribute : TagValueAttribute
    {
        public StringValueAttribute(int offset, int maxLength) : base(offset)
        {
            MaxLength = maxLength;
        }

        public int MaxLength { get; }
    }
}
