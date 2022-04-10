using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.matg)]
    public class GlobalsTag : BaseTag
    {
        public GlobalsTag(uint id) : base(id)
        {
        }

        [ReferenceArray(192)]
        public SoundInfo[] SoundInfos { get; set; }

        [ReferenceArray(336)]
        public MaterialDefinition[] MaterialDefinitions { get; set; }

        [FixedLength(180)]
        public class MaterialDefinition
        {
            [InternedString(0)]
            public string Name { get; set; }

            [InternedString(4)]
            public string ParentName { get; set; }

            [PrimitiveValue(8)]
            public ushort Flags { get; set; }

            [PrimitiveValue(14)]
            public ushort OldMaterialType { get; set; }

            [InternedString(16)]
            public string GeneralArmor { get; set; }

            [InternedString(20)]
            public string SpecificArmor { get; set; }

            [PrimitiveValue(28)]
            public float Friction { get; set; }

            [PrimitiveValue(32)]
            public float Restitution { get; set; }

            [PrimitiveValue(36)]
            public float Density { get; set; }

            [PrimitiveValue(52)]
            public TagRef BsdtTag { get; set; }

            [PrimitiveValue(60)]
            public TagRef<SoundTag> SoundSweetenerSmall { get; set; }

            [PrimitiveValue(68)]
            public TagRef<SoundTag> SoundSweetenerMedium { get; set; }

            [PrimitiveValue(76)]
            public TagRef<SoundTag> SoundSweetenerLarge { get; set; }

            [PrimitiveValue(84)]
            public TagRef<LoopingSoundTag> SoundSweetenerRolling { get; set; }

            [PrimitiveValue(92)]
            public TagRef<LoopingSoundTag> SoundSweetenerGrinding { get; set; }

            [PrimitiveValue(100)]
            public TagRef<SoundTag> SoundSweetenerMelee { get; set; }

            [PrimitiveValue(116)]
            public TagRef EffectSweetenerSmall { get; set; }

            [PrimitiveValue(124)]
            public TagRef EffectSweetenerMedium { get; set; }

            [PrimitiveValue(132)]
            public TagRef EffectSweetenerlarge { get; set; }

            [PrimitiveValue(140)]
            public TagRef EffectSweetenerRolling { get; set; }

            [PrimitiveValue(148)]
            public TagRef EffectSweetenerGrinding { get; set; }

            [PrimitiveValue(156)]
            public TagRef EffectSweetenerMelee { get; set; }

            [PrimitiveValue(176)]
            public TagRef MaterialEffects { get; set; }
        }

        [FixedLength(36)]
        public class SoundInfo
        {
            [PrimitiveValue(4)]
            public TagRef LegacySoundClasses { get; set; }

            [PrimitiveValue(12)]
            public TagRef SoundEffects { get; set; }

            [PrimitiveValue(20)]
            public TagRef SoundMix { get; set; }

            [PrimitiveValue(28)]
            public TagRef CombatDialogConstants { get; set; }

            [PrimitiveValue(32)]
            public TagRef<SoundMappingTag> SoundMap { get; set; }
        }
    }
}
