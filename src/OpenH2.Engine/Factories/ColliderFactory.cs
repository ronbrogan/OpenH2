using OpenH2.Core.Maps.Vista;
using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Foundation.Physics;
using OpenH2.Physics.Colliders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace OpenH2.Engine.Factories
{
    public static class ColliderFactory
    {
        private static ICollider DefaultCollider = new BoxCollider(IdentityTransform.Instance, new Vector3(0.1f));

        public static ICollider GetConvexColliderForHlmt(H2vMap map, HaloModelTag hlmt, int damageLevel = 0)
        {
            if (map.TryGetTag(hlmt.ColliderId, out var coll) == false)
            {
                // TODO: determine why collider can be invalid, if it's expected
                return DefaultCollider;
            }

            if (coll.ColliderComponents.Length == 0
                || coll.ColliderComponents[0].DamageLevels.Length <= damageLevel
                || coll.ColliderComponents[0].DamageLevels[damageLevel].Parts.Length == 0)
            {
                Console.WriteLine($"No colliders defined in coll[{hlmt.ColliderId}] for damage level {damageLevel}");
                return DefaultCollider;
            }

            var colliderMeshes = new List<Vector3[]>();

            foreach (var component in coll.ColliderComponents)
            {
                var container = component.DamageLevels[damageLevel];

                foreach(var info in container.Parts)
                {
                    colliderMeshes.Add(info.Vertices.Select(v => new Vector3(v.x, v.y, v.z)).ToArray());
                }
            }

            var collider = new ConvexModelCollider(colliderMeshes);

            return collider;
        }

        public static ICollider GetAggregateColliderForPhmo(H2vMap map, PhysicsModelTag phmo, int damageLevel = 0)
        {
            var collider = new AggregateCollider();
            var listAltIndex = 0;

            foreach (var body in phmo.RigidBodies)
            {
                switch (body.ComponentType)
                {
                    case PhysicsModelTag.RigidBodyComponentType.Capsule:
                        break;
                    case PhysicsModelTag.RigidBodyComponentType.Sphere:
                        break;
                    case PhysicsModelTag.RigidBodyComponentType.Box:
                        AddBoxCollider(collider, phmo.BoxDefinitions[body.ComponentIndex]);
                        break;
                    case PhysicsModelTag.RigidBodyComponentType.Triangles:
                        break;
                    case PhysicsModelTag.RigidBodyComponentType.Polyhedra:
                        AddPolyhedronCollider(collider, phmo, phmo.PolyhedraDefinitions[body.ComponentIndex]);
                        break;
                    case PhysicsModelTag.RigidBodyComponentType.FixedList:
                    case PhysicsModelTag.RigidBodyComponentType.ComponentList:
                        AddCompositeCollider(collider, phmo, body, ref listAltIndex);
                        break;
                    default:
                        break;
                }
            }

            return collider;
        }

        private static void AddBoxCollider(AggregateCollider aggregate, PhysicsModelTag.BoxDefinition box)
        {
            var xform = new Transform();
            xform.UseTransformationMatrix(box.Transform);
            var boxCollider = new BoxCollider(xform, box.HalfWidthsMaybe);
            boxCollider.PhysicsMaterial = box.Material;
            aggregate.AddCollider(boxCollider);
        }

        private static void AddPolyhedronCollider(AggregateCollider aggregate, PhysicsModelTag phmo, PhysicsModelTag.PolyhedraDefinition mesh)
        {
            var meshVerts = new List<Vector3>(mesh.HullCount * 4);

            ReadOnlySpan<TetrahedralHull> hulls = mesh.InlineHulls;

            if (mesh.HullCount > 3)
            {
                hulls = phmo.PolyhedraAlternativeDefinitions.AsSpan().Slice(mesh.ExternalHullsOffset, mesh.HullCount);
            }

            for (var i = 0; i < mesh.HullCount; i++)
            {
                var hull = hulls[i];
                meshVerts.Add(hull[0]);
                meshVerts.Add(hull[1]);
                meshVerts.Add(hull[2]);
                meshVerts.Add(hull[3]);
            }

            var convexModel = new ConvexModelCollider(new() { meshVerts.ToArray() });
            convexModel.PhysicsMaterial = mesh.Material;

            aggregate.AddCollider(convexModel);
        }

        private static void AddCompositeCollider(AggregateCollider aggregate, PhysicsModelTag phmo, PhysicsModelTag.RigidBody body, ref int listAltIndex)
        {
            var list = phmo.Lists[body.ComponentIndex];
            Span<PhysicsModelTag.ListShape> shapes;

            if (list.Count < 5)
            {
                shapes = new PhysicsModelTag.ListShape[list.Count];
                for(var i = 0; i < list.Count; i++)
                {
                    shapes[i] = new PhysicsModelTag.ListShape()
                    {
                        ComponentType = (PhysicsModelTag.RigidBodyComponentType)list.InlineShapes[i * 4 + 0],
                        ComponentIndex = list.InlineShapes[i * 4 + 2],
                    };
                }
            }
            else
            {
                shapes = phmo.ListShapes.AsSpan().Slice(list.ExternalShapesOffset, list.Count);
            }

            foreach(var shape in shapes)
            {
                switch (shape.ComponentType)
                {
                    case PhysicsModelTag.RigidBodyComponentType.Sphere:
                        break;
                    case PhysicsModelTag.RigidBodyComponentType.Capsule:
                        break;
                    case PhysicsModelTag.RigidBodyComponentType.Box:
                        AddBoxCollider(aggregate, phmo.BoxDefinitions[shape.ComponentIndex]);
                        break;
                    case PhysicsModelTag.RigidBodyComponentType.Polyhedra:
                        AddPolyhedronCollider(aggregate, phmo, phmo.PolyhedraDefinitions[shape.ComponentIndex]);
                        break;

                    default:
                    case PhysicsModelTag.RigidBodyComponentType.Triangles:
                    case PhysicsModelTag.RigidBodyComponentType.FixedList:
                    case PhysicsModelTag.RigidBodyComponentType.ComponentList:
                        throw new Exception("what");
                }
            }
        }

        public static ICollider GetTriangleColliderForHlmt(H2vMap map, HaloModelTag hlmt, int damageLevel = 0)
        {
            if(hlmt.ColliderId.IsInvalid)
            {
                return DefaultCollider;
            }

            if (map.TryGetTag(hlmt.ColliderId, out var coll) == false)
            {
                Console.WriteLine($"Couldn't find COLL[{hlmt.ColliderId.Id}]");
                return DefaultCollider;
            }

            if (coll.ColliderComponents.Length == 0
                || coll.ColliderComponents[0].DamageLevels.Length <= damageLevel
                || coll.ColliderComponents[0].DamageLevels[damageLevel].Parts.Length == 0)
            {
                Console.WriteLine($"No colliders defined in coll[{hlmt.ColliderId}] for damage level {damageLevel}");
                return DefaultCollider;
            }

            var colliderMeshes = new List<TriangleMeshCollider>();

            foreach (var component in coll.ColliderComponents)
            {
                var container = component.DamageLevels[damageLevel];

                // TODO material lookup
                colliderMeshes.Add(TriangleMeshCollider.Create(container.Parts, i => -1));
            }

            // TODO: relative transforms?
            var collider = new TriangleModelCollider()
            {
                MeshColliders = colliderMeshes.ToArray()
            };

            return collider;
        }
    }
}
