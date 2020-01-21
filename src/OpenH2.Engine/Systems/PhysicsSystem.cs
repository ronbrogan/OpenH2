using OpenH2.Core.Architecture;
using OpenH2.Engine.Components;
using OpenH2.Engine.Stores;
using OpenH2.Foundation.Physics;
using OpenH2.Physics.Simulation;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Engine.Systems
{
    public class PhysicsSystem : WorldSystem
    {
        private readonly RigidBodyIntegrator Integrator;
        public readonly IterativePhysicsSimulator Simulator;

        private InputStore input;
        public bool StepMode = true;
        public bool ShouldStep = false;

        public PhysicsSystem(World world) : base(world)
        {
            this.Integrator = new RigidBodyIntegrator();
            this.Simulator = new IterativePhysicsSimulator(1);
        }

        public override void Update(double timestep)
        {
            if(StepMode)
            {
                if(input == null)
                    input = this.world.GetGlobalResource<InputStore>();

                ShouldStep = input.Keyboard.IsKeyDown(OpenTK.Input.Key.P);

                if(ShouldStep == false)
                    return;
            }

            var allBodies = new List<IBody>();

            var rigidBodies = this.world.Components<RigidBodyComponent>();

            foreach(var body in rigidBodies)
            {
                Integrator.Integrate(body, (float)timestep);
                allBodies.Add(body);
            }

            allBodies.AddRange(this.world.Components<StaticGeometryComponent>());

            var contacts = this.Simulator.DetectCollisions(this.world, allBodies);

            this.Simulator.ResolveCollisions(contacts, timestep);
        }
    }
}
