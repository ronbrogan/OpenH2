using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Maps.Vista;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Engine.Factories;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenH2.Engine.EntityFactories
{
    public class MachineryFactory
    {
        public static Machine FromTag(H2vMap map, ScenarioTag scenario,  ScenarioTag.MachineryInstance instance)
        {
            var mach = new Machine();
            mach.FriendlyName = "Machine_" + instance.MachineryDefinitionIndex;

            if (instance.MachineryDefinitionIndex == ushort.MaxValue)
            {
                Console.WriteLine($"MACH index out of range");
                return mach;
            }

            var id = scenario.MachineryDefinitions[instance.MachineryDefinitionIndex].Machinery;
            var tag = map.GetTag(id);

            mach.FriendlyName = tag.Name;

            var orientation = QuaternionExtensions.FromH2vOrientation(instance.Orientation);
            var xform = new TransformComponent(mach, instance.Position, orientation);

            if (map.TryGetTag(tag.Model, out var model))
            {
                var components = new List<Component>();
                Dictionary<string, (Vector3, Quaternion)> bones = new();

                if(map.TryGetTag(model.RenderModel, out var renderModel))
                {
                    bones = renderModel.Nodes.ToDictionary(n => n.Name, n => (n.Translation, n.Orientation));

                    components.Add(new RenderModelComponent(mach, new Model<BitmapTag>
                    {
                        Note = $"[{tag.Id}] {tag.Name}",
                        Meshes = MeshFactory.GetRenderModel(map, model, renderModel),
                        Flags = ModelFlags.Diffuse | ModelFlags.CastsShadows | ModelFlags.ReceivesShadows
                    }));

                    components.Add(new RenderModelComponent(mach, new Model<BitmapTag>
                    {
                        Note = $"[{tag.Id}] {tag.Name} bones",
                        Meshes = MeshFactory.GetBonesModel(renderModel),
                        Flags = ModelFlags.Wireframe,
                        RenderLayer = RenderLayers.Debug
                    }));
                }

                if (map.TryGetTag(model.PhysicsModel, out var phmo) && phmo.RigidBodies.Length > 0)
                {
                    foreach(var (body, bone) in PhysicsComponentFactory.CreateKinematicRigidBodies(mach, bones, xform, phmo))
                    {
                        components.Add(body);
                    
                        components.Add(new RenderModelComponent(mach, new Model<BitmapTag>
                        {
                            Note = $"PHMO [{phmo.Id}], body {body.GetHashCode()} viz",
                            Meshes = MeshFactory.GetRenderModel(body.Collider, new Vector4(1f, 0f, 1f, 1f)),
                            Flags = ModelFlags.Wireframe | ModelFlags.IsStatic,
                            RenderLayer = RenderLayers.Collision,
                            Position = bone.Item1,
                            Orientation = bone.Item2
                        }));
                    }
                }

                mach.SetComponents(xform, components.ToArray());

                if (TryGetWellKnownIndex(scenario, instance, out var wki))
                {
                    foreach (var trig in scenario.TriggerVolumes)
                    {
                        if (trig.ParentId == wki)
                        {
                            var deltaP = Vector3.Zero;
                            var deltaQ = Quaternion.Identity;

                            var parentBone = renderModel?.Nodes.FirstOrDefault(b => b.Name == trig.ParentDescription);

                            if (parentBone != null)
                            {
                                deltaP = parentBone.Translation;
                                deltaQ = parentBone.Orientation;
                            }

                            TriggerFactory.WithScenarioTriggerVolume(mach, scenario, trig, deltaP, deltaQ);
                        }
                    }
                }
            }

            return mach;
        }

        private static bool TryGetWellKnownIndex(ScenarioTag scenario, ScenarioTag.MachineryInstance instance, out int result)
        {
            var machIndex = Array.IndexOf(scenario.MachineryInstances, instance);

            for (int i = 0; i < scenario.WellKnownItems.Length; i++)
            {
                if (scenario.WellKnownItems[i].ItemType == ScenarioTag.WellKnownVarType.Machinery
                    && scenario.WellKnownItems[i].Index == machIndex)
                {
                    result = i;
                    return true;
                }
            }

            result = 0;
            return false;
        }
    }
}
