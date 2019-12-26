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
    public class ItemFactory
    {
        public static Item FromTag(H2vMap map, ScenarioTag scenario,  ScenarioTag.ItemCollectionPlacement instance)
        {
            var scenery = new Item();

            if (instance.ItemCollectionReference.IsInvalid)
                return scenery;

            if(map.TryGetTag<BaseTag>(instance.ItemCollectionReference, out var itemTag) == false)
                throw new Exception("Unable to load itmc");
            
            var meshes = new List<ModelMesh>();

            // TODO: add support for other tags
            if(itemTag is ItemCollectionTag itmc)
            {
                // I've only seen 1 item collections though
                foreach (var item in itmc.Items)
                {
                    if (map.TryGetTag<BaseTag>(item.ItemTag, out var tag) == false)
                    {
                        throw new Exception("No tag found for weap/equip");
                    }

                    TagRef<PhysicalModelTag> itemHlmt = default;

                    if (tag is WeaponTag weap)
                        itemHlmt = weap.Hlmt;

                    if (tag is EquipmentTag eqip)
                        itemHlmt = eqip.Hlmt;

                    if (itemHlmt == default)
                        continue;

                    if (map.TryGetTag(itemHlmt, out var hlmt) == false)
                    {
                        Console.WriteLine($"Couldn't find ITMC[{tag.Id}]'s HLMT[{itemHlmt.Id}]");
                        continue;
                    }

                    if (map.TryGetTag(hlmt.Model, out var model) == false)
                    {
                        Console.WriteLine($"No MODE[{hlmt.Model.Id}] found for HLMT[{hlmt.Id}]");
                        continue;
                    }

                    foreach (var lod in model.Lods)
                    {
                        var partIndex = lod.Permutations[0].HighestPieceIndex;
                        meshes.AddRange(model.Parts[partIndex].Model.Meshes);
                    }
                }
            }

            var renderModelMeshes = new List<Mesh<BitmapTag>>(meshes.Count);

            foreach (var mesh in meshes)
            {
                var mat = map.CreateMaterial(mesh);

                renderModelMeshes.Add(new Mesh<BitmapTag>()
                {
                    Compressed = mesh.Compressed,
                    ElementType = mesh.ElementType,
                    Indicies = mesh.Indices,
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
                    Note = $"[{itemTag.Id}] {itemTag.Name}",
                    //Position = instance.Position,
                    //Orientation = instance.Orientation.ToQuaternion(),
                    Flags = ModelFlags.Diffuse | ModelFlags.CastsShadows | ModelFlags.ReceivesShadows,
                    Meshes = renderModelMeshes.ToArray()
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
