using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;
using System.Numerics;
using OpenBlam.Serialization.Materialization;
using System.IO;
using System;
using OpenBlam.Core.MapLoading;
using System.Diagnostics;

namespace OpenH2.Core.Tags
{
    [FixedLength(48)]
    public unsafe struct TetrahedralHull
    {
        private fixed float xValues[4];
        private fixed float yValues[4];
        private fixed float zValues[4];

        public Vector3 I => new Vector3(xValues[0], yValues[0], zValues[0]);
        public Vector3 J => new Vector3(xValues[1], yValues[1], zValues[1]);
        public Vector3 K => new Vector3(xValues[2], yValues[2], zValues[2]);
        public Vector3 L => new Vector3(xValues[3], yValues[3], zValues[3]);
        public Vector3 this[int i] => new Vector3(xValues[i], yValues[i], zValues[i]);

        public TetrahedralHull(Vector3 i, Vector3 j, Vector3 k, Vector3 l)
        {
            xValues[0] = i.X;
            yValues[0] = i.Y;
            zValues[0] = i.Z;

            xValues[1] = j.X;
            yValues[1] = j.Y;
            zValues[1] = j.Z;

            xValues[2] = k.X;
            yValues[2] = k.Y;
            zValues[2] = k.Z;

            xValues[3] = l.X;
            yValues[3] = l.Y;
            zValues[3] = l.Z;
        }

        [PrimitiveValueMaterializer]
        public static TetrahedralHull ReadTetrahedralHull(Stream s, int offset)
        {
            var hull = new TetrahedralHull();
            var hullBytes = new Span<byte>(&hull, sizeof(TetrahedralHull));
            s.Position = offset;
            s.Read(hullBytes);
            return hull;
        }

        [PrimitiveValueMaterializer]
        public static TetrahedralHull ReadTetrahedralHull(ReadOnlySpan<byte> s, int offset)
        {
            var hull = new TetrahedralHull();
            var hullBytes = new Span<byte>(&hull, sizeof(TetrahedralHull));
            s.Slice(offset, sizeof(TetrahedralHull)).CopyTo(hullBytes);
            return hull;
        }
    }

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
        public Phantom[] Phantoms { get; set; }

        [ReferenceArray(48)]
        public NodeEdge[] NodeEdges { get; set; }

        [ReferenceArray(56)]
        public RigidBody[] RigidBodies { get; set; }

        [ReferenceArray(64)]
        public MaterialReference[] MaterialReferences { get; set; }

        [ReferenceArray(72)]
        public SphereDefinition[] Spheres { get; set; }

        // unused
        [ReferenceArray(80)]
        public MultiSphereDefinition[] MultiSpheres { get; set; }

        [ReferenceArray(88)]
        public CapsuleDefinition[] CapsuleDefinitions { get; set; }

        [ReferenceArray(96)]
        public BoxDefinition[] BoxDefinitions { get; set; }

        // unused
        [ReferenceArray(104)]
        public TriangleDefinition[] TriangleDefinitions { get; set; }

        [ReferenceArray(112)]
        public PolyhedraDefinition[] PolyhedraDefinitions { get; set; }

        [ReferenceArray(120)]
        public TetrahedralHull[] PolyhedraAlternativeDefinitions { get; set; }

        [ReferenceArray(128)]
        public PolyhedronPlane[] PolyhedronPlanes { get; set; }

        // unused? baked away?
        [ReferenceArray(136)]
        public MassDistributionBlock[] MassDistributionBlocks { get; set; }

        [ReferenceArray(144)]
        public List[] Lists { get; set; }

        [ReferenceArray(152)]
        public ListShape[] ListShapes { get; set; }

        // mopps @160

        [ReferenceArray(168)]
        public byte[] MoppCodes { get; set; }

        [ReferenceArray(176)]
        public HingeConstraint[] HingeContraints { get; set; }

        [ReferenceArray(184)]
        public RagdollConstraint[] RagdollConstraints { get; set; }

        [ReferenceArray(192)]
        public Region[] Regions { get; set; }

        [ReferenceArray(200)]
        public Node[] Nodes { get; set; }

        [ReferenceArray(232)]
        public LimitedHingeConstraint[] LimitedHingeConstraints { get; set; }

        // ball and socket constraints @240

        public override void PopulateExternalData(MapStream reader)
        {
            var polyOffset = 0;
            foreach(var poly in this.PolyhedraDefinitions)
            {
                if(poly.HullCount > 3)
                {
                    poly.ExternalHullsOffset = polyOffset;
                    polyOffset += poly.HullCount;
                }
            }

            Debug.Assert(polyOffset == this.PolyhedraAlternativeDefinitions.Length);

            var listOffset = 0;
            foreach (var list in this.Lists)
            {
                if (list.Count > 4)
                {
                    list.ExternalShapesOffset = listOffset;
                    listOffset += list.Count;
                }
            }

            Debug.Assert(listOffset == this.ListShapes.Length);
        }

        [FixedLength(104)]
        public class Phantom
        {
            [PrimitiveValue(4)]
            public ushort Flags { get; set; }

            [InternedString(8)]
            public string Description { get; set; }

            [PrimitiveValue(32)]
            public float ValueC { get; set; }

            [PrimitiveValue(36)]
            public float ValueD { get; set; }

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
        public class NodeEdge
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

        public enum RigidBodyMotion : ushort
        {
            Sphere = 0,
            StabilizedSphere = 1,
            Box = 2,
            StabilizedBox = 3,
            Keyframed = 4,
            Fixed = 5
        }

        public enum RigidBodyComponentType : ushort
        {
            Sphere = 0,
            Capsule = 1,
            Box = 2,
            Triangles = 3,
            Polyhedra = 4,
            Phantom = 6,
            FixedList = 14,
            ComponentList = 15
        }

        [FixedLength(144)]
        public class RigidBody
        {
            [PrimitiveValue(0)]
            public ushort Node { get; set; }

            [PrimitiveValue(2)]
            public ushort Region { get; set; }

            [PrimitiveValue(4)]
            public ushort Permutation { get; set; }

            [PrimitiveValue(8)]
            public Vector3 BoundingSphereOffset { get; set; }

            [PrimitiveValue(20)]
            public float BoundingSphereRadius { get; set; }

            [PrimitiveValue(24)]
            public ushort Flags { get; set; }

            [PrimitiveValue(26)]
            public RigidBodyMotion MotionType { get; set; }

            [PrimitiveValue(28)]
            public ushort NoPhantomPowerAltIndex { get; set; }

            [PrimitiveValue(30)]
            public ushort Size { get; set; }

            [PrimitiveValue(32)]
            public float InertiaTensorScale { get; set; }

            [PrimitiveValue(36)]
            /// <summary>0 - 10 (10 is really, really high)</summary>
            public float LinearDamping { get; set; }

            [PrimitiveValue(40)]
            /// <summary>0 - 10 (10 is really, really high)</summary>
            public float AngularDamping { get; set; }

            [PrimitiveValue(44)]
            public Vector3 CentorOfMassOffset { get; set; }

            [PrimitiveValue(56)]
            public RigidBodyComponentType ComponentType { get; set; }

            [PrimitiveValue(58)]
            public ushort ComponentIndex { get; set; }

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
            public string Name { get; set; }

            [InternedString(4)]
            public string GlobalMaterialName { get; set; }

            [PrimitiveValue(8)]
            public ushort PhantomIndex { get; set; }

            [PrimitiveValue(10)]
            public ushort Flags { get; set; }
        }

        [FixedLength(128)]
        public class SphereDefinition
        {
            [InternedString(0)]
            public string Name { get; set; }

            [PrimitiveValue(4)]
            public ushort Material { get; set; }

            [PrimitiveValue(6)]
            public ushort Flags { get; set; }

            [PrimitiveValue(8)]
            public float RelativeMassScale { get; set; }

            [PrimitiveValue(12)]
            public float Friction { get; set; }

            [PrimitiveValue(16)]
            public float Restitution { get; set; }

            [PrimitiveValue(20)]
            public float Volume { get; set; }

            [PrimitiveValue(24)]
            public float Mass { get; set; }


            [PrimitiveValue(28)]
            public ushort ComponentIndex { get; set; }

            [PrimitiveValue(30)]
            public byte PhantomIndex { get; set; }

            [PrimitiveValue(36)]
            public ushort Size { get; set; }

            [PrimitiveValue(38)]
            public ushort Count { get; set; }

            [PrimitiveValue(44)]
            public float Radius { get; set; }

            [PrimitiveValue(52)]
            public ushort Size2 { get; set; }

            [PrimitiveValue(54)]
            public ushort Count2 { get; set; }

            [PrimitiveValue(64)]
            public Matrix4x4 Transform { get; set; }
        }

        [FixedLength(132)]
        public class MultiSphereDefinition
        {
            [InternedString(0)]
            public string Name { get; set; }

            [PrimitiveValue(4)]
            public ushort Material { get; set; }

            [PrimitiveValue(6)]
            public ushort Flags { get; set; }

            [PrimitiveValue(8)]
            public float RelativeMassScale { get; set; }

            [PrimitiveValue(12)]
            public float Friction { get; set; }

            [PrimitiveValue(16)]
            public float Restitution { get; set; }

            [PrimitiveValue(20)]
            public float Volume { get; set; }

            [PrimitiveValue(24)]
            public float Mass { get; set; }

            [PrimitiveValue(28)]
            public ushort PhantomIndex { get; set; }

            [PrimitiveValue(30)]
            public ushort Size { get; set; }

            [PrimitiveValue(32)]
            public ushort Count { get; set; }

            [PrimitiveValue(34)]
            public ushort NumSpheres { get; set; }


            [PrimitiveArray(36, 24)]
            public float[] Spheres { get; set; }
        }


        [FixedLength(80)]
        public class CapsuleDefinition
        {
            [InternedString(0)]
            public string Name { get; set; } 

            [PrimitiveValue(4)]
            public ushort Material { get; set; }

            [PrimitiveValue(6)]
            public ushort Flags { get; set; }

            [PrimitiveValue(8)]
            public float RelativeMassScale { get; set; }

            [PrimitiveValue(12)]
            public float Friction { get; set; }

            [PrimitiveValue(16)]
            public float Restitution { get; set; }

            [PrimitiveValue(20)]
            public float Volume { get; set; }

            [PrimitiveValue(24)]
            public float Mass { get; set; }


            [PrimitiveValue(28)]
            public ushort ComponentIndex { get; set; }

            [PrimitiveValue(30)]
            public byte PhantomIndex { get; set; }

            [PrimitiveValue(36)]
            public ushort Size { get; set; }

            [PrimitiveValue(38)]
            public ushort Count { get; set; }

            [PrimitiveValue(44)]
            public float Radius { get; set; }

            [PrimitiveValue(48)]
            public Vector3 Bottom { get; set; }

            [PrimitiveValue(64)]
            public Vector3 Top { get; set; }
        }

        [FixedLength(144)]
        public class BoxDefinition
        {
            [InternedString(0)]
            public string Name { get; set; }

            [PrimitiveValue(4)]
            public ushort Material { get; set; }

            [PrimitiveValue(6)]
            public ushort Flags { get; set; }

            [PrimitiveValue(8)]
            public float RelativeMassScale { get; set; }

            [PrimitiveValue(12)]
            public float Friction { get; set; }

            [PrimitiveValue(16)]
            public float Restitution { get; set; }

            [PrimitiveValue(20)]
            public float Volume { get; set; }

            [PrimitiveValue(24)]
            public float Mass { get; set; }

            [PrimitiveValue(28)]
            public ushort PhantomIndex { get; set; }

            [PrimitiveValue(30)]
            public ushort Size { get; set; }

            [PrimitiveValue(32)]
            public ushort Count { get; set; }

            [PrimitiveValue(34)]
            public float Radius { get; set; }

            [PrimitiveValue(48)]
            public Vector3 HalfWidthsMaybe { get; set; }

            // other stuff, some floats, zeroes

            [PrimitiveValue(80)]
            public Matrix4x4 Transform { get; set; }
        }

        [FixedLength(90)]
        public class TriangleDefinition
        {
            [InternedString(0)]
            public string Name { get; set; }

            [PrimitiveValue(4)]
            public ushort Material { get; set; }

            [PrimitiveValue(6)]
            public ushort Flags { get; set; }

            [PrimitiveValue(8)]
            public float RelativeMassScale { get; set; }

            [PrimitiveValue(12)]
            public float Friction { get; set; }

            [PrimitiveValue(16)]
            public float Restitution { get; set; }

            [PrimitiveValue(20)]
            public float Volume { get; set; }

            [PrimitiveValue(24)]
            public float Mass { get; set; }

            [PrimitiveValue(28)]
            public ushort PhantomIndex { get; set; }

            [PrimitiveValue(30)]
            public ushort Size { get; set; }

            [PrimitiveValue(32)]
            public ushort Count { get; set; }

            [PrimitiveValue(34)]
            public float Radius { get; set; }

            [PrimitiveArray(42, 3)]
            public Vector3 Translation { get; set; }
        }

        [FixedLength(256)]
        public class PolyhedraDefinition
        {
            [InternedString(0)]
            public string Name { get; set; }

            [PrimitiveValue(4)]
            public ushort Material { get; set; }

            [PrimitiveValue(6)]
            public ushort Flags { get; set; }

            [PrimitiveValue(8)]
            public float RelativeMassScale { get; set; }

            [PrimitiveValue(12)]
            public float Friction { get; set; }

            [PrimitiveValue(16)]
            public float Restitution { get; set; }

            [PrimitiveValue(20)]
            public float Volume { get; set; }

            [PrimitiveValue(24)]
            public float Mass { get; set; }

            [PrimitiveValue(28)]
            public ushort ComponentIndex { get; set; }

            [PrimitiveValue(30)]
            public byte PhantomIndex { get; set; }

            [PrimitiveValue(36)]
            public ushort Size { get; set; }

            [PrimitiveValue(38)]
            public ushort Count { get; set; }

            [PrimitiveValue(44)]
            public float Radius { get; set; }

            [PrimitiveValue(48)]
            public Vector3 AabbHalfExtents { get; set; }

            [PrimitiveValue(64)]
            public Vector3 AabbCenter { get; set; }

            [PrimitiveValue(84)]
            public short HullCount { get; set; }

            [PrimitiveValue(88)]
            public short HullCapacity { get; set; }

            [PrimitiveValue(90)]
            public short HullFlags { get; set; }

            [PrimitiveArray(96, 3)]
            public TetrahedralHull[] InlineHulls { get; set; }

            // must be prepopulated by consumer
            public int ExternalHullsOffset { get; set; }

            [PrimitiveValue(240)]
            public short NumVertices { get; set; }

            [PrimitiveValue(244)]
            public int ValueB { get; set; }

            [PrimitiveValue(248)]
            public short PlaneEquationsCount { get; set; }

            [PrimitiveValue(252)]
            public short PlaneEquationsCapacity { get; set; }
        }

        [FixedLength(16)]
        public class PolyhedronPlane
        {
            [PrimitiveValue(0)]
            public Vector3 Normal { get; set; }

            [PrimitiveValue(12)]
            public float Distance { get; set; }
        }

        [FixedLength(56)]
        public class MassDistributionBlock
        {
            // Haven't seen anything meaningful yet
        }

        [FixedLength(56)]
        public class List
        {
            [PrimitiveValue(0)]
            public int Unknown { get; set; }

            [PrimitiveValue(4)]
            public ushort ValA { get; set; }

            [PrimitiveValue(6)]
            public ushort ValB { get; set; }

            [PrimitiveValue(8)]
            public ushort ValC { get; set; }

            [PrimitiveValue(10)]
            public ushort ValD { get; set; }

            [PrimitiveValue(12)]
            public int Unknown2 { get; set; }

            [PrimitiveValue(16)]
            public int Count { get; set; }

            [PrimitiveValue(20)]
            public int Capacity { get; set; }

            [PrimitiveArray(24, 16)]
            public ushort[] InlineShapes { get; set; }

            public int ExternalShapesOffset { get; set; }
        }

        [FixedLength(8)]
        [DebuggerDisplay("{ComponentType} - {ComponentIndex}")]
        public class ListShape
        {
            [PrimitiveValue(0)]
            public RigidBodyComponentType ComponentType { get; set; }

            [PrimitiveValue(2)]
            public ushort ComponentIndex { get; set; }

            [PrimitiveValue(4)]
            public ushort ValC { get; set; }

            [PrimitiveValue(6)]
            public ushort ValD { get; set; }
        }

        [FixedLength(148)]
        public class HingeConstraint
        {
            [InternedString(0)]
            public string Name { get; set; }
        }

        [FixedLength(148)]
        public class RagdollConstraint
        {
            [InternedString(0)]
            public string Name { get; set; }
        }

        [FixedLength(12)]
        public class Region
        {
            [InternedString(0)]
            public string Name { get; set; }

            [ReferenceArray(4)]
            public Permutation[] Permutations { get; set; }


            [FixedLength(12)]
            public class Permutation
            {
                [InternedString(0)]
                public string Name { get; set; }

                [ReferenceArray(4)]
                public RigidBodyRef[] RigidBodies { get; set; }


                [FixedLength(4)]
                public class RigidBodyRef
                {
                    [PrimitiveValue(0)]
                    public ushort RigidBodyIndex { get; set; }

                    [PrimitiveValue(2)]
                    public ushort ValB { get; set; }
                }
            }
        }

        [FixedLength(12)]
        public class Node
        {
            [InternedString(0)]
            public string Name { get; set; }

            [PrimitiveValue(4)]
            public ushort Flags { get; set; }

            [PrimitiveValue(6)]
            public ushort Parent { get; set; }

            [PrimitiveValue(8)]
            public ushort Sibling { get; set; }

            [PrimitiveValue(10)]
            public ushort Child { get; set; }
        }

        [FixedLength(132)]
        public class LimitedHingeConstraint
        {
            [InternedString(0)]
            public string Name { get; set; }

            [PrimitiveValue(4)]
            public ushort NodeA { get; set; }

            [PrimitiveValue(6)]
            public ushort NodeB { get; set; }

            [PrimitiveArray(8, 26)]
            public float[] FloatsA { get; set; }


            [PrimitiveValue(120)]
            public float LimitFriction { get; set; }

            [PrimitiveValue(124)]
            public float LimitMinAngle { get; set; }

            [PrimitiveValue(128)]
            public float LimitMaxAngle { get; set; }
        }
    }
}
