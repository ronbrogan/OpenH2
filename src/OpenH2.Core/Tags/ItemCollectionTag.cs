using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;

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
