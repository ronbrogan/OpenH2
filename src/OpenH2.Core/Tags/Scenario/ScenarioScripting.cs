using OpenH2.Core.Representations;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Core.Tags.Scenario
{
    public partial class ScenarioTag
    {
        [FixedLength(36)]
        public class AiSquadGroupDefinition
        {
            [StringValue(0, 32)]
            public string Description { get; set; }

            [PrimitiveValue(32)]
            public ushort Index1 { get; set; }

            [PrimitiveValue(34)]
            public ushort Index2 { get; set; }
        }

        [FixedLength(116)]
        public class AiSquadDefinition
        {
            [StringValue(0, 32)]
            public string Description { get; set; }

            [ReferenceArray(72)]
            public StartingLocation[] StartingLocations { get; set; }

            [FixedLength(100)]
            public class StartingLocation
            {
                [InternedString(0)]
                public string Description { get; set; }

                [PrimitiveValue(4)]
                public Vector3 Position { get; set; }

                [PrimitiveValue(20)]
                public float Rotation { get; set; }

                [StringValue(64, 32)]
                public string AnotherDescription { get; set; }
            }
        }

        [FixedLength(56)]
        public class Obj360_String
        {
            [StringValue(0, 32)]
            public string Description { get; set; }
        }

        [FixedLength(40)]
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
            public ushort ScriptIndex { get; set; }

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
