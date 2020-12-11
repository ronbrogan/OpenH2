using OpenH2.Core.Factories;
using OpenH2.Core.ExternalFormats;
using OpenH2.Core.Maps;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Common.Collision;
using OpenH2.Core.Tags.Common.Models;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using OpenH2.Core.Maps.Vista;

namespace OpenH2.ModelDumper
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Must provide 2 arguments: map path, and output directory");
                return;
            }

            var mapPath = args[0];
            var outPath = args[1];

            if (File.Exists(mapPath) == false)
            {
                Console.WriteLine($"Error: Could not find {mapPath}");
                return;
            }

            var factory = new MapFactory(Path.GetDirectoryName(mapPath));
            var h2map = factory.Load(Path.GetFileName(mapPath));

            if (h2map is not H2vMap scene)
            {
                throw new NotSupportedException("Only Vista maps are supported");
            }

            var writer = new WavefrontObjWriter(Path.GetFileNameWithoutExtension(mapPath));

            var bsps = scene.GetLocalTagsOfType<BspTag>();
            var bspMeshes = bsps
                .SelectMany(b => b.RenderChunks);

            foreach(var chunk in bspMeshes)
            { 
                //writer.WriteModel(chunk.Model, default, "bsp");
            }

            var instancedGeometries = bsps.SelectMany(b => b.InstancedGeometryInstances
                .Select(i => new { Bsp = b, Instance = i }));

            foreach (var geom in instancedGeometries)
            {
                var def = geom.Bsp.InstancedGeometryDefinitions[geom.Instance.Index];

                var xform = Matrix4x4.CreateScale(new Vector3(geom.Instance.Scale))
                    * Matrix4x4.CreateFromQuaternion(QuatFrom3x3Mat4(geom.Instance.RotationMatrix)) 
                    * Matrix4x4.CreateTranslation(geom.Instance.Position);

               // writer.WriteModel(def.Model, xform, "instanced_" + geom.Instance.Index);
            }

            var scenario = scene.GetLocalTagsOfType<ScenarioTag>().First();

            //foreach(var blocInstance in scenario.BlocInstances)
            //{
            //    var def = scenario.BlocDefinitions[blocInstance.BlocDefinitionIndex];

            //    var xform = Matrix4x4.CreateScale(new Vector3(1))
            //        * Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(blocInstance.Orientation.Y, blocInstance.Orientation.Z, blocInstance.Orientation.X))
            //        * Matrix4x4.CreateTranslation(blocInstance.Position);

            //    if(!scene.TryGetTag(def.Bloc, out var bloc))
            //    {
            //        continue;
            //    }

            //    if (!scene.TryGetTag(bloc.PhysicalModel, out var hlmt))
            //    {
            //        continue;
            //    }

            //    if (scene.TryGetTag(hlmt.RenderModel, out var mode))
            //    {
            //        var part = mode.Components[0].DamageLevels[0].HighestPieceIndex;

            //        writer.WriteModel(mode.Parts[part].Model, xform, "bloc_" + blocInstance.BlocDefinitionIndex);
            //    }

            //    if (scene.TryGetTag(hlmt.ColliderId, out var coll))
            //    {
            //        var meshes = new List<ModelMesh>();

            //        foreach (var comp in coll.ColliderComponents)
            //        {
            //            var level = comp.DamageLevels[0];
            //            //foreach(var level in comp.DamageLevels)
            //            {
            //                var triMesh = GetTriangulatedCollisionMesh(level.Parts);

            //                meshes.Add(new ModelMesh()
            //                {
            //                    Indices = triMesh.Item2.ToArray(),
            //                    Verticies = triMesh.Item1.Select(v => new VertexFormat(v, Vector2.Zero, Vector3.Zero)).ToArray(),
            //                    ElementType = Foundation.MeshElementType.TriangleList
            //                });
            //            }
            //        }


            //        var model = new MeshCollection(meshes.ToArray());
            //        writer.WriteModel(model, xform, "bloc_" + blocInstance.BlocDefinitionIndex + "_coll");
            //    }
            //}

            //foreach (var scenInstance in scenario.SceneryInstances)
            //{
            //    var def = scenario.SceneryDefinitions[scenInstance.SceneryDefinitionIndex];

            //    var xform = Matrix4x4.CreateScale(new Vector3(1))
            //        * Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(scenInstance.Orientation.Y, scenInstance.Orientation.Z, scenInstance.Orientation.X))
            //        * Matrix4x4.CreateTranslation(scenInstance.Position);

            //    if (!scene.TryGetTag(def.Scenery, out var scen))
            //    {
            //        continue;
            //    }

            //    if (!scene.TryGetTag(scen.PhysicalModel, out var phmo))
            //    {
            //        continue;
            //    }

            //    if (!scene.TryGetTag(phmo.RenderModel, out var mode))
            //    {
            //        continue;
            //    }

            //    writer.WriteModel(mode.Parts[0].Model, xform, "scen_" + scenInstance.SceneryDefinitionIndex);
            //}

            foreach (var machInstance in scenario.MachineryInstances)
            {
                var def = scenario.MachineryDefinitions[machInstance.MachineryDefinitionIndex];

                var xform = Matrix4x4.CreateScale(new Vector3(1))
                    * Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(machInstance.Orientation.Y, machInstance.Orientation.Z, machInstance.Orientation.X))
                    * Matrix4x4.CreateTranslation(machInstance.Position);

                if (!scene.TryGetTag(def.Machinery, out var mach))
                {
                    continue;
                }

                if (!scene.TryGetTag(mach.Model, out var hlmt))
                {
                    continue;
                }

                if (scene.TryGetTag(hlmt.RenderModel, out var mode))
                {
                    //writer.WriteModel(mode.Parts[0].Model, xform, "mach_" + machInstance.MachineryDefinitionIndex);
                }

                if(scene.TryGetTag(hlmt.ColliderId, out var coll))
                {
                    var meshes = new List<ModelMesh>();

                    foreach(var comp in coll.ColliderComponents)
                    {
                        var level = comp.DamageLevels[0];
                        //foreach(var level in comp.DamageLevels)
                        {
                            var triMesh = GetTriangulatedCollisionMesh(level.Parts);

                            meshes.Add(new ModelMesh()
                            {
                                Indices = triMesh.Item2.ToArray(),
                                Verticies = triMesh.Item1.Select(v => new VertexFormat(v, Vector2.Zero, Vector3.Zero)).ToArray(),
                                ElementType = Foundation.MeshElementType.TriangleList
                            });
                        }
                    }    


                    var model = new MeshCollection(meshes.ToArray());
                    //writer.WriteModel(model, xform, "mach_" + machInstance.MachineryDefinitionIndex + "_coll");
                }
            }

            foreach (var itemPlacement in scenario.ItemCollectionPlacements)
            {
                var xform = Matrix4x4.CreateScale(new Vector3(1))
                    * Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(itemPlacement.Orientation.Y, itemPlacement.Orientation.Z, itemPlacement.Orientation.X))
                    * Matrix4x4.CreateTranslation(itemPlacement.Position);

                if (!scene.TryGetTag<BaseTag>(itemPlacement.ItemCollectionReference, out var itemTag))
                    continue;

                TagRef<HaloModelTag> hlmtRef = default;

                if (itemTag is ItemCollectionTag itmc)
                {
                    if (!scene.TryGetTag<BaseTag>(itmc.Items[0].ItemTag, out var item))
                        continue;

                    if (item is WeaponTag weap)
                        hlmtRef = weap.Hlmt;
                }

                if (hlmtRef == default)
                    continue;
                
                if (!scene.TryGetTag(hlmtRef, out var hlmt))
                    continue;

                if (!scene.TryGetTag(hlmt.RenderModel, out var mode))
                    continue;

                var index = mode.Components[0].DamageLevels[0].HighestPieceIndex;
                
                //writer.WriteModel(mode.Parts[index].Model, xform, "itmc_" + itemPlacement.ItemCollectionReference);
            }

            var count = 0;
            foreach (var character in scenario.CharacterDefinitions)
            {
                var charTag = scene.GetTag(character.CharacterReference);
                Console.WriteLine("Dumping char " + character.CharacterReference.Id);

                var xform = Matrix4x4.CreateScale(new Vector3(1))
                    * Matrix4x4.CreateFromQuaternion(Quaternion.Identity)
                    * Matrix4x4.CreateTranslation(new Vector3(1f * count++, 0, 0));

                if (!scene.TryGetTag(charTag.Biped, out var biped))
                {
                    continue;
                }

                if (!scene.TryGetTag(biped.Model, out var hlmt))
                {
                    continue;
                }

                if (scene.TryGetTag(hlmt.RenderModel, out var mode))
                {
                    writer.WriteModel(mode.Parts[0].Model, xform, "char_" + character.CharacterReference.Id);
                }

                //if (scene.TryGetTag(hlmt.ColliderId, out var coll))
                //{
                //    var meshes = new List<ModelMesh>();

                //    foreach (var comp in coll.ColliderComponents)
                //    {
                //        var level = comp.DamageLevels[0];
                //        //foreach(var level in comp.DamageLevels)
                //        {
                //            var triMesh = GetTriangulatedCollisionMesh(level.Parts);

                //            meshes.Add(new ModelMesh()
                //            {
                //                Indices = triMesh.Item2.ToArray(),
                //                Verticies = triMesh.Item1.Select(v => new VertexFormat(v, Vector2.Zero, Vector3.Zero)).ToArray(),
                //                ElementType = Foundation.MeshElementType.TriangleList
                //            });
                //        }
                //    }


                //    var model = new MeshCollection(meshes.ToArray());
                //    writer.WriteModel(model, xform, "mach_" + machInstance.MachineryDefinitionIndex + "_coll");
                //}
            }

            File.WriteAllText(outPath, writer.ToString());

            Console.WriteLine($"Processed {Path.GetFileName(mapPath)}");
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }

        private static Quaternion QuatFrom3x3Mat4(float[] vals)
        {
            var mat4 = new Matrix4x4(
                vals[0],
                vals[1],
                vals[2],
                0f,
                vals[3],
                vals[4],
                vals[5],
                0f,
                vals[6],
                vals[7],
                vals[8],
                0f,
                0f,
                0f,
                0f,
                1f);

            if (Matrix4x4.Decompose(mat4, out var _, out var q, out var _) == false)
            {
                return Quaternion.Identity;
            }

            return q;
        }

        private static (List<Vector3>, List<int>) GetTriangulatedCollisionMesh(ICollisionInfo[] collisionInfos)
        {
            var totalFaces = collisionInfos.Sum(c => c.Faces.Length);

            List<Vector3> verts = new List<Vector3>(collisionInfos.Sum(c => c.Vertices.Length));
            List<int> indices = new List<int>(totalFaces * 4);

            for (var i = 0; i < collisionInfos.Length; i++)
            {
                var currentVertStart = verts.Count();

                var col = collisionInfos[i];
                verts.AddRange(col.Vertices.Select(v => new Vector3(v.x, v.y, v.z)));

                var faceVerts = new List<ushort>(8);

                for (var faceIndex = 0; faceIndex < col.Faces.Length; faceIndex++)
                {
                    var face = col.Faces[faceIndex];

                    ushort edgeIndex = face.FirstEdge;
                    do
                    {
                        var edge = col.HalfEdges[edgeIndex];

                        faceVerts.Add(edge.Face0 == faceIndex
                            ? edge.Vertex0
                            : edge.Vertex1);

                        edgeIndex = edge.Face0 == faceIndex
                            ? edge.NextEdge
                            : edge.PrevEdge;

                    } while (edgeIndex != face.FirstEdge);

                    // Triangulate into a fan, this assumes that we're working with convex
                    // polygons with no colinear triplets, if this isn't sufficient we'll
                    // have to resort to ear cutting
                    var possibleTriangles = faceVerts.Count - 2;
                    var first = faceVerts[0];
                    for (var f = 0; f < possibleTriangles; f++)
                    {
                        var second = faceVerts[f + 1];
                        var third = faceVerts[f + 2];

                        indices.Add(currentVertStart + first);
                        indices.Add(currentVertStart + second);
                        indices.Add(currentVertStart + third);
                    }

                    faceVerts.Clear();
                }
            }

            return (verts, indices);
        }
    }
}
