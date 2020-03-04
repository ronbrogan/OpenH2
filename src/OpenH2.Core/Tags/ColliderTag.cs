using OpenH2.Core.Tags.Layout;
using System.Numerics;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.coll)]
    public class ColliderTag : BaseTag
    {
        public ColliderTag(uint id) : base(id)
        {
        }

        [ReferenceArray(20)]
        public uint[] Ids { get; set; }

        [ReferenceArray(28)]
        public Obj28[] Obj28s { get; set; }

        [ReferenceArray(36)]
        public Obj36[] Obj36s { get; set; }

        [ReferenceArray(44)]
        public Obj44[] Obj44s { get; set; }


        [FixedLength(12)] 
        public class Obj28 
        {
            [PrimitiveValue(0)]
            public uint Id { get; set; }
            
            [ReferenceArray(4)]
            public Obj28_4[] Obj4s { get; set; }

            [FixedLength(20)]
            public class Obj28_4
            {
                [PrimitiveValue(0)]
                public ushort ValA { get; set; }

                [PrimitiveValue(2)]
                public ushort ValB { get; set; }

                [ReferenceArray(4)]
                public Obj28_4_4[] Obj4s { get; set; }

                [PrimitiveArray(12, 2)]
                public uint[] Obj12s { get; set; }

                [FixedLength(68)]
                public class Obj28_4_4
                {
                    [ReferenceArray(4)]
                    public Obj28_4_4_4[] Obj4s { get; set; }

                    [ReferenceArray(12)]
                    public Obj28_4_4_12[] Obj12s { get; set; }

                    [ReferenceArray(20)]
                    public Obj28_4_4_20[] Obj20s { get; set; }

                    [ReferenceArray(28)]
                    public Obj28_4_4_28[] Obj28s { get; set; }

                    // skip

                    [ReferenceArray(44)]
                    public Obj28_4_4_44[] Obj44s { get; set; }

                    [ReferenceArray(52)]
                    public Obj28_4_4_52[] Obj52s { get; set; }

                    [ReferenceArray(60)]
                    public Obj28_4_4_60[] Obj60s { get; set; }


                    [FixedLength(8)]
                    public class Obj28_4_4_4
                    {
                        [PrimitiveArray(0, 4)]
                        public ushort[] Values { get; set; }
                    }

                    [FixedLength(16)]
                    public class Obj28_4_4_12
                    {
                        [PrimitiveValue(0)]
                        public uint Index { get; set; }

                        [PrimitiveValue(4)]
                        public Vector3 Value { get; set; }
                    }

                    [FixedLength(4)]
                    public class Obj28_4_4_20
                    {
                        [PrimitiveValue(0)]
                        public ushort ValA { get; set; }

                        [PrimitiveValue(2)]
                        public ushort ValB { get; set; }
                    }

                    [FixedLength(4)]
                    public class Obj28_4_4_28
                    {
                        [PrimitiveValue(0)]
                        public ushort ValA { get; set; }

                        [PrimitiveValue(2)]
                        public byte ValB { get; set; }

                        [PrimitiveValue(3)]
                        public byte ValC { get; set; }
                    }

                    [FixedLength(8)]
                    public class Obj28_4_4_44
                    {
                        [PrimitiveArray(0, 4)]
                        public ushort[] Values { get; set; }
                    }

                    [FixedLength(12)]
                    public class Obj28_4_4_52
                    {
                        [PrimitiveArray(0, 6)]
                        public ushort[] Values { get; set; }
                    }

                    [FixedLength(16)]
                    public class Obj28_4_4_60
                    {
                        [PrimitiveValue(0)]
                        public Vector3 Value { get; set; }

                        [PrimitiveValue(12)]
                        public ushort Index { get; set; }

                        [PrimitiveValue(14)]
                        public ushort Unknown { get; set; }
                    }
                }

            }
        }
        [FixedLength(4)] public class Obj36 { }
        [FixedLength(4)] public class Obj44 { }
    }
}