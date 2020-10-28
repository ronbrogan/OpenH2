using OpenH2.Core.Architecture;
using OpenH2.Core.GameObjects;
using OpenH2.Core.Scripting;
using System;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;

namespace OpenH2.Engine.Scripting
{
    public partial class ScriptEngine : IScriptEngine
    {

        public IGameObject object_at_marker(IGameObject entity, string stringId)
        {
            return default(IGameObject);
        }

        /// <summary>allows an object to take damage again</summary>
        public void object_can_take_damage(IGameObject entity)
        {
        }

        /// <summary>allows an object to take damage again</summary>
        public void object_can_take_damage(GameObjectList object_list)
        {
        }

        /// <summary>Set whether the object can die from damage or not (as opposed to by scripting)</summary>
        public void object_cannot_die(IGameObject entity, bool boolean)
        {
        }

        /// <summary>prevents an object from taking damage</summary>
        public void object_cannot_take_damage(IGameObject entity) // Unit?
        {
        }

        /// <summary>prevents an object from taking damage</summary>
        public void object_cannot_take_damage(GameObjectList object_list)
        {
        }

        /// <summary>makes an object use the highest lod for the remainder of the levels' cutscenes.</summary>
        public void object_cinematic_lod(IGameObject entity, bool boolean)
        {
        }

        /// <summary>makes an object use the highest lod for the remainder of the levels' cutscenes.</summary>
        public void object_cinematic_lod(bool boolean)
        {
        }

        /// <summary>makes an object bypass visibility and always render during cinematics.</summary>
        public void object_cinematic_visibility(IGameObject entity, bool boolean)
        {
        }

        /// <summary>clears all funciton variables for sin-o-matic use</summary>
        public void object_clear_all_function_variables(IGameObject entity)
        {
        }

        /// <summary>clears one funciton variables for sin-o-matic use</summary>
        public void object_clear_function_variable(IGameObject entity, string /*id*/ string_id)
        {
        }

        /// <summary>creates an object from the scenario.</summary>
        public void object_create(IEntityIdentifier object_name)
        {
            var def = this.scene.Scenario.WellKnownItems[object_name.Identifier];
            this.scene.CreateEntity(def);
        }

        /// <summary>creates an object, destroying it first if it already exists.</summary>
        public void object_create_anew(IEntityIdentifier object_name)
        {
            var def = this.scene.Scenario.WellKnownItems[object_name.Identifier];
            this.scene.CreateEntity(def);
        }

        /// <summary>creates anew all objects from the scenario whose names contain the given substring.</summary>
        public void object_create_anew_containing(string value)
        {
            var wellKnowns = this.scene.Scenario.WellKnownItems;
            for (int i = 0; i < wellKnowns.Length; i++)
            {
                if (wellKnowns[i].Identifier.Contains(value))
                    this.scene.CreateEntity(wellKnowns[i]);
            }
        }

        /// <summary>creates an object, potentially resulting in multiple objects if it already exists.</summary>
        public void object_create_clone(IEntityIdentifier object_name)
        {
            var def = this.scene.Scenario.WellKnownItems[object_name.Identifier];
            this.scene.CreateEntity(def);
        }

        /// <summary>creates all objects from the scenario whose names contain the given substring.</summary>
        public void object_create_containing(string value)
        {
            var wellKnowns = this.scene.Scenario.WellKnownItems;
            for (int i = 0; i < wellKnowns.Length; i++)
            {
                if (wellKnowns[i].Identifier.Contains(value))
                    this.scene.CreateEntity(wellKnowns[i]);
            }
        }

        /// <summary>applies damage to a damage section, causing all manner of effects/constraint breakage to occur</summary>
        public void object_damage_damage_section(IGameObject entity, string /*id*/ string_id, float real)
        {
        }

        /// <summary>destroys an object.</summary>
        public void object_destroy(IGameObject entity)
        {
            if(entity is Entity e)
                this.scene.RemoveEntity(e);
        }

        /// <summary>destroys all objects from the scenario whose names contain the given substring.</summary>
        public void object_destroy_containing(string value)
        {
            foreach(var entity in this.scene.Entities.Values)
            {
                if (entity.FriendlyName.Contains(value))
                    this.scene.RemoveEntity(entity);
            }
        }

        /// <summary>destroys all objects matching the type mask</summary>
        public void object_destroy_type_mask(int value)
        {
        }

        /// <summary>disabled dynamic simulation for this object (makes it fixed)</summary>
        public void object_dynamic_simulation_disable(IGameObject entity, bool boolean)
        {
        }

        /// <summary>returns the parent of the given object</summary>
        public IGameObject object_get_parent(IGameObject entity)
        {
            if (entity == null)
                return null;

            return entity.Parent;
        }

        /// <summary>returns the ai attached to this object, if any</summary>
        public IAiActor object_get_ai(IGameObject entity)
        {
            if (entity == null)
                return null;

            return entity.Ai;
        }

        /// <summary>returns the health [0,1] of the object, returns -1 if the object does not exist</summary>
        public float object_get_health(IGameObject entity)
        {
            if (entity == null)
                return -1;

            return entity.Health;
        }

        /// <summary>returns the shield [0,1] of the object, returns -1 if the object does not exist</summary>
        public float object_get_shield(IGameObject entity)
        {
            if (entity == null)
                return -1;

            return entity.Shield;
        }

        /// <summary>hides or shows the object passed in</summary>
        public void object_hide(IGameObject entity, bool boolean)
        {
            if (entity == null)
                return;

            if (boolean)
            {
                entity.Hide();
            }
            else
            {
                entity.Show();
            }
        }

        /// <summary>returns TRUE if the specified model target is destroyed</summary>
        public short object_model_targets_destroyed(IGameObject entity, string /*id*/ target)
        {
            return default(short);
        }

        /// <summary>when this object deactivates it will be deleted</summary>
        public void object_set_deleted_when_deactivated(IGameObject entity)
        {
        }

        /// <summary>sets funciton variable for sin-o-matic use</summary>
        public void object_set_function_variable(IGameObject entity, string /*id*/ string_id, float real, float real1)
        {
        }

        /// <summary>sets the desired region (use "" for all regions) to the permutation with the given name, e.g. (object_set_permutation flood "right arm" ~damaged)</summary>
        public void object_set_permutation(IGameObject entity, string /*id*/ string_id, string /*id*/ string_id1)
        {
        }

        /// <summary>sets phantom power to be latched at 1.0f or 0.0f</summary>
        public void object_set_phantom_power(IGameObject entity, bool boolean)
        {
        }

        /// <summary>sets the desired region (use "" for all regions) to the model state with the given name, e.g. (object_set_region_state marine head destroyed)</summary>
        public void object_set_region_state(IGameObject entity, string /*id*/ string_id, IDamageState model_state)
        {
        }

        /// <summary>sets the scale for a given object and interpolates over the given number of frames to achieve that scale</summary>
        public void object_set_scale(IGameObject entity, float scale, short interpolateOver)
        {
            if (entity == null)
                return;

            entity.Scale(scale);
        }

        /// <summary>sets the shield vitality of the specified object (between 0 and 1).</summary>
        public void object_set_shield(IGameObject entity, float vitality)
        {
            if (entity == null)
                return;

            entity.Shield = vitality;
        }

        /// <summary>make this objects shield be stunned permanently</summary>
        public void object_set_shield_stun_infinite(IGameObject entity)
        {
        }

        /// <summary>Sets the (object-relative) forward velocity of the given object</summary>
        public void object_set_velocity(IGameObject entity, float real)
        {
        }

        /// <summary>Sets the (object-relative) forward velocity of the given object</summary>
        public void object_set_velocity(IGameObject entity, float real, float real1, float real12)
        {
        }

        /// <summary>moves the specified object to the specified flag.</summary>
        public void object_teleport(IGameObject entity, ILocationFlag cutscene_flag)
        {
            if (entity == null || cutscene_flag == null)
                return;

            entity.TeleportTo(cutscene_flag.Position);
        }

        /// <summary>makes an object use the cinematic directional and ambient lights instead of sampling the lightmap.</summary>
        public void object_uses_cinematic_lighting(IGameObject entity, bool boolean)
        {
        }

        /// <summary>attaches the second object to the first both strings can be empty</summary>
        public void objects_attach(IGameObject parent, string /*id*/ string_id, IGameObject entity, string /*id*/ string_id1)
        {
            if (parent == null || entity == null)
                return; 

            parent.Attach(entity);
        }

        private bool ObjectCanSeePoint(IGameObject entity, Vector3 point, float degrees)
        {
            if (entity == null)
                return false;

            var entityForward = Vector3.Transform(EngineGlobals.Forward, entity.Orientation);
            var flagForward = point - entity.Position;

            var entityHeading = MathF.Atan2(entityForward.Y, entityForward.X);
            var flagHeading = MathF.Atan2(flagForward.Y, flagForward.X);

            var entityPitch = MathF.Atan2(entityForward.Z, entityForward.X);
            var flagPitch = MathF.Atan2(flagForward.Z, flagForward.X);

            var toleranceRadians = (degrees * MathF.PI) / 180f;
            var headingDelta = MathF.Abs(flagHeading - entityHeading);
            var pitchDelta = MathF.Abs(flagPitch - entityPitch);

            return headingDelta < toleranceRadians && pitchDelta < toleranceRadians;
        }

        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the flag.</summary>
        public bool objects_can_see_flag(IGameObject entity, ILocationFlag locationFlag, float degrees)
        {
            if (entity == null || locationFlag == null)
                return false;

            return ObjectCanSeePoint(entity, locationFlag.Position, degrees);
        }

        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the flag.</summary>
        public bool objects_can_see_flag(GameObjectList list, ILocationFlag locationFlag, float degrees)
        {
            if (list  == null || locationFlag == null)
                return false;

            foreach (var entity in list.Objects)
            {
                if(ObjectCanSeePoint(entity, locationFlag.Position, degrees))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the object.</summary>
        public bool objects_can_see_object(IGameObject entity, IGameObject target, float degrees)
        {
            return ObjectCanSeePoint(entity, target.Position, degrees);
        }
        
        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the object.</summary>
        public bool objects_can_see_object(GameObjectList list, IGameObject target, float degrees)
        {
            if (list == null || target == null)
                return false;

            foreach (var entity in list.Objects)
            {
                if (ObjectCanSeePoint(entity, target.Position, degrees))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>detaches from the given parent object the given child object</summary>
        public void objects_detach(IGameObject parent, IGameObject child)
        {
            if (parent == null || child == null)
                return;

            parent.Detach(child);
        }

        /// <summary>returns minimum distance from any of the specified objects to the specified flag. (returns -1 if there are no objects, or no flag, to check)</summary>
        public float objects_distance_to_flag(GameObjectList list, ILocationFlag locationFlag)
        {
            if(list == null || list.Objects.Length == 0 || locationFlag == null)
            {
                return -1;
            }

            var test = locationFlag.Position;
            var min = float.MaxValue;

            foreach(var e in list.Objects)
            {
                min = MathF.Min(Vector3.Distance(e.Position, test), min);
            }

            return min;
        }

        /// <summary>returns minimum distance from any of the specified objects to the specified flag. (returns -1 if there are no objects, or no flag, to check)</summary>
        public float objects_distance_to_flag(IGameObject entity, ILocationFlag locationFlag)
        {
            if (locationFlag == null || entity == null)
            {
                return -1;
            }

            return Vector3.Distance(entity.Position, locationFlag.Position);
        }

        /// <summary>returns minimum distance from any of the specified objects to the specified destination object. (returns -1 if there are no objects to check)</summary>
        public float objects_distance_to_object(GameObjectList list, IGameObject entity)
        {
            if (list == null || list.Objects.Length == 0 || entity == null)
            {
                return -1;
            }

            var test = entity.Position;
            var min = float.MaxValue;

            foreach (var e in list.Objects)
            {
                min = MathF.Min(Vector3.Distance(e.Position, test), min);
            }

            return min;
        }
    }
}
