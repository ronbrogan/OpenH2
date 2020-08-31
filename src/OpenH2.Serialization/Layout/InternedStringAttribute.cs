using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Serialization.Layout
{
    public class InternedStringAttribute : SerializableMemberAttribute
    {
        public InternedStringAttribute(int offset) : base(offset)
        {
        }
    }
}
