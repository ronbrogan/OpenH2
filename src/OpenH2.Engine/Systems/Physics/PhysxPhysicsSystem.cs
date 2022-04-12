using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using OpenH2.Core.Architecture;
using OpenH2.Engine.Components;
using OpenH2.Engine.Components.Globals;
using OpenH2.Engine.Stores;
using OpenH2.Engine.Systems.Movement;
using OpenH2.Foundation.Physics;
using OpenH2.Physics.Colliders;
using OpenH2.Physics.Core;
using OpenH2.Physics.Proxying;
using OpenH2.Physx.Extensions;
using OpenH2.Physx.Proxies;
using PhysX;
using PhysX.VisualDebugger;
using ErrorCode = PhysX.ErrorCode;
using PxFoundation = PhysX.Foundation;
using PxPhysics = PhysX.Physics;
using PxScene = PhysX.Scene;
using PxTolerancesScale = PhysX.TolerancesScale;

namespace OpenH2.Engine.Systems.Physics
{
    public class PhysxPhysicsSystem : WorldSystem
    {
        private PxFoundation physxFoundation;
        private PxPhysics physxPhysics;
        private PxScene physxScene;
        private PxTolerancesScale physxScale;
        private ControllerManager controllerManager;
        private Cooking cooker;
        private Pvd pvd;

        private Material defaultMat;
        private Material characterControlMat;
        private Material frictionlessMat;
        private MaterialListComponent materialList;
        private Material[] globalMaterials;
        private Dictionary<int, Material> adhocMaterials = new Dictionary<int, Material>();

        private Dictionary<MoverComponent, Controller> ControllerMap = new Dictionary<MoverComponent, Controller>();
        private PhysicsForceQueue queuedForces = new PhysicsForceQueue();
        private IContactCreateProvider contactProvider;

        private InputStore input;
        public bool StepMode = false;
        public bool ShouldStep = false;
        private SimulationCallback simCallback;

        public PhysxPhysicsSystem(World world) : base(world)
        {
            // Setup PhysX infra here

            physxFoundation = new PxFoundation(new ConsoleErrorCallback());
            physxScale = new PxTolerancesScale()
            {
                Length = 0.1f,
                Speed = 98.1f
            };

#if DEBUG
            pvd = new Pvd(physxFoundation);
            pvd.Connect("localhost", 5425, TimeSpan.FromSeconds(2), InstrumentationFlag.Debug);
            physxPhysics = new PxPhysics(physxFoundation, physxScale, false, pvd);
#else
            this.physxPhysics = new PxPhysics(this.physxFoundation, this.physxScale);
#endif
            defaultMat = physxPhysics.CreateMaterial(0.5f, 0.5f, 0.1f);

            characterControlMat = physxPhysics.CreateMaterial(0.5f, 0.5f, 0f);
            characterControlMat.RestitutionCombineMode = CombineMode.Minimum;

            frictionlessMat = physxPhysics.CreateMaterial(0f, 0f, 0f);
            frictionlessMat.RestitutionCombineMode = CombineMode.Minimum;
            frictionlessMat.Flags = MaterialFlags.DisableFriction;

            var sceneDesc = new SceneDesc(physxScale)
            {
                BroadPhaseType = BroadPhaseType.SweepAndPrune,
                Gravity = world.Gravity,
                Flags = SceneFlag.EnableStabilization,
                SolverType = SolverType.TGS,
                FilterShader = new DefaultFilterShader(),
                BounceThresholdVelocity = 0.2f * world.Gravity.Length()
            };

            physxScene = physxPhysics.CreateScene(sceneDesc);

#if DEBUG
            physxScene.SetVisualizationParameter(VisualizationParameter.ContactPoint, true);
            physxScene.SetVisualizationParameter(VisualizationParameter.ContactNormal, true);
            physxScene.SetVisualizationParameter(VisualizationParameter.ContactForce, true);
            physxScene.SetVisualizationParameter(VisualizationParameter.ContactError, true);

            var pvdClient = physxScene.GetPvdSceneClient();
            pvdClient.SetScenePvdFlags(SceneVisualizationFlags.TransmitContacts | SceneVisualizationFlags.TransmitConstraints | SceneVisualizationFlags.TransmitSceneQueries);
#endif

            var cookingParams = new CookingParams()
            {
                Scale = physxScale,
                AreaTestEpsilon = 0.001f,
                MidphaseDesc = new MidphaseDesc()
                {
                    Bvh33Desc = new Bvh33MidphaseDesc()
                    {
                        MeshCookingHint = MeshCookingHint.SimulationPerformance
                    }
                },
                BuildTriangleAdjacencies = true,
                MeshCookingHint = MeshCookingHint.SimulationPerformance,
                MeshWeldTolerance = 0.001f
            };

            cooker = physxPhysics.CreateCooking(cookingParams);
            controllerManager = physxScene.CreateControllerManager();

            var contactProv = new ContactModifyProxy();
            contactProvider = contactProv;
            physxScene.ContactModifyCallback = contactProv;

            simCallback = new SimulationCallback();
            physxScene.SetSimulationEventCallback(simCallback);
        }

        public override void Initialize(Core.Architecture.Scene scene)
        {
            // Setup materials from globals
            adhocMaterials.Clear();
            materialList = world.Components<MaterialListComponent>().FirstOrDefault();
            Debug.Assert(materialList != null);
            var allMaterials = materialList.GetPhysicsMaterials();

            globalMaterials = new Material[allMaterials.Length + 1];
            globalMaterials[0] = defaultMat;
            Debug.Assert(allMaterials.Length > allMaterials.Max(m => m.Id));

            foreach (var mat in allMaterials)
            {
                GetOrCreateMaterial(mat.Id);
            }

            // Cook terrain and static geom meshes
            var terrains = world.Components<StaticTerrainComponent>();

            foreach (var terrain in terrains)
            {
                AddTerrain(terrain);
            }

            var sceneries = world.Components<StaticGeometryComponent>();
            foreach (var scenery in sceneries)
            {
                AddStaticGeom(scenery);
            }

            var rigidBodies = world.Components<RigidBodyComponent>();
            foreach (var body in rigidBodies)
            {
                AddRigidBodyComponent(body);
            }

            var movers = world.Components<MoverComponent>();
            foreach (var mover in movers)
            {
                AddCharacterController(mover);
            }

            var triggers = world.Components<TriggerGeometryComponent>();
            foreach (var trigger in triggers)
            {
                AddTrigger(trigger);
            }

            scene.OnEntityAdd += AddEntity;
            scene.OnEntityRemove += RemoveEntity;
        }


        private const float stepSize = 1f / 100f;
        private double totalTime = 0d;
        private double simulatedTime = 0d;
        public override void Update(double timestep)
        {
            totalTime += timestep;

            // Take fixed-size steps until we've caught up with reality
            while (totalTime - simulatedTime > stepSize)
            {
                simulatedTime += stepSize;

                TakeStep(stepSize);
            }
        }

        private void TakeStep(float step)
        {
            // Split simulation to allow modification of current simulation step
            physxScene.Collide(step);
            physxScene.FetchCollision(block: true);

            // Need to apply any forces here, between FetchCollision and Advance.
            // However, contacts aren't available here - we'll need to record contacts or something during ContactModify via the Collide phase
            foreach (var (body, velocity) in queuedForces.VelocityChanges)
            {
                body.AddVelocity(velocity);
            }

            foreach (var (body, force) in queuedForces.ForceChanges)
            {
                body.AddForce(force);
            }

            queuedForces.Clear();

            physxScene.Advance();
            physxScene.FetchResults(block: true);

            // Update all engine transforms
            foreach (var actor in physxScene.RigidDynamicActors)
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

            foreach (var controller in controllerManager.Controllers)
            {
                if (controller.Actor.UserData is MoverComponent mover)
                {
                    mover.Transform.Position = controller.Position;
                    mover.Transform.UpdateDerivedData();
                }
            }

            // TODO: track touch found/lost somwhere
            foreach (var triggerSet in simCallback.TriggerEventSets)
            {
                foreach (var triggerEvent in triggerSet)
                {
                    var comp = triggerEvent.TriggerActor.UserData as TriggerGeometryComponent;
                    Debug.Assert(comp != null);

                    Console.WriteLine($"[TRIG] {comp.Name} <{triggerEvent.Status}> {triggerEvent.OtherActor.UserData.ToString()}");
                }
            }

            simCallback.TriggerEventSets.Clear();
        }

        public void AddEntity(Entity entity)
        {
            var rigidBodies = entity.GetChildren<RigidBodyComponent>();
            foreach (var body in rigidBodies)
            {
                AddRigidBodyComponent(body);
            }

            if (entity.TryGetChild<MoverComponent>(out var mover))
            {
                AddCharacterController(mover);
            }

            foreach (var trigger in entity.GetChildren<TriggerGeometryComponent>())
            {
                AddTrigger(trigger);
            }

            if (entity.TryGetChild<StaticTerrainComponent>(out var terrain))
            {
                AddTerrain(terrain);
            }

            if (entity.TryGetChild<StaticGeometryComponent>(out var geom))
            {
                AddStaticGeom(geom);
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

            if (entity.TryGetChild<TriggerGeometryComponent>(out var trigger))
            {
                RemoveTrigger(trigger);
            }

            if (entity.TryGetChild<StaticTerrainComponent>(out var terrain))
            {
                RemoveTerrain(terrain);
            }

            if (entity.TryGetChild<StaticGeometryComponent>(out var geom))
            {
                RemoveStaticGeom(geom);
            }
        }

        public void AddRigidBodyComponent(RigidBodyComponent component)
        {
            if (component.PhysicsImplementation is not RigidDynamic dynamic)
            {
                dynamic = physxPhysics.CreateRigidDynamic(component.Transform.TransformationMatrix);
                dynamic.CenterOfMassLocalPose = Matrix4x4.CreateTranslation(component.CenterOfMass);
                dynamic.MassSpaceInertiaTensor = MathUtil.Diagonalize(component.InertiaTensor);
                dynamic.Mass = component.Mass;
                component.PhysicsImplementation = new RigidBodyProxy(dynamic);

                if (component.IsDynamic == false)
                {
                    dynamic.RigidBodyFlags = RigidBodyFlag.Kinematic;
                }

                dynamic.UserData = component;
                dynamic.Name = component.Parent.FriendlyName ?? component.Parent.Id.ToString();

                AddCollider(dynamic, component.Collider);
            }

            physxScene.AddActor(dynamic);

            if (component.IsDynamic)
            {
                dynamic.PutToSleep();
            }
        }

        // TODO: Currently, this system is responsible for creating and setting PhysicsImplementation properties
        //   -issue: PhysicsImplementations can't be passed/delegated before this system is initialized, as the props are unset
        //   -motive: DynamicMovementController wants to be able to setup callbacks before any physics events happen
        public void AddCharacterController(MoverComponent component)
        {
            var config = component.Config;

            var radius = 0.175f;

            // TODO: reduce duplicated code
            if (component.Mode == MoverComponent.MovementMode.Freecam)
            {
                var posPose = Matrix4x4.CreateTranslation(component.Transform.TransformationMatrix.Translation);
                var rot = Matrix4x4.CreateRotationY(MathF.PI / -2f);

                var body = physxPhysics.CreateRigidDynamic(rot * posPose);
                body.MassSpaceInertiaTensor = new Vector3(0, 0, 0);
                body.Mass = 175f;
                body.UserData = component;
                body.RigidBodyFlags = RigidBodyFlag.Kinematic;

                var capsuleDesc = new CapsuleGeometry(radius, config.Height / 2f - radius);

                var shape = RigidActorExt.CreateExclusiveShape(body, capsuleDesc, characterControlMat);
                // TODO: centralize filter data construction
                shape.SimulationFilterData = new FilterData((uint)(OpenH2FilterData.NoClip | OpenH2FilterData.PlayerCharacter), 0, 0, 0);

                shape.ContactOffset = 0.001f;
                shape.RestOffset = 0.0009f;

                var bodyProxy = new RigidBodyProxy(body);
                component.PhysicsImplementation = bodyProxy;
                physxScene.AddActor(body);
            }
            if (component.Mode == MoverComponent.MovementMode.KinematicCharacterControl)
            {
                var desc = new CapsuleControllerDesc()
                {
                    Height = config.Height - .02f - 2 * radius,
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

                var controller = controllerManager.CreateController<CapsuleController>(desc);

                controller.Actor.UserData = component;
                component.PhysicsImplementation = new KinematicCharacterControllerProxy(controller);

                ControllerMap.Add(component, controller);
            }
            else if (component.Mode == MoverComponent.MovementMode.DynamicCharacterControl)
            {
                var posPose = Matrix4x4.CreateTranslation(component.Transform.TransformationMatrix.Translation);
                var rot = Matrix4x4.CreateRotationY(MathF.PI / -2f);

                var body = physxPhysics.CreateRigidDynamic(rot * posPose);
                body.MassSpaceInertiaTensor = new Vector3(0, 0, 0);
                body.Mass = 175f;
                body.UserData = component;

                var capsuleDesc = new CapsuleGeometry(radius, config.Height / 2f - radius);

                var shape = RigidActorExt.CreateExclusiveShape(body, capsuleDesc, characterControlMat);
                // TODO: centralize filter data construction
                shape.SimulationFilterData = new FilterData((uint)OpenH2FilterData.PlayerCharacter, 0, 0, 0);

                shape.ContactOffset = 0.001f;
                shape.RestOffset = 0.0009f;

                var bodyProxy = new RigidBodyProxy(body);
                component.PhysicsImplementation = bodyProxy;
                physxScene.AddActor(body);

                if (component.State is DynamicMovementController dynamicMover)
                {
                    var contactInfo = ContactCallbackData.Normal;
                    contactProvider.RegisterContactCallback(body, contactInfo, dynamicMover.ContactFound);
                }
            }
        }

        public void AddTrigger(TriggerGeometryComponent component)
        {
            var halfSize = component.Size / 2f;
            var posPose = Matrix4x4.CreateTranslation(component.Transform.TransformationMatrix.Translation);
            var rot = Matrix4x4.CreateFromQuaternion(component.Transform.Orientation);
            var posCorrection = Matrix4x4.CreateTranslation(halfSize);

            var body = physxPhysics.CreateRigidStatic(posCorrection * rot * posPose);
            body.Name = component.Name;

            Geometry volume = component.Shape switch
            {
                TriggerGeometryShape.Cuboid => new BoxGeometry(halfSize),
            };

            var shape = RigidActorExt.CreateExclusiveShape(body, volume, defaultMat, ShapeFlag.TriggerShape);
            shape.SimulationFilterData = new FilterData((uint)OpenH2FilterData.TriggerVolume, 0, 0, 0);

            body.UserData = component;
            physxScene.AddActor(body);
        }

        private void AddStaticGeom(StaticGeometryComponent geom)
        {
            if (geom.PhysicsActor is RigidStatic existingRigid)
            {
                physxScene.AddActor(existingRigid);
                return;
            }

            var rigid = physxPhysics.CreateRigidStatic(geom.Transform.TransformationMatrix);

            AddCollider(rigid, geom.Collider);

            physxScene.AddActor(rigid);
            geom.PhysicsActor = rigid;
        }

        private void AddTerrain(StaticTerrainComponent terrain)
        {
            if (terrain.PhysicsActor is RigidStatic existingRigid)
            {
                physxScene.AddActor(existingRigid);
                return;
            }

            var rigid = physxPhysics.CreateRigidStatic();

            AddCollider(rigid, terrain.Collider);

            physxScene.AddActor(rigid);
            terrain.PhysicsActor = rigid;
        }

        private void RemoveStaticGeom(StaticGeometryComponent geom)
        {
            if (geom.PhysicsActor is RigidStatic rigid)
            {
                physxScene.RemoveActor(rigid);
            }
        }

        private void RemoveTerrain(StaticTerrainComponent terrain)
        {
            if (terrain.PhysicsActor is RigidStatic rigid)
            {
                physxScene.RemoveActor(rigid);
            }
        }

        public void RemoveRigidBodyComponent(RigidBodyComponent component)
        {
            if (component.PhysicsImplementation is RigidBodyProxy p)
            {
                physxScene.RemoveActor(p.RigidBody);
            }
        }

        private void RemoveCharacterController(MoverComponent mover)
        {
            if (ControllerMap.TryGetValue(mover, out var ctrl))
            {
                //?
            }
        }

        private void RemoveTrigger(TriggerGeometryComponent comp)
        {
            // TODO: implement cleaning up triggers
        }

        private void AddCollider(RigidActor actor, ICollider collider)
        {
            if (collider is AggregateCollider agg)
            {
                foreach (var c in agg.ColliderComponents)
                    AddCollider(actor, c);
            }
            else if (collider is TriangleMeshCollider triCollider)
            {
                var desc = triCollider.GetDescriptor(GetMaterialIndices);

                // Avoiding offline cook path for now because
                //   1. Comments in Physx.Net imply memory leak using streams
                //   2. I don't want to deal with disk caching cooks yet
                var finalMesh = physxPhysics.CreateTriangleMesh(cooker, desc);

                var meshGeom = new TriangleMeshGeometry(finalMesh);

                RigidActorExt.CreateExclusiveShape(actor, meshGeom, globalMaterials, null);
            }
            else if (collider is TriangleModelCollider triModelCollider)
            {
                foreach (var mesh in triModelCollider.MeshColliders)
                {
                    var desc = mesh.GetDescriptor(GetMaterialIndices);

                    // Avoiding offline cook path for now because
                    //   1. Comments in Physx.Net imply memory leak using streams
                    //   2. I don't want to deal with disk caching cooks yet
                    var finalMesh = physxPhysics.CreateTriangleMesh(cooker, desc);

                    var meshGeom = new TriangleMeshGeometry(finalMesh);

                    RigidActorExt.CreateExclusiveShape(actor, meshGeom, globalMaterials, null);
                }
            }
            else if (collider is IVertexBasedCollider vertCollider)
            {
                var desc = new ConvexMeshDesc() { Flags = ConvexFlag.ComputeConvex };
                desc.SetPositions(vertCollider.GetTransformedVertices());
                var mesh = physxPhysics.CreateConvexMesh(cooker, desc);
                var geom = new ConvexMeshGeometry(mesh);
                var mat = GetOrCreateMaterial(vertCollider.PhysicsMaterial);
                // TODO: re-use shared shapes instead of creating exclusive
                RigidActorExt.CreateExclusiveShape(actor, geom, mat);
            }
            else if (collider is ConvexModelCollider modelCollider)
            {
                foreach (var verts in modelCollider.Meshes)
                {
                    var desc = new ConvexMeshDesc() { Flags = ConvexFlag.ComputeConvex };
                    desc.SetPositions(verts);
                    var mesh = physxPhysics.CreateConvexMesh(cooker, desc);
                    var geom = new ConvexMeshGeometry(mesh);
                    var mat = GetOrCreateMaterial(modelCollider.PhysicsMaterial);
                    // TODO: re-use shared shapes instead of creating exclusive
                    RigidActorExt.CreateExclusiveShape(actor, geom, mat);
                }
            }
        }

        private Material GetOrCreateMaterial(int id)
        {
            // This method accounts for having defaultMat at location 0 of the globalMaterials array
            // Because of this, any id check against the array must be incremented by 1 and checked appropriately

            Material mat;

            // If it's a valid index and the expanded mats array can hold it, and it's not null, use it
            if (id >= 0 && globalMaterials.Length - 1 > id && globalMaterials[id + 1] != null)
                return globalMaterials[id + 1];
            else if (adhocMaterials.TryGetValue(id, out mat))
                return mat;

            // Get original def with raw id
            var matDef = materialList?.GetPhysicsMaterial(id);

            if (matDef == null)
                return defaultMat;

            mat = physxPhysics.CreateMaterial(matDef.StaticFriction, matDef.DynamicFriction, matDef.Restitution);

            // If it's a valid index and the expanded mats array can hold it, set it
            if (id >= 0 && globalMaterials.Length - 1 > id)
                globalMaterials[id + 1] = mat;
            else
                adhocMaterials[id] = mat;

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
