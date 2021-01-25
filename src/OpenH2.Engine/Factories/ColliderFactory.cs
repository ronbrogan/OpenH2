using OpenH2.Core.Maps.Vista;
using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Foundation.Physics;
using OpenH2.Physics.Colliders;
using System;
using System.Collections.Generic;
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

            var colliderMeshes = new List<Vector3[]>();
            var convexModelMaterial = -1;

            foreach (var mesh in phmo.PolyhedraDefinitions)
            {
                var meshVerts = new List<Vector3>();

                for(var i = 0; i < mesh.CountA; i++)
                {
                    var x1 = mesh.FloatsC[i * 12 + 0];
                    var x2 = mesh.FloatsC[i * 12 + 1];
                    var x3 = mesh.FloatsC[i * 12 + 2];
                    var x4 = mesh.FloatsC[i * 12 + 3];

                    // TODO: figure out inidcator on how many polyhedra to read
                    if (x1 + x2 + x3 + x4 == 0) break;

                    var y1 = mesh.FloatsC[i * 12 + 4];
                    var y2 = mesh.FloatsC[i * 12 + 5];
                    var y3 = mesh.FloatsC[i * 12 + 6];
                    var y4 = mesh.FloatsC[i * 12 + 7];

                    var z1 = mesh.FloatsC[i * 12 + 8];
                    var z2 = mesh.FloatsC[i * 12 + 9];
                    var z3 = mesh.FloatsC[i * 12 + 10];
                    var z4 = mesh.FloatsC[i * 12 + 11];

                    meshVerts.Add(new Vector3(x1, y1, z1));
                    meshVerts.Add(new Vector3(x2, y2, z2));
                    meshVerts.Add(new Vector3(x3, y3, z3));
                    meshVerts.Add(new Vector3(x4, y4, z4));
                }

                if(meshVerts.Count > 0)
                {
                    colliderMeshes.Add(meshVerts.ToArray());
                    convexModelMaterial = mesh.MaterialIndexMaybe;
                }
            }

            foreach(var p in phmo.PolyhedraAlternativeDefinitions)
            {
                var meshVerts = new List<Vector3>();

                var x1 = p.Floats[0];
                var x2 = p.Floats[1];
                var x3 = p.Floats[2];
                var x4 = p.Floats[3];
                         
                var y1 = p.Floats[4];
                var y2 = p.Floats[5];
                var y3 = p.Floats[6];
                var y4 = p.Floats[7];
                         
                var z1 = p.Floats[8];
                var z2 = p.Floats[9];
                var z3 = p.Floats[10];
                var z4 = p.Floats[11];

                meshVerts.Add(new Vector3(x1, y1, z1));
                meshVerts.Add(new Vector3(x2, y2, z2));
                meshVerts.Add(new Vector3(x3, y3, z3));
                meshVerts.Add(new Vector3(x4, y4, z4));

                //colliderMeshes.Add(meshVerts.ToArray());
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
                boxCollider.PhysicsMaterial = box.MaterialIndexMaybe;
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
