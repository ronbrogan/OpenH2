namespace OpenH2.Serialization.Layout
{

    public sealed class StringValueAttribute : SerializableMemberAttribute
    {
        public StringValueAttribute(int offset, int maxLength) : base(offset)
        {
            MaxLength = maxLength;
        }

        public int MaxLength { get; }
    }
}
