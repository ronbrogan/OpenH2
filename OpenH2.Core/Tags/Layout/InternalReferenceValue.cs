using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Tags.Layout
{
    public class InternalReferenceValueAttribute : TagValueAttribute
    {
        public InternalReferenceValueAttribute(int offset) : base(offset)
        {
        }
    }
}
