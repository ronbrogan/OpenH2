using OpenH2.Core.Animation;
using OpenH2.Core.Architecture;
using OpenH2.Core.GameObjects;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using System;
using System.Collections.Generic;

namespace OpenH2.Engine.Systems
{
    /// <summary>
    /// AnimationSystem is responsible for the state of PoseComponent instances
    /// This primarily consists of updating the PoseComponent with the proper
    /// current animation data and ticking to the next frame's index
    /// 
    /// This system does not handle processing the pose into, for example, transformed
    /// vertices of a mesh. This is the responsibilty of whatever needs to express the pose
    /// </summary>
    public class AnimationSystem : WorldSystem
    {
        private const double tickTime = 1d / 30d;
        double totalTime = 0;


        public AnimationSystem(World world) : base(world)
        {
        }

        public override void Update(double timestep)
        {
            totalTime += timestep;
            var poses = world.Components<PoseComponent>();

            while(totalTime > tickTime)
            {
                totalTime -= tickTime;

                Tick(poses);
            }
        }

        private void Tick(List<PoseComponent> poseComponents)
        {
            // Process animation changes from scripts, other systems?

            // Update current/next pose index
        }

        internal void StartAnimation(IUnit unit, AnimationGraphTag.Animation animation, bool interpolate, bool loop)
        {
            var frameData = JmadDataProcessor.GetProcessor().GetAnimation(animation.FrameCount, animation.NodeCount, animation.Data);

            // Find PostComponent for unit?

            // Set animation on the PoseComponent

            throw new NotImplementedException();
        }
    }
}
