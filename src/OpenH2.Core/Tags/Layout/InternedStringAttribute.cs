using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Tags.Layout
{
    public class InternedStringAttribute : TagValueAttribute
    {
        public InternedStringAttribute(int offset) : base(offset)
        {
        }
    }
}
