using OpenH2.Core.Architecture;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Engine.Stores;
using OpenH2.Foundation;
using OpenH2.Foundation.Physics;
using OpenH2.Physics.Colliders;
using PhysX;
using PhysX.VisualDebugger;
using System;
using System.Collections.Generic;
using System.Numerics;
using PxFoundation = PhysX.Foundation;
using PxPhysics = PhysX.Physics;
using PxScene = PhysX.Scene;

namespace OpenH2.Engine.Systems
{
    public class PhysicsSystem : WorldSystem
    {
        private PxFoundation physxFoundation;
        private PxPhysics physxPhysics;
        private PxScene physxScene;
        private Cooking cooker;
        private Material defaultMat;
        private Pvd pvd;


        private InputStore input;
        public bool StepMode = true;
        public bool ShouldStep = false;

        public PhysicsSystem(World world) : base(world)
        {
            // Setup PhysX infra here

            this.physxFoundation = new PxFoundation(new ConsoleErrorCallback());

#if DEBUG
            pvd = new Pvd(this.physxFoundation);
            pvd.Connect("localhost", 5425, TimeSpan.FromSeconds(10), InstrumentationFlag.All);
            this.physxPhysics = new PxPhysics(this.physxFoundation, false, pvd);
#else
            this.physxPhysics = new PxPhysics(this.physxFoundation);
#endif
            this.defaultMat = this.physxPhysics.CreateMaterial(0.5f, 0.5f, 0.1f);            

            var sceneDesc = new SceneDesc()
            {
                BroadPhaseType = BroadPhaseType.SweepAndPrune,
                Gravity = world.Gravity,
                Flags = SceneFlag.EnableStabilization,
                SolverType = SolverType.TGS
            };

            this.physxScene = this.physxPhysics.CreateScene(sceneDesc);
            var scale = TolerancesScale.Default;
            var cookingParams = new CookingParams()
            {
                Scale = scale,
                AreaTestEpsilon = 0.06f * scale.Length * scale.Length,
                MidphaseDesc = new MidphaseDesc()
                {
                    Bvh33Desc = new Bvh33MidphaseDesc()
                    {
                        MeshCookingHint = MeshCookingHint.SimulationPerformance
                    }
                }
            };

            cooker = this.physxPhysics.CreateCooking(cookingParams);
        }

        public override void Initialize(Core.Architecture.Scene scene)
        {
            // Cook terrain and static geom meshes
            var terrains = world.Components<StaticTerrainComponent>();

            foreach (var terrain in terrains)
            {
                var meshDesc = new TriangleMeshDesc()
                {
                    Points = terrain.Vertices,
                    Triangles = terrain.TriangleIndices
                };

                // Avoiding offline cook path for now because
                //   1. Comments in Physx.Net imply memory leak using streams
                //   2. I don't want to deal with disk caching cooks yet
                var finalMesh = this.physxPhysics.CreateTriangleMesh(cooker, meshDesc);

                var meshGeom = new TriangleMeshGeometry(finalMesh);

                var rigid = this.physxPhysics.CreateRigidStatic();
                RigidActorExt.CreateExclusiveShape(rigid, meshGeom, defaultMat);
                this.physxScene.AddActor(rigid);
            }

            var scenery = world.Components<StaticGeometryComponent>();
            // TODO: other static geom

            var rigidBodies = this.world.Components<RigidBodyComponent>();
            foreach(var body in rigidBodies)
            {
                AddRigidBodyComponent(body);
            }

            scene.OnEntityAdd += this.AddEntityRigidBodies;
        }

        public void AddEntityRigidBodies(Entity entity)
        {
            if(entity.TryGetChild<RigidBodyComponent>(out var rigidBody) == false)
            {
                return;
            }

            AddRigidBodyComponent(rigidBody);
        }

        public void AddRigidBodyComponent(RigidBodyComponent component)
        {
            var actor = this.physxPhysics.CreateRigidDynamic(component.Transform.TransformationMatrix);
            actor.UserData = component;
            actor.Name = component.Parent.Id.ToString();
            actor.CenterOfMassLocalPose = Matrix4x4.CreateTranslation(component.CenterOfMass);
            actor.MassSpaceInertiaTensor = MathUtil.Diagonalize(component.InertiaTensor);
            actor.Mass = component.Mass;

            if (component.Collider is IVertexBasedCollider vertCollider)
            {
                var desc = new ConvexMeshDesc() { Flags = ConvexFlag.ComputeConvex };
                desc.SetPositions(vertCollider.Vertices);
                var mesh = this.physxPhysics.CreateConvexMesh(this.cooker, desc);
                var geom = new ConvexMeshGeometry(mesh);
                // TODO: re-use shared shapes instead of creating exclusive
                RigidActorExt.CreateExclusiveShape(actor, geom, defaultMat);
            }
            else if(component.Collider is ConvexModelCollider modelCollider)
            {
                foreach(var verts in modelCollider.Meshes)
                {
                    var desc = new ConvexMeshDesc() { Flags = ConvexFlag.ComputeConvex };
                    desc.SetPositions(verts);
                    var mesh = this.physxPhysics.CreateConvexMesh(this.cooker, desc);
                    var geom = new ConvexMeshGeometry(mesh);
                    // TODO: re-use shared shapes instead of creating exclusive
                    RigidActorExt.CreateExclusiveShape(actor, geom, defaultMat);
                }
            }

            this.physxScene.AddActor(actor);
        }

        public override void Update(double timestep)
        {
            if (TakeStep() == false)
                return;

            // Update all PhysX transforms, add forces?
            // 
            var rigidBodies = this.world.Components<RigidBodyComponent>();
            foreach(var rigid in rigidBodies)
            {
                //if(ActorMap.TryGetValue(rigid, out var actor) && actor is RigidDynamic dynamic)
                {
                    //dynamic.AddForce(rigid.ForceAccumulator);
                    //dynamic.AddTorque(rigid.TorqueAccumulator);
                }
            }

            // Simulate, fetch results
            this.physxScene.Simulate((float)timestep);
            this.physxScene.FetchResults(true);

            // Update all engine transforms, reset forces?
            foreach (var actor in this.physxScene.RigidDynamicActors)
            {
                if(actor.UserData is RigidBodyComponent component)
                {
                    component.Transform.UseTransformationMatrix(actor.GlobalPose);
                }
            }
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

                return ShouldStep;
            }

            return true;
        }

        private class ConsoleErrorCallback : ErrorCallback
        {
            public override void ReportError(ErrorCode errorCode, string message, string file, int lineNumber)
            {
                Console.WriteLine("[PHYSX-{0}] {1} @ {2}:{3}", errorCode.ToString(), message, file, lineNumber);
            }
        }
    }
}
