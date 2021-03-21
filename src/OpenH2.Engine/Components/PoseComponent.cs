using OpenH2.Core.Animation;
using OpenH2.Core.Architecture;

namespace OpenH2.Engine.Components
{
    public class PoseComponent : Component
    {
        public AnimationNodeTransform[,] CurrentAnimation { get; set; }
        public int CurrentFrame { get; set; }

        public PoseComponent(Entity parent) : base(parent)
        {
        }

        public AnimationNodeTransform GetBoneTransform(int boneIndex)
        {
            if(boneIndex >= CurrentAnimation.GetLength(1))
            {
                return default;
            }

            return CurrentAnimation[CurrentFrame, boneIndex];
        }
    }
}
