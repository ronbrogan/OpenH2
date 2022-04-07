﻿using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.vehc)]
    public class VehicleCollectionTag : BaseTag
    {
        public VehicleCollectionTag(uint id) : base(id)
        {
        }


        [ReferenceArray(0)]
        public VehicleReference[] VehicleReferences { get; set; }

        [FixedLength(20)]
        public class VehicleReference
        {
            [PrimitiveValue(0)]
            public float Param { get; set; }

            [PrimitiveValue(8)]
            public TagRef<VehicleTag> Vehicle { get; set; }
        }
    }
}
