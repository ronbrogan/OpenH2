using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;

namespace OpenH2.Engine.Scripting
{
    public partial class ScriptEngine : IScriptEngine
    {

        /// <summary>returns the object attached to the marker of the given parent object</summary>
        public Entity entity_at_marker(Entity entity, string /*id*/ string_id)
        {
            return default(Entity);
        }

        public Entity object_at_marker(Entity entity, string stringId)
        {
            return default(Entity);
        }

        /// <summary>allows an object to take damage again</summary>
        public void object_can_take_damage(Entity entity)
        {
        }

        /// <summary>allows an object to take damage again</summary>
        public void object_can_take_damage(EntityList object_list)
        {
        }

        /// <summary>Set whether the object can die from damage or not (as opposed to by scripting)</summary>
        public void object_cannot_die(Entity entity, bool boolean)
        {
        }

        /// <summary>Set whether the object can die from damage or not (as opposed to by scripting)</summary>
        public void object_cannot_die(bool boolean)
        {
        }

        /// <summary>prevents an object from taking damage</summary>
        public void object_cannot_take_damage(Entity entity) // Unit?
        {
        }

        /// <summary>prevents an object from taking damage</summary>
        public void object_cannot_take_damage(EntityList object_list)
        {
        }

        /// <summary>makes an object use the highest lod for the remainder of the levels' cutscenes.</summary>
        public void object_cinematic_lod(Entity entity, bool boolean)
        {
        }

        /// <summary>makes an object use the highest lod for the remainder of the levels' cutscenes.</summary>
        public void object_cinematic_lod(bool boolean)
        {
        }

        /// <summary>makes an object bypass visibility and always render during cinematics.</summary>
        public void object_cinematic_visibility(Entity entity, bool boolean)
        {
        }

        /// <summary>clears all funciton variables for sin-o-matic use</summary>
        public void object_clear_all_function_variables(Entity entity)
        {
        }

        /// <summary>clears one funciton variables for sin-o-matic use</summary>
        public void object_clear_function_variable(Entity entity, string /*id*/ string_id)
        {
        }

        /// <summary>creates an object from the scenario.</summary>
        public void object_create(EntityIdentifier object_name)
        {
        }

        /// <summary>creates an object from the scenario.</summary>
        public void object_create(Entity object_name)
        {
        }

        /// <summary>creates an object, destroying it first if it already exists.</summary>
        public void object_create_anew(EntityIdentifier object_name)
        {
        }

        /// <summary>creates an object, destroying it first if it already exists.</summary>
        public void object_create_anew(Entity entity)
        {
        }

        /// <summary>creates anew all objects from the scenario whose names contain the given substring.</summary>
        public void object_create_anew_containing(string value)
        {
        }

        /// <summary>creates an object, potentially resulting in multiple objects if it already exists.</summary>
        public void object_create_clone(EntityIdentifier object_name)
        {
        }

        /// <summary>creates all objects from the scenario whose names contain the given substring.</summary>
        public void object_create_containing(string value)
        {
        }

        /// <summary>applies damage to a damage section, causing all manner of effects/constraint breakage to occur</summary>
        public void object_damage_damage_section(Entity entity, string /*id*/ string_id, float real)
        {
        }

        /// <summary>applies damage to a damage section, causing all manner of effects/constraint breakage to occur</summary>
        public void object_damage_damage_section(string /*id*/ emotion, float floatValue)
        {
        }

        /// <summary>destroys an object.</summary>
        public void object_destroy(Entity entity)
        {
        }

        /// <summary>destroys an object.</summary>
        public void object_destroy()
        {
        }

        /// <summary>destroys all objects from the scenario whose names contain the given substring.</summary>
        public void object_destroy_containing(string value)
        {
        }

        /// <summary>destroys all objects matching the type mask</summary>
        public void object_destroy_type_mask(int value)
        {
        }

        /// <summary>disabled dynamic simulation for this object (makes it fixed)</summary>
        public void object_dynamic_simulation_disable(bool boolean)
        {
        }

        /// <summary>disabled dynamic simulation for this object (makes it fixed)</summary>
        public void object_dynamic_simulation_disable(Entity entity, bool boolean)
        {
        }

        /// <summary>returns the parent of the given object</summary>
        public Entity object_get_parent(Entity entity)
        {
            return default(Entity);
        }

        /// <summary>returns the ai attached to this object, if any</summary>
        public AI object_get_ai()
        {
            return default(AI);
        }

        /// <summary>returns the ai attached to this object, if any</summary>
        public AI object_get_ai(Entity entity)
        {
            return default(AI);
        }

        /// <summary>returns the health [0,1] of the object, returns -1 if the object does not exist</summary>
        public float object_get_health(Entity entity)
        {
            return default(float);
        }

        /// <summary>returns the health [0,1] of the object, returns -1 if the object does not exist</summary>
        public float object_get_health()
        {
            return default(float);
        }

        /// <summary>returns the parent of the given object</summary>
        public Entity entity_get_parent()
        {
            return default(Entity);
        }

        /// <summary>returns the parent of the given object</summary>
        public Entity entity_get_parent(Entity entity)
        {
            return default(Entity);
        }

        /// <summary>returns the shield [0,1] of the object, returns -1 if the object does not exist</summary>
        public float object_get_shield(Entity entity)
        {
            return default(float);
        }

        /// <summary>returns the shield [0,1] of the object, returns -1 if the object does not exist</summary>
        public float object_get_shield()
        {
            return default(float);
        }

        /// <summary>hides or shows the object passed in</summary>
        public void object_hide(Entity entity, bool boolean)
        {
        }

        /// <summary>returns TRUE if the specified model target is destroyed</summary>
        public short object_model_targets_destroyed(Entity entity, string /*id*/ target)
        {
            return default(short);
        }

        /// <summary>when this object deactivates it will be deleted</summary>
        public void object_set_deleted_when_deactivated(Entity entity)
        {
        }

        /// <summary>sets funciton variable for sin-o-matic use</summary>
        public void object_set_function_variable(Entity entity, string /*id*/ string_id, float real, float real1)
        {
        }

        /// <summary>sets funciton variable for sin-o-matic use</summary>
        public void object_set_function_variable(string /*id*/ emotion, float floatValue0, float floatValue1)
        {
        }

        /// <summary>sets the desired region (use "" for all regions) to the permutation with the given name, e.g. (object_set_permutation flood "right arm" ~damaged)</summary>
        public void object_set_permutation(Entity entity, string /*id*/ string_id, string /*id*/ string_id1)
        {
        }

        /// <summary>sets the desired region (use "" for all regions) to the permutation with the given name, e.g. (object_set_permutation flood "right arm" ~damaged)</summary>
        public void object_set_permutation(string /*id*/ emotion, string /*id*/ emotion1)
        {
        }

        /// <summary>sets phantom power to be latched at 1.0f or 0.0f</summary>
        public void object_set_phantom_power(bool boolean)
        {
        }

        /// <summary>sets phantom power to be latched at 1.0f or 0.0f</summary>
        public void object_set_phantom_power(Entity entity, bool boolean)
        {
        }

        /// <summary>sets the desired region (use "" for all regions) to the model state with the given name, e.g. (object_set_region_state marine head destroyed)</summary>
        public void object_set_region_state(Entity entity, string /*id*/ string_id, DamageState model_state)
        {
        }

        /// <summary>sets the scale for a given object and interpolates over the given number of frames to achieve that scale</summary>
        public void object_set_scale(float floatValue, short valueValue)
        {
        }

        /// <summary>sets the scale for a given object and interpolates over the given number of frames to achieve that scale</summary>
        public void object_set_scale(Entity entity, float real, short value)
        {
        }

        /// <summary>sets the shield vitality of the specified object (between 0 and 1).</summary>
        public void object_set_shield(Entity entity, float real)
        {
        }

        /// <summary>sets the shield vitality of the specified object (between 0 and 1).</summary>
        public void object_set_shield()
        {
        }

        /// <summary>make this objects shield be stunned permanently</summary>
        public void object_set_shield_stun_infinite(Entity entity)
        {
        }

        /// <summary>Sets the (object-relative) forward velocity of the given object</summary>
        public void object_set_velocity(Entity entity, float real)
        {
        }

        /// <summary>Sets the (object-relative) forward velocity of the given object</summary>
        public void object_set_velocity(Entity entity, float real, float real1, float real12)
        {
        }

        /// <summary>moves the specified object to the specified flag.</summary>
        public void object_teleport(Entity entity, ScenarioTag.LocationFlagDefinition cutscene_flag)
        {
        }

        /// <summary>makes an object use the cinematic directional and ambient lights instead of sampling the lightmap.</summary>
        public void object_uses_cinematic_lighting(Entity entity, bool boolean)
        {
        }

        /// <summary>makes an object use the cinematic directional and ambient lights instead of sampling the lightmap.</summary>
        public void object_uses_cinematic_lighting(bool boolean)
        {
        }

        /// <summary>clears the mission objectives.</summary>
        public void objectives_clear()
        {
        }

        /// <summary>mark objectives 0..n as complete</summary>
        public void objectives_finish_up_to(int value)
        {
        }

        /// <summary>show objectives 0..n</summary>
        public void objectives_show_up_to(int value)
        {
        }

        /// <summary>attaches the second object to the first both strings can be empty</summary>
        public void objects_attach(Entity entity, string /*id*/ string_id, Entity entity1, string /*id*/ string_id1)
        {
        }

        /// <summary>attaches the second object to the first both strings can be empty</summary>
        public void objects_attach(Entity entity, string /*id*/ emotion0, string /*id*/ emotion1)
        {
        }

        /// <summary>attaches the second object to the first both strings can be empty</summary>
        public void objects_attach(string /*id*/ emotion0, Entity entity, string /*id*/ emotion1)
        {
        }

        /// <summary>attaches the second object to the first both strings can be empty</summary>
        public void objects_attach(string /*id*/ emotion0, string /*id*/ emotion1)
        {
        }

        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the flag.</summary>
        public bool objects_can_see_flag(Entity entity, ScenarioTag.LocationFlagDefinition locationFlag, float floatValue)
        {
            return default(bool);
        }

        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the flag.</summary>
        public bool objects_can_see_flag(EntityList list, ScenarioTag.LocationFlagDefinition locationFlag, float floatValue)
        {
            return default(bool);
        }

        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the object.</summary>
        public bool objects_can_see_object(Entity entity, EntityIdentifier obj, float degrees)
        {
            return default(bool);
        }

        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the object.</summary>
        public bool objects_can_see_object(EntityList list, EntityIdentifier obj, float degrees)
        {
            return default(bool);
        }

        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the object.</summary>
        public bool objects_can_see_object(float floatValue)
        {
            return default(bool);
        }

        /// <summary>detaches from the given parent object the given child object</summary>
        public void objects_detach(Entity entity, Entity entity1)
        {
        }

        /// <summary>detaches from the given parent object the given child object</summary>
        public void objects_detach(Entity entity)
        {
        }

        /// <summary>detaches from the given parent object the given child object</summary>
        public void objects_detach()
        {
        }

        /// <summary>returns minimum distance from any of the specified objects to the specified flag. (returns -1 if there are no objects, or no flag, to check)</summary>
        public float objects_distance_to_flag(EntityList list, ScenarioTag.LocationFlagDefinition locationFlag)
        {
            return default(float);
        }

        /// <summary>returns minimum distance from any of the specified objects to the specified flag. (returns -1 if there are no objects, or no flag, to check)</summary>
        public float objects_distance_to_flag(Entity entity, ScenarioTag.LocationFlagDefinition locationFlag)
        {
            return default(float);
        }

        /// <summary>returns minimum distance from any of the specified objects to the specified destination object. (returns -1 if there are no objects to check)</summary>
        public float objects_distance_to_object(EntityList list, Entity entity)
        {
            return default(float);
        }
    }
}
