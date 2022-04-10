﻿using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.@char)]
    public class CharacterTag : BaseTag
    {
        public CharacterTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(8)]
        public TagRef<CharacterTag> Parent { get; set; }

        [PrimitiveValue(16)]
        public TagRef Unit { get; set; }

        [PrimitiveValue(24)]
        public TagRef Creature { get; set; }

        [PrimitiveValue(32)]
        public TagRef Style { get; set; }

        [PrimitiveValue(40)]
        public TagRef<CharacterTag> Major { get; set; }
    }
}
