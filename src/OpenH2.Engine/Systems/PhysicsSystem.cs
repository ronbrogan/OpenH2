using OpenH2.Core.Architecture;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Engine.Stores;
using OpenH2.Foundation;
using OpenH2.Foundation.Physics;
using OpenH2.Physics.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
        public bool DebugContacts = true;

        private Mesh<BitmapTag> DebugMesh;
        private Model<BitmapTag> DebugModel;

        public PhysicsSystem(World world) : base(world)
        {
            this.Integrator = new RigidBodyIntegrator();
            this.Simulator = new IterativePhysicsSimulator(10);


            DebugMesh = new Mesh<BitmapTag>()
            {
                ElementType = MeshElementType.Point,
                Verticies = Array.Empty<VertexFormat>(),
                Indicies = Array.Empty<int>(),
                Material = new Material<BitmapTag>() { DiffuseColor = new Vector4(0, 1, 1, 1) }
            };

            DebugModel = new Model<BitmapTag>
            {
                Meshes = new[] { DebugMesh },
                Flags = ModelFlags.DebugViz
            };
        }

        public override void Update(double timestep)
        {
            if (TakeStep() == false)
                return;

            var allBodies = new List<IBody>();

            var rigidBodies = this.world.Components<RigidBodyComponent>();

            foreach(var body in rigidBodies)
            {
                Integrator.Integrate(body, (float)timestep);
                allBodies.Add(body);
            }

            allBodies.AddRange(this.world.Components<StaticGeometryComponent>());

            var contacts = this.Simulator.DetectCollisions(this.world, allBodies);

            if(DebugContacts)
            {
                UpdateAndRenderContacts(contacts);
            }

            this.Simulator.ResolveCollisions(contacts, timestep);
        }

        private bool TakeStep()
        {
            if (input == null)
                input = this.world.GetGlobalResource<InputStore>();

            if (input.Keyboard.IsKeyDown(OpenTK.Input.Key.P) && input.Keyboard.IsKeyDown(OpenTK.Input.Key.ShiftRight))
                StepMode = !StepMode;

            if (StepMode)
            {
                ShouldStep = input.Keyboard.IsKeyDown(OpenTK.Input.Key.P);

                if (ShouldStep == false)
                {
                    // Render debug contacts from previous iteration
                    if (DebugContacts)
                    {
                        var renderstore = this.world.GetGlobalResource<RenderListStore>();
                        renderstore.AddModel(DebugModel, Matrix4x4.Identity);
                    }

                    return false;
                }
            }

            return true;
        }

        private void UpdateAndRenderContacts(Contact[] contacts)
        {
            var renderstore = this.world.GetGlobalResource<RenderListStore>();

            // Ensure there's enough space + some padding for future growth
            if (DebugMesh.Verticies.Length < contacts.Length)
            {
                DebugMesh.Verticies = new VertexFormat[contacts.Length + 64];
                DebugMesh.Indicies = new int[contacts.Length + 64];
            }

            for (var i = 0; i < contacts.Length; i++)
            {
                var p = contacts[i];
                DebugMesh.Verticies[i] = new VertexFormat(p.Point, Vector2.Zero, p.Normal);
                DebugMesh.Indicies[i] = i;
            }

            // Set rest of indices array to 0
            Array.Clear(DebugMesh.Indicies, contacts.Length, DebugMesh.Indicies.Length - contacts.Length);

            DebugMesh.Dirty = true;

            renderstore.AddModel(DebugModel, Matrix4x4.Identity);
        }
    }
}
