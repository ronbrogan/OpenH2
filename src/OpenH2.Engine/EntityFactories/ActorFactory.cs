using OpenH2.Core.Architecture;
using OpenH2.Core.Maps.Vista;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Engine.Factories;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Engine.EntityFactories
{
    public static class ActorFactory
    {
        public static Actor FromStartingLocation(H2vMap map,
            ScenarioTag.AiSquadDefinition.StartingLocation loc)
        {
            var entity = new Actor();
            entity.FriendlyName = loc.Description;

            var charIndex = loc.CharacterIndex;

            if(charIndex == ushort.MaxValue)
            {
                charIndex = map.Scenario.AiSquadDefinitions[loc.SquadIndex].CharacterIndex;
            }

            if (charIndex == ushort.MaxValue)
            {
                throw new Exception("Couldn't determine character to create");
            }

            var character = map.GetTag(map.Scenario.CharacterDefinitions[charIndex].CharacterReference);
            if(map.TryGetTag(character.Biped, out var biped) == false)
            {
                return entity;
            }

            var comp = new RenderModelComponent(entity, new Model<BitmapTag>
            {
                Note = $"[{biped.Id}] {biped.Name}",
                Flags = ModelFlags.Diffuse | ModelFlags.CastsShadows | ModelFlags.ReceivesShadows,
                Meshes = MeshFactory.GetRenderModel(map, biped.Model)
            });

            var boneComp = new RenderModelComponent(entity, new Model<BitmapTag>
            {
                Note = $"[{biped.Id}] {biped.Name} Bones",
                Flags = ModelFlags.Wireframe,
                Meshes = MeshFactory.GetBonesModel(map, biped.Model)
            });

            var orientation = Quaternion.CreateFromAxisAngle(EngineGlobals.Up, loc.Rotation);
            var xform = new TransformComponent(entity, loc.Position, orientation);

            // TODO: add back
            var body = PhysicsComponentFactory.CreateDynamicRigidBody(entity, xform, map, biped.Model);

            var comOffset = Vector3.Zero;

            if (map.TryGetTag(biped.Model, out var hlmt) &&
                map.TryGetTag(hlmt.PhysicsModel, out var phmo) &&
                phmo.BodyParameters.Length > 0)
            {
                comOffset = phmo.BodyParameters[0].CenterOfMass;
            }

            var centerOfMass = new BoundsComponent(entity, comOffset - new Vector3(0.02f), comOffset + new Vector3(0.02f), new Vector4(1f, 1f, 0, 1f));
            var origin = new BoundsComponent(entity, new Vector3(-0.02f), new Vector3(0.02f), new Vector4(0, 1f, 0, 1f));

            var originalTag = new OriginalTagComponent(entity, loc);

            entity.SetComponents(xform, /*comp,*/ boneComp, centerOfMass, origin, originalTag);

            return entity;
        }
    }
}
