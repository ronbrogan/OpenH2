using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Tags.Layout
{
    public class TagLabelAttribute : Attribute
    {
        public string Label { get; set; }

        public TagLabelAttribute(string label)
        {
            this.Label = label;
        }
    }
}
