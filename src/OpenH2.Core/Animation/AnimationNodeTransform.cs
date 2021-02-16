using System.Numerics;

namespace OpenH2.Core.Animation
{
    public struct AnimationNodeTransform
    {
        public Quaternion Orientation { get; set; }
        public Vector3 Translation { get; set; }

        public AnimationNodeTransform(Quaternion orient, Vector3 translate)
        {
            this.Orientation = orient;
            this.Translation = translate;
        }
    }
}
