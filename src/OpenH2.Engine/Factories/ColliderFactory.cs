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

            if (phmo.TriangleDefinitions.Length > 0)
                Debugger.Break();

            var colliderMeshes = new List<Vector3[]>();
            var convexModelMaterial = -1;

            var polyhedraAltIndex = 0;

            foreach (var mesh in phmo.PolyhedraDefinitions)
            {
                var meshVerts = new List<Vector3>();

                if(mesh.HullCount <= 3)
                {
                    for (var i = 0; i < mesh.HullCount; i++)
                    {
                        var x = mesh.InlineHull_0_X;
                        var y = mesh.InlineHull_0_Y;
                        var z = mesh.InlineHull_0_Z;

                        if(i == 1)
                        {
                            x = mesh.InlineHull_1_X;
                            y = mesh.InlineHull_1_Y;
                            z = mesh.InlineHull_1_Z;
                        }
                        else if(i == 2)
                        {
                            x = mesh.InlineHull_2_X;
                            y = mesh.InlineHull_2_Y;
                            z = mesh.InlineHull_2_Z;
                        }

                        meshVerts.Add(new Vector3(x[0], y[0], z[0]));
                        meshVerts.Add(new Vector3(x[1], y[1], z[1]));
                        meshVerts.Add(new Vector3(x[2], y[2], z[2]));
                        meshVerts.Add(new Vector3(x[3], y[3], z[3]));
                    }
                }
                else
                {
                    for(var i = 0; i < mesh.HullCount; i++)
                    {
                        // No indexes seem available for this, accumulating the index over polyhedra instances
                        var hull = phmo.PolyhedraAlternativeDefinitions[polyhedraAltIndex++];

                        meshVerts.Add(new Vector3(hull.XValues[0], hull.YValues[0], hull.ZValues[0]));
                        meshVerts.Add(new Vector3(hull.XValues[1], hull.YValues[1], hull.ZValues[1]));
                        meshVerts.Add(new Vector3(hull.XValues[2], hull.YValues[2], hull.ZValues[2]));
                        meshVerts.Add(new Vector3(hull.XValues[3], hull.YValues[3], hull.ZValues[3]));
                    }
                }

                if(meshVerts.Count > 0)
                {
                    colliderMeshes.Add(meshVerts.ToArray());
                    convexModelMaterial = mesh.Material;
                }
            }

            if(colliderMeshes.Count > 0)
            {
                var convexModel = new ConvexModelCollider(colliderMeshes);
                convexModel.PhysicsMaterial = convexModelMaterial;

                collider.AddCollider(convexModel);
            }

            foreach(var box in phmo.BoxDefinitions)
            {
                var xform = new Transform();
                xform.UseTransformationMatrix(box.Transform);
                var boxCollider = new BoxCollider(xform, box.HalfWidthsMaybe);
                boxCollider.PhysicsMaterial = box.Material;
                collider.AddCollider(boxCollider);
            }

            return collider;
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
