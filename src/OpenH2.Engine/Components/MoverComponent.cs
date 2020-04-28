using OpenH2.Core.Architecture;
using OpenH2.Physics.Proxying;
using System.Numerics;

namespace OpenH2.Engine.Components
{
    /// <summary>
    /// A component representing a controllable mover that moves the entity in the desired way
    /// Also known as a "Character Controller"
    /// </summary>
    public class MoverComponent : Component
    {
        public IPhysicsProxy PhysicsImplementation { get; set; } = NullPhysicsProxy.Instance;
        public TransformComponent Transform { get; }
        public MovementConfig Config { get; }
        public MovementMode Mode { get; }

        public MoverComponent(Entity parent, TransformComponent xform, MovementConfig config) : base(parent)
        {
            this.Transform = xform;
            this.Config = config;
            this.Mode = config.Mode;
        }

        public class MovementConfig
        {
            public MovementConfig(float height = 0.725f, float speed = 1f)
            {
                var halfHeight = height / 2f;

                Mode = MovementMode.Freecam;
                Speed = speed;
                Height = height;
                FootOffset = new Vector3(0, 0, -halfHeight);
                EyeOffset = new Vector3(0, 0, halfHeight);
            }

            public MovementMode Mode { get; set; }
            public float Speed { get; set; }
            public float Height { get; set; }
            public Vector3 FootOffset { get; }
            public Vector3 EyeOffset { get; }
        }

        public enum MovementMode
        {
            Freecam,
            KinematicCharacterControl,
            DynamicCharacterControl
        }
    }
}
