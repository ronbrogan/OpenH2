using OpenH2.Core.Architecture;
using OpenH2.Core.Maps;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Common.Collision;
using OpenH2.Engine.Components;
using OpenH2.Physics.Colliders;
using System;
using System.Collections.Generic;
using System.Linq;
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


            if (map.TryGetTag(hlmt.PhysicsModel, out var phmo) && phmo.BodyParameters.Length > 0)
            {
                var param = phmo.BodyParameters[0];

                body = new RigidBodyComponent(parent, transform, param.InertiaTensor, param.Mass, param.CenterOfMass);
                body.Collider = ColliderFactory.GetConvexColliderForHlmt(map, hlmt, damageLevel);
            }
            else
            {
                body = new RigidBodyComponent(parent, transform);
            }            
            
            return body;
        }

        public static RigidBodyComponent CreateKinematicRigidBody(Entity parent, TransformComponent transform, H2vMap map, TagRef<HaloModelTag> hlmtRef, int damageLevel = 0)
        {
            if (map.TryGetTag(hlmtRef, out var hlmt) == false)
            {
                throw new Exception($"Couldn't find HLMT[{hlmtRef.Id}]");
            }

            RigidBodyComponent body;

            if (!map.TryGetTag(hlmt.PhysicsModel, out var phmo) || phmo.BodyParameters.Length == 0)
            {
                return null;
            }

            var param = phmo.BodyParameters[0];

            body = new RigidBodyComponent(parent, transform, param.InertiaTensor, param.Mass, param.CenterOfMass)
            {
                IsDynamic = false,
                Collider = ColliderFactory.GetTriangleColliderForHlmt(map, hlmt, damageLevel)
            };

            return body;
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

        public static StaticTerrainComponent CreateTerrain(Entity parent, ICollisionInfo[] collisionInfos, ShaderInfo[] shaders)
        {
            return new StaticTerrainComponent(parent)
            {
                Collider = TriangleMeshCollider.Create(collisionInfos, MatLookup(shaders))
            };
        }

        public static StaticGeometryComponent CreateStaticGeometry(Entity parent, TransformComponent xform, ICollisionInfo collisionInfos, ShaderInfo[] shaders)
        {
            return CreateStaticGeometry(parent, xform, new[] { collisionInfos }, shaders);
        }

        public static StaticGeometryComponent CreateStaticGeometry(Entity parent, TransformComponent xform, ICollisionInfo[] collisionInfos, ShaderInfo[] shaders)
        {
            return new StaticGeometryComponent(parent, xform)
            {
                Collider = TriangleMeshCollider.Create(collisionInfos, MatLookup(shaders))
            };
        }

        private static Func<int,int> MatLookup(ShaderInfo[] shaders)
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
