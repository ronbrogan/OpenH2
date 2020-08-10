using OpenH2.Core.Representations;
using OpenH2.Core.Tags.Layout;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.matg)]
    public class GlobalsTag : BaseTag
    {
        public GlobalsTag(uint id) : base(id)
        {
        }

        [ReferenceArray(336)]
        public MaterialDefinition[] MaterialDefinitions { get; set; }

        [FixedLength(180)]
        public class MaterialDefinition
        {
            [InternedString(0)]
            public string MaterialName { get; set; }

            [InternedString(4)]
            public string BaseMaterialName1 { get; set; }

            [PrimitiveValue(8)]
            public ushort ValA { get; set; }

            [PrimitiveValue(14)]
            public ushort ValB { get; set; }

            [InternedString(16)]
            public string BaseMaterialName2 { get; set; }

            [PrimitiveValue(28)]
            public float Friction { get; set; }

            [PrimitiveValue(32)]
            public float Restitution { get; set; }

            [PrimitiveValue(36)]
            public float FloatC { get; set; }

            [PrimitiveValue(52)]
            public TagRef BsdtTag { get; set; }

            [PrimitiveValue(60)]
            public TagRef<SoundTag> SoundTagA { get; set; }

            [PrimitiveValue(68)]
            public TagRef<SoundTag> SoundTagB { get; set; }

            [PrimitiveValue(76)]
            public TagRef<SoundTag> SoundTagC { get; set; }

            [PrimitiveValue(84)]
            public TagRef<LoopingSoundTag> LoopingSoundTagA { get; set; }

            [PrimitiveValue(100)]
            public TagRef<SoundTag> SoundTagD { get; set; }

            [PrimitiveValue(116)]
            public TagRef EffectTagA { get; set; }

            [PrimitiveValue(124)]
            public TagRef EffectTagB { get; set; }

            [PrimitiveValue(132)]
            public TagRef EffectTagC { get; set; }

            [PrimitiveValue(140)]
            public TagRef EffectTagD { get; set; }

            [PrimitiveValue(148)]
            public TagRef EffectTagE { get; set; }

            [PrimitiveValue(156)]
            public TagRef EffectTagF { get; set; }

            [PrimitiveValue(176)]
            public TagRef FootTag { get; set; }
        }
    }
}
