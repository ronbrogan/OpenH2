using OpenH2.Core.Architecture;
using OpenH2.Core.Maps;
using OpenH2.Core.Maps.Vista;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Common.Collision;
using OpenH2.Engine.Components;
using OpenH2.Foundation;
using OpenH2.Foundation.Physics;
using OpenH2.Physics.Colliders;
using System;
using System.Collections.Generic;
using System.Numerics;
using static OpenH2.Core.Tags.BspTag;

namespace OpenH2.Engine.Factories
{
    public static class PhysicsComponentFactory
    {
        public static RigidBodyComponent CreateDynamicRigidBody(Entity parent, TransformComponent transform, H2vMap map, TagRef<HaloModelTag> hlmtRef, int damageLevel = 0)
        {
            if(map.TryGetTag(hlmtRef, out var hlmt) == false)
            {
                throw new Exception($"Couldn't find HLMT[{hlmtRef.Id}]");
            }

            RigidBodyComponent body;


            if (map.TryGetTag(hlmt.PhysicsModel, out var phmo) && phmo.RigidBodies.Length > 0)
            {
                var param = phmo.RigidBodies[0];

                body = new RigidBodyComponent(parent, transform, param.InertiaTensor, param.Mass, param.CenterOfMass);
                body.Collider = ColliderFactory.GetAggregateColliderForPhmo(phmo);
            }
            else
            {
                body = new RigidBodyComponent(parent, transform);
            }            
            
            return body;
        }

        public static IEnumerable<(RigidBodyComponent, (Vector3, Quaternion))> CreateKinematicRigidBodies(Entity parent, Dictionary<string, (Vector3, Quaternion)> bones, ITransform root, PhysicsModelTag phmo)
        {
            foreach(var body in phmo.RigidBodies)
            {
                var transform = new Transform()
                {
                    Position = Vector3.Zero,
                    Orientation = Quaternion.Identity,
                    Scale = Vector3.One
                };

                if(bones.TryGetValue(phmo.Nodes[body.Node].Name, out var bone))
                {
                    transform.Position = bone.Item1;
                    transform.Orientation = bone.Item2;
                }

                transform.UpdateDerivedData();

                var parentedXform = new CompositeTransform(root, transform);

                yield return (new RigidBodyComponent(parent, parentedXform, body.InertiaTensor, body.Mass, body.CenterOfMass)
                {
                    IsDynamic = false,
                    Collider = ColliderFactory.GetAggregateColliderForPhmoBody(phmo, body)
                }, bone);
            }
        }


        public static StaticGeometryComponent CreateStaticRigidBody(Entity parent, TransformComponent transform, H2vMap map, TagRef<HaloModelTag> hlmtRef, int damageLevel = 0)
        {
            if (map.TryGetTag(hlmtRef, out var hlmt) == false)
            {
                return null;
            }

            return new StaticGeometryComponent(parent, transform)
            {
                Collider = ColliderFactory.GetTriangleColliderForHlmt(map, hlmt, damageLevel)
            };
        }

        public static StaticTerrainComponent CreateTerrain(Entity parent, ICollisionInfo[] collisionInfos, CollisionMaterial[] shaders)
        {
            return new StaticTerrainComponent(parent)
            {
                Collider = TriangleMeshCollider.Create(collisionInfos, MatLookup(shaders))
            };
        }

        public static StaticGeometryComponent CreateStaticGeometry(Entity parent, TransformComponent xform, ICollisionInfo collisionInfos, CollisionMaterial[] shaders)
        {
            return CreateStaticGeometry(parent, xform, new[] { collisionInfos }, shaders);
        }

        public static StaticGeometryComponent CreateStaticGeometry(Entity parent, TransformComponent xform, ICollisionInfo[] collisionInfos, CollisionMaterial[] shaders)
        {
            return new StaticGeometryComponent(parent, xform)
            {
                Collider = TriangleMeshCollider.Create(collisionInfos, MatLookup(shaders))
            };
        }

        private static Func<int,int> MatLookup(CollisionMaterial[] shaders)
        {
            return i =>
            {
                if (i < shaders.Length)
                {
                    var shader = shaders[i];
                    return shader.GlobalMaterialId;
                }
                
                return -1;
            };
        }
    }
}
