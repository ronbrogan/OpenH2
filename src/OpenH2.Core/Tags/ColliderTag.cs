using OpenH2.Core.Tags.Common.Collision;
using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.coll)]
    public class ColliderTag : BaseTag
    {
        public ColliderTag(uint id) : base(id)
        {
        }

        [ReferenceArray(20)]
        public Id[] Ids { get; set; }

        [ReferenceArray(28)]
        public ColliderDefinition[] ColliderComponents { get; set; }

        [ReferenceArray(36)]
        public Obj36[] Obj36s { get; set; }

        [ReferenceArray(44)]
        public Obj44[] Obj44s { get; set; }

        [FixedLength(4)]
        public class Id
        {
            [InternedString(0)]
            public string Value { get; set; }
        }

        [FixedLength(12)] 
        public class ColliderDefinition 
        {
            [InternedString(0)]
            public string DefName { get; set; }
            
            [ReferenceArray(4)]
            public CollisionContainer[] DamageLevels { get; set; }

            [FixedLength(20)]
            public class CollisionContainer
            {
                [InternedString(0)]
                public string CollName { get; set; }

                [ReferenceArray(4)]
                public CollisionInfo[] Parts { get; set; }

                [PrimitiveArray(12, 2)]
                public uint[] Obj12s { get; set; }

                [FixedLength(68)]
                public class CollisionInfo : ICollisionInfo
                {
                    [ReferenceArray(4)]
                    public Node3D[] Node3Ds { get; set; }

                    [ReferenceArray(12)]
                    public Common.Collision.Plane[] Planes { get; set; }

                    [ReferenceArray(20)]
                    public RawObject3[] RawObject3s { get; set; }

                    //[ReferenceArray(28)]
                    public RawObject4[] RawObject4s { get; set; }

                    [ReferenceArray(36)]
                    public Node2D[] Node2Ds { get; set; }

                    [ReferenceArray(44)]
                    public Face[] Faces { get; set; }

                    [ReferenceArray(52)]
                    public HalfEdgeContainer[] HalfEdges { get; set; }

                    [ReferenceArray(60)]
                    public Vertex[] Vertices { get; set; }
                }
            }
        }

        [FixedLength(4)] public class Obj36 { }
        [FixedLength(4)] public class Obj44 { }
    }
}