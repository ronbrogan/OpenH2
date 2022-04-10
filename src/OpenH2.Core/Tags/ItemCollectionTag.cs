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

        [PrimitiveValue(8)]
        public int SpawnTime { get; set; }

        [FixedLength(16)]
        public class Item
        {
            [PrimitiveValue(0)]
            ///<summary>How likely this item is chosen, 100 => 100%</summary>
            public float Weight { get; set; }

            // Equip or Weap
            [PrimitiveValue(8)]
            public TagRef ItemTag { get; set; }

            [InternedString(12)]
            public string Variant { get; set; }
        }
    }
}
