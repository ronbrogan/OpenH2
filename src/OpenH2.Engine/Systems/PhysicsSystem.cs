using OpenH2.Core.Architecture;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
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
        private ControllerManager controllerManager;
        private Cooking cooker;
        private Material defaultMat;
        private Pvd pvd;

        private Dictionary<MoverComponent, Controller> ControllerMap = new Dictionary<MoverComponent, Controller>();

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
            this.controllerManager = this.physxScene.CreateControllerManager();
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

                var actor = CreateStaticActor(meshDesc);
                this.physxScene.AddActor(actor);
            }

            var sceneries = world.Components<StaticGeometryComponent>();
            foreach(var scenery in sceneries)
            {
                var meshDesc = new TriangleMeshDesc()
                {
                    Points = scenery.Vertices,
                    Triangles = scenery.TriangleIndices
                };

                var actor = CreateStaticActor(meshDesc, scenery.Transform.TransformationMatrix);
                this.physxScene.AddActor(actor);
            }

            var rigidBodies = this.world.Components<RigidBodyComponent>();
            foreach(var body in rigidBodies)
            {
                AddRigidBodyComponent(body);
            }

            var movers = this.world.Components<MoverComponent>();
            foreach (var mover in movers)
            {
                AddCharacterController(mover);
            }

            scene.OnEntityAdd += this.AddEntityRigidBodies;
        }

        public override void Update(double timestep)
        {
            if (TakeStep() == false)
                return;

            // Update all PhysX transforms, add forces?
            // 
            var rigidBodies = this.world.Components<RigidBodyComponent>();
            foreach (var rigid in rigidBodies)
            {
                //if(ActorMap.TryGetValue(rigid, out var actor) && actor is RigidDynamic dynamic)
                {
                    //dynamic.AddForce(rigid.ForceAccumulator);
                    //dynamic.AddTorque(rigid.TorqueAccumulator);
                }
            }

            var movers = this.world.Components<MoverComponent>();
            foreach(var mover in movers)
            {
                if(this.ControllerMap.TryGetValue(mover, out var controller))
                {
                    controller.Move(mover.DisplacementAccumulator, TimeSpan.FromSeconds(timestep));
                }
            }

            // Simulate, fetch results
            this.physxScene.Simulate((float)timestep);
            this.physxScene.FetchResults(true);

            // Update all engine transforms, reset forces?
            foreach (var actor in this.physxScene.RigidDynamicActors)
            {
                if (actor.UserData is RigidBodyComponent component)
                {
                    component.Transform.UseTransformationMatrix(actor.GlobalPose);
                }
            }

            foreach(var controller in this.controllerManager.Controllers)
            {
                if(controller.Actor.UserData is MoverComponent mover)
                {
                    mover.Transform.Position = controller.Position;
                    mover.Transform.UpdateDerivedData();
                }
            }
        }

        public void AddEntityRigidBodies(Entity entity)
        {
            if(entity.TryGetChild<RigidBodyComponent>(out var rigidBody))
            {
                AddRigidBodyComponent(rigidBody);
            }

            if(entity.TryGetChild<MoverComponent>(out var mover))
            {
                AddCharacterController(mover);
            }
        }

        public void AddRigidBodyComponent(RigidBodyComponent component)
        {
            RigidActor actor;

            if(component.IsDynamic)
            {
                var dynamic = this.physxPhysics.CreateRigidDynamic(component.Transform.TransformationMatrix);
                dynamic.CenterOfMassLocalPose = Matrix4x4.CreateTranslation(component.CenterOfMass);
                dynamic.MassSpaceInertiaTensor = MathUtil.Diagonalize(component.InertiaTensor);
                dynamic.Mass = component.Mass;
                actor = dynamic;
            }
            else
            {
                actor = this.physxPhysics.CreateRigidStatic(component.Transform.TransformationMatrix);
            }

            actor.UserData = component;
            actor.Name = component.Parent.Id.ToString();

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

        public void AddCharacterController(MoverComponent component)
        {
            if (component.Mode != MoverComponent.MovementMode.CharacterControl)
                return;

            ITransform xform = IdentityTransform.Instance();

            if (component.TryGetSibling<TransformComponent>(out var xformComponent))
            {
                xform = xformComponent;
            }

            var desc = new CapsuleControllerDesc()
            {
                Height = 0.725f,
                Position = xform.Position,
                Radius = 0.175f,
                MaxJumpHeight = 1f,
                UpDirection = new Vector3(0, 0, 1),
                SlopeLimit = MathF.Cos(0.872665f), // cos(50 degrees)
                StepOffset = 0.1f,
                Material = defaultMat,
                ReportCallback = new CustomHitReport()
            };

            var controller = this.controllerManager.CreateController<CapsuleController>(desc);

            controller.Actor.UserData = component;

            this.ControllerMap.Add(component, controller);
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

        private Actor CreateStaticActor(TriangleMeshDesc meshDesc, Matrix4x4? transform = null)
        {
            // Avoiding offline cook path for now because
            //   1. Comments in Physx.Net imply memory leak using streams
            //   2. I don't want to deal with disk caching cooks yet
            var finalMesh = this.physxPhysics.CreateTriangleMesh(cooker, meshDesc);

            var meshGeom = new TriangleMeshGeometry(finalMesh);

            var rigid = this.physxPhysics.CreateRigidStatic(transform);
            RigidActorExt.CreateExclusiveShape(rigid, meshGeom, defaultMat);
            return rigid;
        }

        private class CustomHitReport : UserControllerHitReport
        {
            public override void OnControllerHit(ControllersHit hit)
            {
                //throw new NotImplementedException();
            }

            public override void OnObstacleHit(ControllerObstacleHit hit)
            {
                //throw new NotImplementedException();
            }

            public override void OnShapeHit(ControllerShapeHit hit)
            {
                if (hit.Shape.Actor.IsDynamic == false)
                    return;

                var dynamicActor = hit.Shape.Actor as RigidDynamic;

                dynamicActor.AddForceAtPosition(hit.WorldNormal * 100, hit.WorldPosition);
            }
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
