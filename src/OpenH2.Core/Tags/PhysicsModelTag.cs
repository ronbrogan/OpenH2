using OpenH2.Core.Tags.Layout;
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

        [ReferenceArray(56)]
        public BodyParameterSet[] BodyParameters { get; set; }

        [ReferenceArray(64)]
        public Obj64[] Obj64s { get; set; }

        [ReferenceArray(96)]
        public Obj96[] Obj96s { get; set; }

        [ReferenceArray(192)]
        public Obj192[] Obj192s { get; set; }

        [ReferenceArray(200)]
        public Obj200[] Obj200s { get; set; }


        [FixedLength(144)]
        public class BodyParameterSet
        {
            [PrimitiveValue(60)]
            public float Mass { get; set; }

            [PrimitiveValue(64)]
            public Vector3 CenterOfMass { get; set; }

            [PrimitiveValue(80)]
            public Matrix4x4 InertiaTensor { get; set; }

        }

        [FixedLength(16)]
        public class Obj64
        {

        }

        [FixedLength(144)]
        public class Obj96
        {

        }

        [FixedLength(28)]
        public class Obj192
        {

        }

        [FixedLength(12)]
        public class Obj200
        {

        }
    }
}