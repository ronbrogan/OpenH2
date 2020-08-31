using OpenH2.Core.Representations;
using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.itmc)]
    public class ItemCollectionTag : BaseTag
    {
        public ItemCollectionTag(uint id) : base(id)
        {
        }

        [ReferenceArray(0)]
        public Item[] Items { get; set; }

        [FixedLength(12)]
        public class Item
        {
            [PrimitiveValue(0)]
            public float Param { get; set; }

            // Equip or Weap
            [PrimitiveValue(8)]
            public TagRef ItemTag { get; set; }
        }
    }
}
