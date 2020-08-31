using System;

namespace OpenH2.Serialization.Layout
{
    /// <summary>
    /// Designates a type as serializable, with a specified size
    /// </summary>
    public sealed class FixedLengthAttribute : SerializableTypeAttribute
    {
        public int Length { get; private set; }

        public FixedLengthAttribute(int length)
        {
            this.Length = length;
        }
    }
}
