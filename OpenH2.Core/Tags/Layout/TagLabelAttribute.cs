using System;

namespace OpenH2.Core.Tags.Layout
{
    public class TagLabelAttribute : Attribute
    {
        public TagName Label { get; set; }

        public TagLabelAttribute(TagName label)
        {
            this.Label = label;
        }
    }
}
