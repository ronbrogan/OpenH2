using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Common;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenH2.Engine.EntityFactories
{
    public class SceneryFactory
    {
        public static Scenery FromInstancedGeometry(H2vMap map, BspTag bsp, BspTag.InstancedGeometryInstance instance)
        {
            var scenery = new Scenery();

            if (instance.Index >= bsp.InstancedGeometryDefinitions.Length)
                return scenery;

            var def = bsp.InstancedGeometryDefinitions[instance.Index];

            var renderModelMeshes = new List<Mesh<BitmapTag>>(def.Model.Meshes.Length);

            foreach (var mesh in def.Model.Meshes)
            {
                var mat = new Material<BitmapTag>();
                mat.DiffuseColor = VectorExtensions.RandomColor();

                if (map.TryGetTag(mesh.Shader, out var shader))
                {
                    MaterialFactory.PopulateMaterial(map, mat, shader);
                }

                renderModelMeshes.Add(new Mesh<BitmapTag>()
                {
                    Compressed = mesh.Compressed,
                    ElementType = mesh.ElementType,
                    Indicies = mesh.Indicies,
                    Note = mesh.Note,
                    RawData = mesh.RawData,
                    Verticies = mesh.Verticies,

                    Material = mat
                });
            }

            var comp = new RenderModelComponent(scenery)
            {
                RenderModel = new Model<BitmapTag>
                {
                    Note = $"[{bsp.Id}] {bsp.Name}//instanced//{instance.Index}",
                    Meshes = renderModelMeshes.ToArray(),                   
                    Flags = ModelFlags.Diffuse | ModelFlags.CastsShadows | ModelFlags.ReceivesShadows
                }
            };

            var xform = new TransformComponent(scenery)
            {
                Position = instance.Position,
                Scale = new Vector3(instance.Scale),
                Orientation = QuatFrom3x3Mat4(instance.RotationMatrix)
            };

            scenery.SetComponents(new Component[] { comp, xform });

            return scenery;
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

            if(Matrix4x4.Decompose(mat4, out var _, out var q, out var _) == false)
            {
                return Quaternion.Identity;
            }

            return q;
        }

        public static Scenery FromTag(H2vMap map, ScenarioTag scenario,  ScenarioTag.SceneryInstance instance)
        {
            var scenery = new Scenery();

            var id = scenario.SceneryReferences[instance.SceneryDefinitionIndex].Scenery;
            map.TryGetTag(id, out var tag);

            if (map.TryGetTag(tag.PhysicalModel, out var hlmt) == false)
            {
                Console.WriteLine($"No HLMT[{tag.PhysicalModel.Id}] found for SCNR[{tag.Id}]");
                return scenery;
            }

            if (map.TryGetTag(hlmt.Model, out var model) == false)
            {
                Console.WriteLine($"No MODE[{hlmt.Model.Id}] found for HLMT[{hlmt.Id}]");
                return scenery;
            }

            var meshes = new List<ModelMesh>();

            foreach (var lod in model.Lods)
            {
                var part = lod.Permutations.First().HighestPieceIndex;
                meshes.AddRange(model.Parts[part].Model.Meshes);
            }

            var renderModelMeshes = new List<Mesh<BitmapTag>>(meshes.Count);

            foreach (var mesh in meshes)
            {
                var mat = new Material<BitmapTag>();
                mat.DiffuseColor = VectorExtensions.RandomColor();

                if (map.TryGetTag(mesh.Shader, out var shader))
                {
                    MaterialFactory.PopulateMaterial(map, mat, shader);
                }

                renderModelMeshes.Add(new Mesh<BitmapTag>()
                {
                    Compressed = mesh.Compressed,
                    ElementType = mesh.ElementType,
                    Indicies = mesh.Indicies,
                    Note = mesh.Note,
                    RawData = mesh.RawData,
                    Verticies = mesh.Verticies,

                    Material = mat
                });
            }

            var comp = new RenderModelComponent(scenery)
            {
                RenderModel = new Model<BitmapTag>
                {
                    Note = $"[{tag.Id}] {tag.Name}",
                    Meshes = renderModelMeshes.ToArray(),
                    Scale = new Vector3(1),
                    Flags = ModelFlags.Diffuse | ModelFlags.CastsShadows | ModelFlags.ReceivesShadows
                }
            };

            var xform = new TransformComponent(scenery)
            {
                Position = instance.Position,
                Orientation = Quaternion.CreateFromYawPitchRoll(instance.Orientation.Y, instance.Orientation.Z, instance.Orientation.X)
            };

            scenery.SetComponents(new Component[] { comp, xform });

            return scenery;
        }
    }
}
