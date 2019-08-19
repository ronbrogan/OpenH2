using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
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

            var def = bsp.InstancedGeometryDefinitions[instance.Index];

            var comp = new RenderModelComponent(scenery);
            comp.Note = $"[{bsp.Id}] {bsp.Name}//instanced//{instance.Index}";
            comp.Meshes = def.Model.Meshes;
            comp.Position = instance.Position;
            comp.Orientation = QuatFrom3x3Mat4(instance.RotationMatrix);
            comp.Scale = new Vector3(instance.Scale);

            EnsureMaterials(map, comp);

            scenery.SetComponents(new[] { comp });

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

            var id = scenario.SceneryReferences[instance.SceneryDefinitionIndex].SceneryId;
            map.TryGetTag<SceneryTag>(id, out var tag);

            if (map.TryGetTag<PhysicalModelTag>(tag.HlmtId, out var hlmt) == false)
            {
                throw new Exception("No model found for scenery");
            }

            if (map.TryGetTag<ModelTag>(hlmt.ModelId, out var model) == false)
            {
                Console.WriteLine($"No MODE[{hlmt.ModelId}] found for HLMT[{hlmt.Id}]");
                return scenery;
            }

            var meshes = new List<Mesh>();

            foreach (var lod in model.Lods)
            {
                var part = lod.Permutations.First().HighestPieceIndex;
                meshes.AddRange(model.Parts[part].Model.Meshes);
            }

            var comp = new RenderModelComponent(scenery);
            comp.Note = $"[{tag.Id}] {tag.Name}";
            comp.Meshes = meshes.ToArray();
            comp.Position = instance.Position;
            comp.Orientation = instance.Orientation.ToQuaternion();
            comp.Scale = new Vector3(1);

            EnsureMaterials(map, comp);

            var components = new List<Component>();
            components.Add(comp);
            scenery.SetComponents(components.ToArray());

            return scenery;
        }

        private static void EnsureMaterials(H2vMap map, RenderModelComponent comp)
        {
            foreach (var mesh in comp.Meshes)
            {
                if (comp.Materials.ContainsKey(mesh.MaterialIdentifier))
                {
                    continue;
                }

                var mat = new Material<BitmapTag>();
                mat.DiffuseColor = VectorExtensions.RandomColor();
                comp.Materials.Add(mesh.MaterialIdentifier, mat);


                if (map.TryGetTag<ShaderTag>(mesh.MaterialIdentifier, out var shader) == false)
                {
                    continue;
                }

                if (shader.BitmapInfos.Length > 0)
                {
                    var bitms = shader.BitmapInfos[0];

                    if (bitms == null)
                        continue;

                    map.TryGetTag<BitmapTag>(bitms.DiffuseBitmapId, out var diffuse);

                    mat.DiffuseMap = diffuse;

                    var bitmRefs = shader.Parameters.SelectMany(p => p.BitmapParameter1s.Select(b => b.BitmapId));
                    foreach (var bitmRef in bitmRefs)
                    {
                        if (map.TryGetTag<BitmapTag>(bitmRef, out var bitm) && bitm.TextureUsage == Core.Enums.Texture.TextureUsage.Bump)
                        {
                            mat.NormalMap = bitm;
                        }
                    }
                }
            }
        }
    }
}
