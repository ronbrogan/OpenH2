using System;

namespace OpenH2.Core.Tags.Layout
{
    public sealed class FixedLengthAttribute : Attribute
    {
        public int Length { get; private set; }

        public FixedLengthAttribute(int length)
        {
            this.Length = length;
        }
    }
}
