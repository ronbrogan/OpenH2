using OpenH2.Core.GameObjects;
using OpenH2.Core.Maps;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;
using System.Numerics;

namespace OpenH2.Core.Tags.Scenario
{
    public partial class ScenarioTag
    {
        [FixedLength(40)]
        public class BipedDefinition
        {
            [PrimitiveValue(4)]
            public TagRef<BipedTag> Biped { get; set; }
        }

        [FixedLength(68)]
        public class StartingProfileDefinition : IGameObjectDefinition<IStartingProfile>
        {
            [StringValue(0, 40)]
            public string Description { get; set; }

            [PrimitiveValue(44)]
            public TagRef<WeaponTag> PrimaryWeapon { get; set; }

            [PrimitiveValue(48)]
            public ushort PrimaryWeaponLoadedAmmo { get; set; }

            [PrimitiveValue(50)]
            public ushort PrimaryWeaponTotalAmmo { get; set; }

            [PrimitiveValue(56)]
            public TagRef<WeaponTag> SecondaryWeapon { get; set; }

            [PrimitiveValue(60)]
            public ushort SecondaryWeaponLoadedAmmo { get; set; }

            [PrimitiveValue(62)]
            public ushort SecondaryWeaponTotalAmmo { get; set; }

            [PrimitiveValue(64)]
            public ushort PrimaryGrenadeCount { get; set; }

            [PrimitiveValue(66)]
            public ushort SecondaryGrenadeCount { get; set; }

            public IStartingProfile? GameObject { get; set; }
        }

        [FixedLength(52)]
        public class PlayerSpawnMarker
        {
            [PrimitiveValue(0)]
            public Vector3 Position { get; set; }

            [PrimitiveValue(12)]
            public float Heading { get; set; }

            [PrimitiveValue(20)]
            public int Flags { get; set; }
        }
    }
}
