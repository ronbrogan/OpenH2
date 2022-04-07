﻿using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;
using OpenBlam.Core.MapLoading;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.scen)]
    public class SceneryTag : BaseTag
    {
        public override string Name { get; set; }

        public SceneryTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(0)]
        public int Type { get; set; }

        [PrimitiveValue(4)]
        public float UniformScale { get; set; }

        [PrimitiveValue(16)]
        public float Unknown { get; set; }

        [PrimitiveValue(56)]
        public TagRef<HaloModelTag> Model { get; set; }

        [PrimitiveValue(64)]
        public TagRef<BlocTag> Bloc { get; set; }

        [PrimitiveValue(80)]
        public uint EffectId { get; set; }

        [PrimitiveValue(88)]
        public uint FootId { get; set; }

        [PrimitiveArray(120, 6)]
        public float[] Params { get; set; }

        //[InternalReferenceValue(12)]
        //public ShaderInfo[] Shaders { get; set; }

        public override void PopulateExternalData(MapStream sceneReader)
        {
        }
    }
}
