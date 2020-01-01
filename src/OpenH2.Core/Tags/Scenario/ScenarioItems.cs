using OpenH2.Core.Representations;
using OpenH2.Core.Tags.Layout;
using System.Numerics;

namespace OpenH2.Core.Tags.Scenario
{
    public partial class ScenarioTag
    {
        [FixedLength(40)]
        public class VehicleDefinition
        {
            [PrimitiveValue(4)]
            public TagRef<VehicleTag> Vehicle { get; set; }
        }

        [FixedLength(40)]
        public class EquipmentDefinition
        {
            [PrimitiveValue(4)]
            public TagRef<EquipmentTag> Equipment { get; set; }
        }

        // TODO: placement test
        [FixedLength(84)]
        public class WeaponPlacement
        {
            [PrimitiveValue(0)]
            public ushort Index { get; set; }

            [PrimitiveValue(8)]
            public Vector3 Position { get; set; }

            [PrimitiveValue(20)]
            public Vector3 Orientation { get; set; }
        }

        [FixedLength(40)]
        public class WeaponDefinition
        {
            [PrimitiveValue(4)]
            public TagRef<WeaponTag> WeaponId { get; set; }
        }

        [FixedLength(144)]
        public class ItemCollectionPlacement
        {
            [PrimitiveValue(64)]
            public Vector3 Position { get; set; }

            [PrimitiveValue(76)]
            public Vector3 Orientation { get; set; }

            [PrimitiveValue(92)]
            public TagRef ItemCollectionReference { get; set; }
        }

        [FixedLength(76)]
        public class BlocInstance
        {
            [PrimitiveValue(0)]
            public ushort BlocDefinitionIndex { get; set; }

            [PrimitiveValue(8)]
            public Vector3 Position { get; set; }

            [PrimitiveValue(20)]
            public Vector3 Orientation { get; set; }

        }

        [FixedLength(40)]
        public class BlocDefinition
        {
            [PrimitiveValue(4)]
            public TagRef<BlocTag> Bloc { get; set; }
        }
    }
}
