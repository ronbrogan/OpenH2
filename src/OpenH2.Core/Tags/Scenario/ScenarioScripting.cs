using OpenH2.Core.GameObjects;
using OpenH2.Core.Scripting;
using OpenBlam.Serialization.Layout;
using System.Diagnostics;
using System.Numerics;

namespace OpenH2.Core.Tags.Scenario
{
    public partial class ScenarioTag
    {
        [FixedLength(36)]
        [DebuggerDisplay("{Description}")]
        public class AiSquadGroupDefinition : IAiActorDefinition
        {
            [StringValue(0, 32)]
            public string Description { get; set; }

            [PrimitiveValue(32)]
            public ushort ParentGroupIndex { get; set; }

            [PrimitiveValue(34)]
            public ushort Index2 { get; set; }

            public IAiActor Actor { get; set; }
        }

        [FixedLength(116)]
        [DebuggerDisplay("{Description}")]
        public class AiSquadDefinition : IAiActorDefinition
        {
            [StringValue(0, 32)]
            public string Description { get; set; }

            [PrimitiveValue(32)]
            public ushort ValueA { get; set; }

            [PrimitiveValue(34)]
            public ushort ValueB { get; set; }

            // (Almost?) always 0 
            [PrimitiveValue(36)]
            public ushort ValueC { get; set; }

            [PrimitiveValue(38)]
            public ushort SquadGroupIndex { get; set; }

            [PrimitiveValue(40)]
            public ushort ValueC2 { get; set; }

            [PrimitiveValue(42)]
            public ushort ValueC3 { get; set; }

            [PrimitiveValue(44)]
            public ushort SpawnMin { get; set; }

            [PrimitiveValue(46)]
            public ushort SpawnMax { get; set; }

            [PrimitiveValue(48)]
            public ushort ValueD3{ get; set; }

            [PrimitiveValue(50)]
            public ushort ValueD4 { get; set; }

            [PrimitiveValue(52)]
            public ushort VehicleIndex { get; set; }

            [PrimitiveValue(54)]
            public ushort CharacterIndex { get; set; }

            [PrimitiveValue(56)]
            public ushort ValueF { get; set; }

            [PrimitiveValue(58)]
            public ushort ValueG { get; set; }

            [PrimitiveValue(60)]
            public ushort WeaponIndex { get; set; }

            [PrimitiveValue(62)]
            public ushort ValueI { get; set; }

            [PrimitiveValue(64)]
            public ushort ValueJ { get; set; }

            [PrimitiveValue(66)]
            public ushort AiOrderIndex { get; set; }

            [InternedString(68)]
            public string Variant { get; set; }

            [ReferenceArray(72)]
            public StartingLocation[] StartingLocations { get; set; }

            public IAiActor Actor { get; set; }

            [FixedLength(100)]
            [DebuggerDisplay("{Description}")]
            public class StartingLocation : IAiActorDefinition
            {
                [InternedString(0)]
                public string Description { get; set; }

                [PrimitiveValue(4)]
                public Vector3 Position { get; set; }

                // (Almost?) always 0 or ushort.MaxValue
                [PrimitiveValue(16)]
                public ushort ZeroOrMax { get; set; }

                // (Almost?) always 0 
                [PrimitiveValue(18)]
                public ushort Zero { get; set; }

                [PrimitiveValue(20)]
                public float Rotation { get; set; }

                [PrimitiveValue(24)]
                public float Float1 { get; set; }

                [PrimitiveValue(28)]
                public ushort Flags { get; set; }

                [PrimitiveValue(30)]
                public ushort Zero2 { get; set; }

                [PrimitiveValue(32)]
                public ushort CharacterIndex { get; set; }

                [PrimitiveValue(34)]
                public ushort WeaponIndex { get; set; }

                [PrimitiveValue(36)]
                public ushort Index6 { get; set; }

                [PrimitiveValue(38)]
                public ushort Zero3 { get; set; }

                [PrimitiveValue(40)]
                public ushort Index8 { get; set; }

                [PrimitiveValue(42)]
                public ushort State { get; set; }

                [PrimitiveValue(44)]
                public ushort Zero4Sometimes { get; set; }

                [PrimitiveValue(46)]
                public ushort Zero5Sometimes { get; set; }

                [InternedString(48)]
                public string Variant { get; set; }

                [PrimitiveValue(52)]
                public ushort Index14 { get; set; }

                [PrimitiveValue(54)]
                public ushort Index15 { get; set; }

                [PrimitiveValue(56)]
                public float Float2 { get; set; }

                [PrimitiveValue(60)]
                public ushort MaxValue { get; set; }

                [PrimitiveValue(62)]
                public ushort Zero7Sometimes { get; set; }

                [StringValue(64, 32)]
                public string StartupScript { get; set; }

                [PrimitiveValue(96)]
                public uint ScriptIndex { get; set; }

                public int SquadIndex { get; set; }

                public IAiActor Actor { get; set; }
            }
        }

        [FixedLength(56)]
        [DebuggerDisplay("{Description}")]
        public class Obj360_String
        {
            [StringValue(0, 32)]
            public string Description { get; set; }

            [PrimitiveValue(32)]
            public uint ValueA { get; set; }

            [PrimitiveValue(36)]
            public uint ValueB { get; set; }

            [ReferenceArray(40)]
            public Obj40[] Obj40s { get; set; }

            [ReferenceArray(48)]
            public Obj48[] Obj48s { get; set; }

            [FixedLength(32)]
            public class Obj40
            {
                [PrimitiveValue(0)]
                public Vector3 FloatsA { get; set; }

                [PrimitiveValue(12)]
                public ushort ValueA { get; set; }

                [PrimitiveValue(14)]
                public ushort ValueB { get; set; }

                [PrimitiveValue(16)]
                public ushort ValueC { get; set; }

                [PrimitiveValue(18)]
                public ushort ValueD { get; set; }

                [PrimitiveValue(20)]
                public ushort ValueE { get; set; }

                [PrimitiveValue(22)]
                public ushort ValueF { get; set; }

                [PrimitiveValue(24)]
                public Vector2 FloatsB { get; set; }
            }

            [FixedLength(136)]
            [DebuggerDisplay("{Description}")]
            public class Obj48
            {
                [StringValue(0, 32)]
                public string Description { get; set; }

                [PrimitiveValue(36)]
                public Vector3 Position { get; set; }
            }
        }

        [FixedLength(40)]
        [DebuggerDisplay("{Description}")]
        public class ScriptVariableDefinition
        {
            [StringValue(0, 32)]
            public string Description { get; set; }

            [PrimitiveValue(32)]
            public ScriptDataType DataType { get; set; }

            [PrimitiveValue(36)]
            public uint Value_32 { get; set; }

            public ushort Value_H16 => (ushort)(Value_32);

            public ushort Value_L16 => (ushort)(Value_32 >> 16);

            public byte Value_B0 => (byte)(Value_32 >> 24);
            public byte Value_B1 => (byte)(Value_32 >> 16);
            public byte Value_B2 => (byte)(Value_32 >> 8);
            public byte Value_B3 => (byte)(Value_32);
        }

        [FixedLength(40)]
        [DebuggerDisplay("{Description}")]
        public class ScriptMethodDefinition
        {
            [StringValue(0, 32)]
            public string Description { get; set; }

            [PrimitiveValue(32)]
            public Lifecycle Lifecycle { get; set; }

            [PrimitiveValue(34)]
            public ScriptDataType ReturnType { get; set; }

            [PrimitiveValue(36)]
            public ushort SyntaxNodeIndex { get; set; }

            [PrimitiveValue(38)]
            public ushort ValueB { get; set; }
        }

        [FixedLength(20)]
        public class ScriptSyntaxNode
        {
            [PrimitiveValue(0)]
            public ushort Checkval { get; set; }

            [PrimitiveValue(2)]
            public ushort OperationId { get; set; }

            [PrimitiveValue(4)]
            public ScriptDataType DataType { get; set; }

            [PrimitiveValue(6)]
            public NodeType NodeType { get; set; }

            [PrimitiveValue(8)]
            public ushort NextIndex { get; set; }

            [PrimitiveValue(10)]
            public ushort NextCheckval { get; set; }

            [PrimitiveValue(12)]
            public ushort NodeString { get; set; }

            [PrimitiveValue(14)]
            public ushort ValueH { get; set; }

            [PrimitiveValue(16)]
            public uint NodeData_32 { get; set; }

            public ushort NodeData_H16 => (ushort)(NodeData_32);

            public ushort NodeData_L16 => (ushort)(NodeData_32 >> 16);

            public byte NodeData_B0 => (byte)(NodeData_32 >> 24);
            public byte NodeData_B1 => (byte)(NodeData_32 >> 16);
            public byte NodeData_B2 => (byte)(NodeData_32 >> 8);
            public byte NodeData_B3 => (byte)(NodeData_32);
        }
    }
}
