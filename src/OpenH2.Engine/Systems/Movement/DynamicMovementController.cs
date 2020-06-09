using OpenH2.Core.Extensions;
using OpenH2.Foundation.Extensions;
using OpenH2.Physics.Proxying;
using System.Numerics;

namespace OpenH2.Engine.Systems.Movement
{


    public class DynamicMovementController
    {
        private float airSpeed = 1f;
        private float groundSpeed = 25f;
        private float jumpSpeed = 1f;
        private float maxVelocityChange = 5.0f;
        public ControllerState state { get; private set; } = ControllerState.Walking;

        public DynamicMovementController()
        {
        }

        public void Move(IPhysicsProxy physics,
            Vector3 inputVector,
            Vector3 forward,
            Vector3 strafe)
        {
            var groundNormal = ProcessCollision(physics);

            // align our movement vectors with the ground normal (ground normal = 'up')
            Vector3 newForward = VectorExtensions.OrthoNormalize(groundNormal, forward);
            Vector3 newStrafe = VectorExtensions.OrthoNormalize(groundNormal, strafe);

            // Calculate movement direction
            var desiredMovement = (inputVector.X * newForward) + (inputVector.Y * newStrafe);

            HandleMovement(physics, desiredMovement);

            // Jump
            if (inputVector.Z > 0f)
            {
                TryJump(physics, groundNormal);   
            }
        }

        public Vector3 ProcessCollision(IPhysicsProxy physics)
        {
            var footResults = physics.Raycast(-EngineGlobals.Up, 1f, 1);

            if (footResults.Length == 0)
            {
                this.state = ControllerState.Falling;
                return EngineGlobals.Up;
            }

            var contact = footResults[0];

            // Handle landing from a fall
            if(this.state == ControllerState.Falling)
            {
                this.state = ControllerState.Walking;
            }

            // TODO: handle walking|sprinting -> sliding
            //    - If velocity exceeds some threshold, transition to sliding
            //    - If we're climbing too steep, transition to sliding

            // TODO: handle sliding -> walking
            //    - If we're slding and drop below threshold and not too steep, transition back to walking

            return contact.Normal;
        }

        public void HandleMovement(IPhysicsProxy physics, Vector3 movement)
        {
            Vector3 currentVelocity = physics.GetVelocity();

            if (this.state == ControllerState.Walking)
            {
                movement *= groundSpeed;
                // TODO: figure out a better limiting method? velocity going up slopes is limited
                Vector3 velocityChange = (movement - currentVelocity);
                velocityChange.X = MathExt.Clamp(velocityChange.X, -maxVelocityChange, maxVelocityChange);
                velocityChange.Y = MathExt.Clamp(velocityChange.Y, -maxVelocityChange, maxVelocityChange);
                velocityChange.Z = 0;
                physics.AddVelocity(velocityChange);
            }
            else if (this.state == ControllerState.Falling)
            {
                // TODO: limit velocity in direction of movement
                movement *= airSpeed;
                physics.AddVelocity(movement);
            }
            else
            {
                // TODO: handle sprinting and sliding movement
                // TODO: desired feel during sliding is 0 friction in the movement direction, simulate via artificial velocity?
                return;
            }            
        }

        private void TryJump(IPhysicsProxy physics, Vector3 jumpDirection)
        {
            if(state != ControllerState.Falling)
            {
                physics.AddVelocity(Vector3.Multiply(jumpSpeed, jumpDirection));
            }
        }

        public enum ControllerState
        {
            Walking,
            Sprinting,
            Sliding,
            Falling
        }
    }
}
