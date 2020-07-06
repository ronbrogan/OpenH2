using OpenH2.Core.Architecture;
using OpenH2.Engine.Components;
using OpenH2.Engine.Components.Globals;
using OpenH2.Engine.Stores;
using OpenH2.Foundation.Physics;
using OpenH2.Physics.Colliders;
using OpenH2.Physics.Proxying;
using OpenH2.Physx.Proxies;
using OpenToolkit.Windowing.Common.Input;
using PhysX;
using PhysX.VisualDebugger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using PxFoundation = PhysX.Foundation;
using PxPhysics = PhysX.Physics;
using PxScene = PhysX.Scene;

namespace OpenH2.Engine.Systems
{
    public class PhysxPhysicsSystem : WorldSystem
    {
        private PxFoundation physxFoundation;
        private PxPhysics physxPhysics;
        private PxScene physxScene;
        private ControllerManager controllerManager;
        private Cooking cooker;
        private Pvd pvd;

        private Material defaultMat;
        private Material invisWallMat;
        private Material frictionlessMat;
        private MaterialListComponent materialList;
        private Material[] globalMaterials;
        private Dictionary<int, Material> adhocMaterials = new Dictionary<int, Material>();
        private Dictionary<MoverComponent, Controller> ControllerMap = new Dictionary<MoverComponent, Controller>();
        
        private InputStore input;
        public bool StepMode = false;
        public bool ShouldStep = false;

        public PhysxPhysicsSystem(World world) : base(world)
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
            this.invisWallMat= this.physxPhysics.CreateMaterial(0f, 0f, 0f);
            this.frictionlessMat = this.physxPhysics.CreateMaterial(0f, 0f, 0f);
            this.frictionlessMat.RestitutionCombineMode = CombineMode.Minimum;
            this.frictionlessMat.Flags = MaterialFlags.DisableFriction;

            var sceneDesc = new SceneDesc()
            {
                BroadPhaseType = BroadPhaseType.SweepAndPrune,
                Gravity = world.Gravity,
                Flags = SceneFlag.EnableStabilization,
                SolverType = SolverType.TGS,
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
            // Setup materials from globals
            this.adhocMaterials.Clear();
            this.materialList = world.Components<MaterialListComponent>().FirstOrDefault();
            Debug.Assert(this.materialList != null);
            var allMaterials = this.materialList.GetPhysicsMaterials();

            this.globalMaterials = new Material[allMaterials.Length + 1];
            this.globalMaterials[0] = this.defaultMat;
            Debug.Assert(allMaterials.Length > allMaterials.Max(m => m.Id));

            foreach(var mat in allMaterials)
            {
                this.GetOrCreateMaterial(mat.Id);
            }

            // Cook terrain and static geom meshes
            var terrains = world.Components<StaticTerrainComponent>();

            foreach (var terrain in terrains)
            {
                var meshDesc = new TriangleMeshDesc()
                {
                    Points = terrain.Vertices,
                    Triangles = terrain.TriangleIndices,
                    MaterialIndices = GetMaterialIndices(terrain.MaterialIndices)
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
                    Triangles = scenery.TriangleIndices,
                    MaterialIndices = GetMaterialIndices(scenery.MaterialIndices)
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

            scene.OnEntityAdd += this.AddEntity;
            scene.OnEntityRemove += this.RemoveEntity;
        }

        public override void Update(double timestep)
        {
            if (TakeStep() == false)
                return;

            // Simulate, fetch results
            this.physxScene.Simulate((float)timestep);
            this.physxScene.FetchResults(true);

            // Update all engine transforms
            foreach (var actor in this.physxScene.RigidDynamicActors)
            {
                if (actor.UserData is RigidBodyComponent rigid)
                {
                    rigid.Transform.UseTransformationMatrix(actor.GlobalPose);
                }
                else if (actor.UserData is MoverComponent mover)
                {
                    mover.Transform.Position = actor.GlobalPosePosition;
                    mover.Transform.UpdateDerivedData();
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

        public void AddEntity(Entity entity)
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

        private void RemoveEntity(Entity entity)
        {
            if (entity.TryGetChild<RigidBodyComponent>(out var rigidBody))
            {
                RemoveRigidBodyComponent(rigidBody);
            }

            if (entity.TryGetChild<MoverComponent>(out var mover))
            {
                RemoveCharacterController(mover);
            }
        }

        

        public void RemoveRigidBodyComponent(RigidBodyComponent component)
        {
            if (component.PhysicsImplementation is RigidBodyProxy p)
            {
                this.physxScene.RemoveActor(p.RigidBody);
            }
        }

        private void RemoveCharacterController(MoverComponent mover)
        {
            if(ControllerMap.TryGetValue(mover, out var ctrl))
            {
                //?
            }
        
        }

        public void AddRigidBodyComponent(RigidBodyComponent component)
        {
            RigidActor body;

            if(component.IsDynamic)
            {
                var dynamic = this.physxPhysics.CreateRigidDynamic(component.Transform.TransformationMatrix);
                dynamic.CenterOfMassLocalPose = Matrix4x4.CreateTranslation(component.CenterOfMass);
                dynamic.MassSpaceInertiaTensor = MathUtil.Diagonalize(component.InertiaTensor);
                dynamic.Mass = component.Mass;
                component.PhysicsImplementation = new RigidBodyProxy(dynamic);
                body = dynamic;
            }
            else
            {
                var stat = this.physxPhysics.CreateRigidStatic(component.Transform.TransformationMatrix);
                component.PhysicsImplementation = NullPhysicsProxy.Instance;

                body = stat;
            }

            body.UserData = component;
            body.Name = component.Parent.Id.ToString();

            if (component.Collider is IVertexBasedCollider vertCollider)
            {
                var desc = new ConvexMeshDesc() { Flags = ConvexFlag.ComputeConvex };
                desc.SetPositions(vertCollider.Vertices);
                var mesh = this.physxPhysics.CreateConvexMesh(this.cooker, desc);
                var geom = new ConvexMeshGeometry(mesh);
                var mat = this.GetOrCreateMaterial(-1);
                // TODO: re-use shared shapes instead of creating exclusive
                RigidActorExt.CreateExclusiveShape(body, geom, mat);
            }
            else if(component.Collider is ConvexModelCollider modelCollider)
            {
                foreach(var verts in modelCollider.Meshes)
                {
                    var desc = new ConvexMeshDesc() { Flags = ConvexFlag.ComputeConvex };
                    desc.SetPositions(verts);
                    var mesh = this.physxPhysics.CreateConvexMesh(this.cooker, desc);
                    var geom = new ConvexMeshGeometry(mesh);
                    var mat = this.GetOrCreateMaterial(-1);
                    // TODO: re-use shared shapes instead of creating exclusive
                    RigidActorExt.CreateExclusiveShape(body, geom, mat);
                }
            }

            this.physxScene.AddActor(body);
        }

        // TODO: Currently, this system is responsible for creating and setting PhysicsImplementation properties
        //   -issue: PhysicsImplementations can't be passed/delegated before this system is initialized, as the props are unset
        //   -motive: DynamicMovementController wants to be able to setup callbacks before any physics events happen
        public void AddCharacterController(MoverComponent component)
        {
            var config = component.Config;

            var radius = 0.175f;

            if (component.Mode == MoverComponent.MovementMode.KinematicCharacterControl)
            {
                

                var desc = new CapsuleControllerDesc()
                {
                    Height = config.Height-.02f - (2 * radius),
                    Position = component.Transform.Position,
                    Radius = radius,
                    MaxJumpHeight = 1f,
                    UpDirection = EngineGlobals.Up,
                    SlopeLimit = MathF.Cos(0.872665f), // cos(50 degrees)
                    StepOffset = 0.1f,
                    Material = defaultMat,
                    ContactOffset = 0.0001f,
                    ReportCallback = new CustomHitReport()
                };

                var controller = this.controllerManager.CreateController<CapsuleController>(desc);

                controller.Actor.UserData = component;
                component.PhysicsImplementation = new KinematicCharacterControllerProxy(controller);

                this.ControllerMap.Add(component, controller);
            }
            else if(component.Mode == MoverComponent.MovementMode.DynamicCharacterControl)
            {
                var posPose = Matrix4x4.CreateTranslation(component.Transform.TransformationMatrix.Translation);
                var rot = Matrix4x4.CreateRotationY(MathF.PI / -2f);

                var body = this.physxPhysics.CreateRigidDynamic(rot * posPose);
                body.MassSpaceInertiaTensor = new Vector3(0, 0, 0);
                body.Mass = 175f;
                body.UserData = component;

                var capsuleDesc = new CapsuleGeometry(radius, config.Height / 2f - radius);
                
                var shape = RigidActorExt.CreateExclusiveShape(body, capsuleDesc, defaultMat);

                shape.ContactOffset = 0.1f;
                shape.RestOffset = 0.09f;

                component.PhysicsImplementation = new RigidBodyProxy(body);
                this.physxScene.AddActor(body);
            }
        }

        private bool TakeStep()
        {
            if (input == null)
                input = this.world.GetGlobalResource<InputStore>();

            if (input.Keyboard.IsKeyDown(Key.P) && input.Keyboard.IsKeyDown(Key.ShiftRight))
                StepMode = !StepMode;

            if (StepMode)
            {
                ShouldStep = input.Keyboard.IsKeyDown(Key.P);

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
            RigidActorExt.CreateExclusiveShape(rigid, meshGeom, globalMaterials, null);
            return rigid;
        }

        private Material GetOrCreateMaterial(int id)
        {
            // This method accounts for having defaultMat at location 0 of the globalMaterials array
            // Because of this, any id check against the array must be incremented by 1 and checked appropriately

            Material mat;

            // If it's a valid index and the expanded mats array can hold it, and it's not null, use it
            if (id >= 0 && this.globalMaterials.Length-1 > id && this.globalMaterials[id+1] != null)
                return this.globalMaterials[id+1];
            else if (this.adhocMaterials.TryGetValue(id, out mat))
                return mat;

            // Get original def with raw id
            var matDef = this.materialList?.GetPhysicsMaterial(id);

            if (matDef == null)
                return this.defaultMat;

            mat = this.physxPhysics.CreateMaterial(matDef.StaticFriction, matDef.DynamicFriction, matDef.Restitution);

            // If it's a valid index and the expanded mats array can hold it, set it
            if (id >= 0 && this.globalMaterials.Length-1 > id)
                this.globalMaterials[id+1] = mat;
            else
                this.adhocMaterials[id] = mat;

            return mat;
        }

        private short[] GetMaterialIndices(int[] array)
        {
            // Add 1 to the index (account for default mat at 0)
            // and cast to short for PhysX
            return array.Select(i => (short)(i + 1)).ToArray();
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

                dynamicActor.AddForceAtPosition(hit.WorldNormal, hit.WorldPosition, ForceMode.Impulse, true);
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
