using OpenH2.Core.GameObjects;
using OpenH2.Core.Representations;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;
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

        [FixedLength(24)]
        public class OriginatingData
        {
            [ReferenceArray(0)]
            public WildcardTagReference[] Entities { get; set; }

            [ReferenceArray(8)]
            public WildcardTagReference[] Scripts { get; set; }

            [ReferenceArray(16)]
            public WildcardTagReference[] Ai { get; set; }

            [FixedLength(8)]
            public class WildcardTagReference
            {
                [StringValue(0, 4)]
                public string TagType { get; set; }

                [PrimitiveValue(4)]
                public TagRef Tag { get; set; }
            }
        }
    }
}
