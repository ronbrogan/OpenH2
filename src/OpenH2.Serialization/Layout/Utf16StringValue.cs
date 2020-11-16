namespace OpenH2.Serialization.Layout
{

    public sealed class Utf16StringValueAttribute : SerializableMemberAttribute
    {
        /// <param name="maxLength">Max length in characters, each character is 2 bytes</param>
        public Utf16StringValueAttribute(int offset, int maxLength) : base(offset)
        {
            MaxLength = maxLength;
        }

        public int MaxLength { get; }
    }
}
