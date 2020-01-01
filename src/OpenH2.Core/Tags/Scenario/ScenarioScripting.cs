using OpenH2.Core.Tags.Layout;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Tags.Scenario
{
    public partial class ScenarioTag
    {
        [FixedLength(36)]
        public class Obj344_String
        {
            [StringValue(0, 32)]
            public string Description { get; set; }

            [PrimitiveValue(32)]
            public ushort Index1 { get; set; }

            [PrimitiveValue(34)]
            public ushort Index2 { get; set; }
        }

        [FixedLength(116)]
        public class Obj352_String
        {
            [StringValue(0, 32)]
            public string Description { get; set; }
        }

        [FixedLength(56)]
        public class Obj360_String
        {
            [StringValue(0, 32)]
            public string Description { get; set; }
        }

        [FixedLength(40)]
        public class Obj440_ScriptMethod
        {
            [StringValue(0, 32)]
            public string Description { get; set; }

            [PrimitiveValue(32)]
            public ushort Index1 { get; set; }

            [PrimitiveValue(34)]
            public ushort Index2 { get; set; }

            [PrimitiveValue(36)]
            public ushort ValueA { get; set; }

            [PrimitiveValue(38)]
            public ushort ValueB { get; set; }
        }

        [FixedLength(20)]
        public class Obj568_ScriptASTNode
        {
            [PrimitiveValue(0)]
            public ushort ValueA { get; set; }

            [PrimitiveValue(2)]
            public ushort ValueB { get; set; }

            [PrimitiveValue(4)]
            public ushort ValueC { get; set; }

            [PrimitiveValue(6)]
            public ushort ValueD { get; set; }

            [PrimitiveValue(8)]
            public ushort ValueE { get; set; }

            [PrimitiveValue(10)]
            public ushort ValueF { get; set; }

            [PrimitiveValue(12)]
            public ushort ValueG { get; set; }

            [PrimitiveValue(14)]
            public ushort ValueH { get; set; }

            [PrimitiveValue(16)]
            public ushort ValueI { get; set; }

            [PrimitiveValue(18)]
            public ushort ValueJ { get; set; }
        }
    }
}
