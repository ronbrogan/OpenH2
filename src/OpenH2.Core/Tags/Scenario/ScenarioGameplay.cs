using OpenH2.Core.Representations;
using OpenH2.Core.Tags.Layout;
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
        public class LoadoutDefinition
        {
            [StringValue(0, 40)]
            public string Description { get; set; }

            [PrimitiveValue(44)]
            public TagRef<WeaponTag> PrimaryWeapon { get; set; }

            [PrimitiveValue(48)]
            public ushort PrimaryWeaponLoadedAmmo { get; set; }

            [PrimitiveValue(50)]
            public ushort PrimaryWeaponReserveAmmo { get; set; }

            [PrimitiveValue(56)]
            public TagRef<WeaponTag> SecondaryWeapon { get; set; }

            [PrimitiveValue(60)]
            public ushort SecondaryWeaponLoadedAmmo { get; set; }

            [PrimitiveValue(62)]
            public ushort SecondaryWeaponReserveAmmo { get; set; }

            [PrimitiveValue(64)]
            public ushort PrimaryGrenadeCount { get; set; }

            [PrimitiveValue(66)]
            public ushort SecondaryGrenadeCount { get; set; }
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
