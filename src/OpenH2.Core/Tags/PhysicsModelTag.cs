using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;
using System.Numerics;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.phmo)]
    public class PhysicsModelTag : BaseTag
    {
        public override string Name { get; set; }

        public PhysicsModelTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(4)]
        public Vector3 Params { get; set; }

        [ReferenceArray(40)]
        public MoverParameter[] MoverParameters { get; set; }

        [ReferenceArray(48)]
        public Obj48[] Obj48s { get; set; }

        [ReferenceArray(56)]
        public BodyParameterSet[] BodyParameters { get; set; }

        [ReferenceArray(64)]
        public MaterialReference[] MaterialReferences { get; set; }

        [ReferenceArray(88)]
        public CapsuleDefinition[] CapsuleDefinitions { get; set; }

        [ReferenceArray(96)]
        public BoxDefinition[] BoxDefinitions { get; set; }

        [ReferenceArray(112)]
        public MeshDefinition[] MeshDefinitions { get; set; }

        [ReferenceArray(120)]
        public Obj120[] Obj120s { get; set; }

        [ReferenceArray(128)]
        // These seem like planes, first 3 floats appear to be unit vectors
        public ColliderPlane[] ColliderPlanes { get; set; }

        [ReferenceArray(144)]
        public Obj144[] Obj144s { get; set; }

        [ReferenceArray(152)]
        public Obj152[] Obj152s { get; set; }

        [ReferenceArray(160)]
        public Obj160[] Obj160s { get; set; }

        [ReferenceArray(168)]
        public byte[] RawData1 { get; set; }

        [ReferenceArray(184)]
        public RagdollComponent[] RagdollComponents { get; set; }

        [ReferenceArray(192)]
        public Variant[] Variants { get; set; }

        [ReferenceArray(200)]
        public Obj200[] Obj200s { get; set; }

        [ReferenceArray(232)]
        public Obj232[] Obj232s { get; set; }


        [FixedLength(112)]
        public class MoverParameter
        {
            [PrimitiveValue(4)]
            public ushort Flags { get; set; }

            [InternedString(8)]
            public string Description { get; set; }

            [PrimitiveValue(40)]
            public float ValueA { get; set; }

            [PrimitiveValue(44)]
            public float ValueB { get; set; }

            [PrimitiveValue(48)]
            public float DeltaVelocity { get; set; }

            [PrimitiveValue(52)]
            public float MaxVelocity { get; set; }
        }


        [FixedLength(24)]
        public class Obj48
        {
            [PrimitiveValue(0)]
            public ushort ValA { get; set; }

            [PrimitiveValue(2)]
            public ushort ValB { get; set; }

            [PrimitiveValue(4)]
            public ushort ValC { get; set; }

            [PrimitiveValue(6)]
            public ushort ValD { get; set; }

            [ReferenceArray(8)]
            public Obj48_8[] Obj8s { get; set; }

            [InternedString(16)]
            public string MaterialNameA { get; set; }

            [InternedString(20)]
            public string MaterialNameB { get; set; }

            [FixedLength(12)]
            public class Obj48_8
            {
                [PrimitiveValue(0)]
                public ushort ValA { get; set; }

                [PrimitiveValue(2)]
                public ushort ValB { get; set; }

                // Only zeroes observed @4

                [PrimitiveValue(8)]
                public float FloatA { get; set; }
            }
        }

        [FixedLength(144)]
        public class BodyParameterSet
        {
            [PrimitiveValue(0)]
            public float UnknownShort1 { get; set; }

            [PrimitiveValue(2)]
            public float UnknownShort2 { get; set; }

            [PrimitiveValue(8)]
            public float UnknownFloat0 { get; set; }

            [PrimitiveValue(16)]
            public float UnknownFloat1 { get; set; }

            [PrimitiveValue(20)]
            public float UnknownFloat2 { get; set; }

            [PrimitiveValue(32)]
            public float UnknownFloat3 { get; set; }

            [PrimitiveValue(40)]
            public float UnknownFloat4 { get; set; }

            [PrimitiveValue(56)]
            public float UnknownInt { get; set; }

            [PrimitiveValue(60)]
            public float Mass { get; set; }

            [PrimitiveValue(64)]
            public Vector3 CenterOfMass { get; set; }

            [PrimitiveValue(80)]
            public Matrix4x4 InertiaTensor { get; set; }

        }

        [FixedLength(12)]
        public class MaterialReference
        {
            [InternedString(0)]
            public string MaterialNameA { get; set; }

            [InternedString(4)]
            public string MaterialNameB { get; set; }

            [PrimitiveValue(12)]
            public float FloatA { get; set; }
        }

        
        [FixedLength(80)]
        public class CapsuleDefinition
        {
            [InternedString(0)]
            // palm trees are named palm_N_pill - capsule defs?
            public string ObjName { get; set; } 

            [PrimitiveValue(4)]
            public ushort ValA { get; set; }

            [PrimitiveValue(6)]
            public ushort MaterialIndexMaybe { get; set; }

            [PrimitiveValue(8)]
            public Vector3 Params { get; set; }

            [PrimitiveArray(20, 2)]
            public float[] FloatsA { get; set; }

            [PrimitiveValue(28)]
            public ushort ValC { get; set; }

            [PrimitiveValue(30)]
            public ushort ValD { get; set; }

            // Ends in mat3x3?
        }

        [FixedLength(144)]
        public class BoxDefinition
        {
            [InternedString(0)]
            // flywheel has physics_box here - cuboid collider definition?
            public string ObjName { get; set; }

            [PrimitiveValue(4)]
            public ushort ValA { get; set; }

            [PrimitiveValue(6)]
            public ushort MaterialIndexMaybe { get; set; }

            [PrimitiveValue(8)]
            public Vector3 Params { get; set; }

            [PrimitiveValue(20)]
            public float FloatA { get; set; }

            [PrimitiveValue(24)]
            public float Mass { get; set; }

            [PrimitiveValue(28)]
            public ushort ValC { get; set; }

            [PrimitiveValue(30)]
            public ushort ValD { get; set; }

            [PrimitiveValue(32)]
            public uint ValE { get; set; }

            [PrimitiveValue(36)]
            public uint ValF { get; set; }

            [PrimitiveValue(40)]
            public uint ValG { get; set; }

            [PrimitiveValue(44)]
            public float FloatB { get; set; }

            [PrimitiveValue(48)]
            public float FloatC { get; set; }

            [PrimitiveValue(52)]
            public Vector3 HalfWidthsMaybe { get; set; }

            // other stuff, some floats, zeroes

            [PrimitiveValue(80)]
            public Matrix4x4 Transform { get; set; }
        }

        [FixedLength(256)]
        public class MeshDefinition
        {
            [InternedString(0)]
            // flywheel has a physics_mesh name - convex mesh def?
            public string ObjName { get; set; }

            [PrimitiveValue(4)]
            public ushort ValA { get; set; }

            [PrimitiveValue(6)]
            public ushort MaterialIndexMaybe { get; set; }

            [PrimitiveValue(8)]
            public Vector3 Params { get; set; }

            [PrimitiveArray(20, 2)]
            public float[] FloatsA { get; set; }

            [PrimitiveArray(28, 6)]
            public ushort[] ShortsA { get; set; }

            [PrimitiveArray(44, 7)]
            public float[] FloatsB { get; set; }

            [PrimitiveArray(96, 36)]
            public float[] FloatsC { get; set; }
        }

        [FixedLength(48)]
        public class Obj120
        {
            [PrimitiveArray(0, 4)]
            public Vector3[] VertsMaybe { get; set; }
        }

        [FixedLength(16)]
        public class ColliderPlane
        {
            [PrimitiveValue(0)]
            public Vector3 Normal { get; set; }

            [PrimitiveValue(12)]
            public float Distance { get; set; }
        }

        [FixedLength(56)]
        public class Obj144
        {
            // Haven't seen anything meaningful yet
        }

        [FixedLength(8)]
        public class Obj152
        {
            // Haven't seen anything meaningful yet
        }

        [FixedLength(24)]
        public class Obj160
        {
            // Haven't seen anything meaningful yet
        }

        [FixedLength(148)]
        public class RagdollComponent
        {
            [InternedString(0)]
            public string ComponentName { get; set; }
        }

        [FixedLength(12)]
        public class Variant
        {
            [InternedString(0)]
            public string VariantName { get; set; }

            [ReferenceArray(4)]
            public DamageLevel[] DamageLevels { get; set; }


            [FixedLength(12)]
            public class DamageLevel
            {
                [InternedString(0)]
                public string DamageLevelName { get; set; }

                [ReferenceArray(4)]
                public NestedObj2[] Nested2 { get; set; }


                [FixedLength(4)]
                public class NestedObj2
                {
                    [PrimitiveValue(0)]
                    // Body index?
                    public ushort ValA { get; set; }

                    [PrimitiveValue(2)]
                    public ushort ValB { get; set; }
                }
            }
        }

        [FixedLength(12)]
        public class Obj200
        {
            [InternedString(0)]
            public string ObjName { get; set; }

            [PrimitiveValue(4)]
            public ushort ValA { get; set; }

            [PrimitiveValue(6)]
            public ushort ValB { get; set; }

            [PrimitiveValue(8)]
            public ushort ValC { get; set; }

            [PrimitiveValue(10)]
            public ushort ValD { get; set; }
        }

        [FixedLength(132)]
        public class Obj232
        {
            [InternedString(0)]
            public string ObjName { get; set; }

            [PrimitiveValue(4)]
            public ushort ValA { get; set; }

            [PrimitiveValue(6)]
            public ushort ValB { get; set; }

            [PrimitiveArray(8, 31)]
            public float[] FloatsA { get; set; }
        }
    }
}