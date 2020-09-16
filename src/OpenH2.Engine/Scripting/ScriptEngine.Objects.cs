using OpenH2.Core.GameObjects;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;

namespace OpenH2.Engine.Scripting
{
    public partial class ScriptEngine : IScriptEngine
    {

        /// <summary>returns the object attached to the marker of the given parent object</summary>
        public IGameObject entity_at_marker(IGameObject entity, string /*id*/ string_id)
        {
            return default(IGameObject);
        }

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

        /// <summary>Set whether the object can die from damage or not (as opposed to by scripting)</summary>
        public void object_cannot_die(bool boolean)
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
        public void object_create(EntityIdentifier object_name)
        {
        }

        /// <summary>creates an object from the scenario.</summary>
        public void object_create(IGameObject object_name)
        {
        }

        /// <summary>creates an object, destroying it first if it already exists.</summary>
        public void object_create_anew(EntityIdentifier object_name)
        {
        }

        /// <summary>creates an object, destroying it first if it already exists.</summary>
        public void object_create_anew(IGameObject entity)
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
        public void object_damage_damage_section(IGameObject entity, string /*id*/ string_id, float real)
        {
        }

        /// <summary>applies damage to a damage section, causing all manner of effects/constraint breakage to occur</summary>
        public void object_damage_damage_section(string /*id*/ emotion, float floatValue)
        {
        }

        /// <summary>destroys an object.</summary>
        public void object_destroy(IGameObject entity)
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
        public void object_dynamic_simulation_disable(IGameObject entity, bool boolean)
        {
        }

        /// <summary>returns the parent of the given object</summary>
        public IGameObject object_get_parent(IGameObject entity)
        {
            return default(IGameObject);
        }

        /// <summary>returns the ai attached to this object, if any</summary>
        public IAiActor object_get_ai()
        {
            return default(IAiActor);
        }

        /// <summary>returns the ai attached to this object, if any</summary>
        public IAiActor object_get_ai(IGameObject entity)
        {
            return default(IAiActor);
        }

        /// <summary>returns the health [0,1] of the object, returns -1 if the object does not exist</summary>
        public float object_get_health(IGameObject entity)
        {
            return default(float);
        }

        /// <summary>returns the health [0,1] of the object, returns -1 if the object does not exist</summary>
        public float object_get_health()
        {
            return default(float);
        }

        /// <summary>returns the parent of the given object</summary>
        public IGameObject entity_get_parent()
        {
            return default(IGameObject);
        }

        /// <summary>returns the parent of the given object</summary>
        public IGameObject entity_get_parent(IGameObject entity)
        {
            return default(IGameObject);
        }

        /// <summary>returns the shield [0,1] of the object, returns -1 if the object does not exist</summary>
        public float object_get_shield(IGameObject entity)
        {
            return default(float);
        }

        /// <summary>returns the shield [0,1] of the object, returns -1 if the object does not exist</summary>
        public float object_get_shield()
        {
            return default(float);
        }

        /// <summary>hides or shows the object passed in</summary>
        public void object_hide(IGameObject entity, bool boolean)
        {
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

        /// <summary>sets funciton variable for sin-o-matic use</summary>
        public void object_set_function_variable(string /*id*/ emotion, float floatValue0, float floatValue1)
        {
        }

        /// <summary>sets the desired region (use "" for all regions) to the permutation with the given name, e.g. (object_set_permutation flood "right arm" ~damaged)</summary>
        public void object_set_permutation(IGameObject entity, string /*id*/ string_id, string /*id*/ string_id1)
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
        public void object_set_phantom_power(IGameObject entity, bool boolean)
        {
        }

        /// <summary>sets the desired region (use "" for all regions) to the model state with the given name, e.g. (object_set_region_state marine head destroyed)</summary>
        public void object_set_region_state(IGameObject entity, string /*id*/ string_id, DamageState model_state)
        {
        }

        /// <summary>sets the scale for a given object and interpolates over the given number of frames to achieve that scale</summary>
        public void object_set_scale(float floatValue, short valueValue)
        {
        }

        /// <summary>sets the scale for a given object and interpolates over the given number of frames to achieve that scale</summary>
        public void object_set_scale(IGameObject entity, float real, short value)
        {
        }

        /// <summary>sets the shield vitality of the specified object (between 0 and 1).</summary>
        public void object_set_shield(IGameObject entity, float real)
        {
        }

        /// <summary>sets the shield vitality of the specified object (between 0 and 1).</summary>
        public void object_set_shield()
        {
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
        public void object_teleport(IGameObject entity, ScenarioTag.LocationFlagDefinition cutscene_flag)
        {
        }

        /// <summary>makes an object use the cinematic directional and ambient lights instead of sampling the lightmap.</summary>
        public void object_uses_cinematic_lighting(IGameObject entity, bool boolean)
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
        public void objects_attach(IGameObject entity, string /*id*/ string_id, IGameObject entity1, string /*id*/ string_id1)
        {
        }

        /// <summary>attaches the second object to the first both strings can be empty</summary>
        public void objects_attach(IGameObject entity, string /*id*/ emotion0, string /*id*/ emotion1)
        {
        }

        /// <summary>attaches the second object to the first both strings can be empty</summary>
        public void objects_attach(string /*id*/ emotion0, IGameObject entity, string /*id*/ emotion1)
        {
        }

        /// <summary>attaches the second object to the first both strings can be empty</summary>
        public void objects_attach(string /*id*/ emotion0, string /*id*/ emotion1)
        {
        }

        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the flag.</summary>
        public bool objects_can_see_flag(IGameObject entity, ScenarioTag.LocationFlagDefinition locationFlag, float floatValue)
        {
            return default(bool);
        }

        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the flag.</summary>
        public bool objects_can_see_flag(GameObjectList list, ScenarioTag.LocationFlagDefinition locationFlag, float floatValue)
        {
            return default(bool);
        }

        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the object.</summary>
        public bool objects_can_see_object(IGameObject entity, EntityIdentifier obj, float degrees)
        {
            return default(bool);
        }

        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the object.</summary>
        public bool objects_can_see_object(GameObjectList list, EntityIdentifier obj, float degrees)
        {
            return default(bool);
        }

        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the object.</summary>
        public bool objects_can_see_object(float floatValue)
        {
            return default(bool);
        }

        /// <summary>detaches from the given parent object the given child object</summary>
        public void objects_detach(IGameObject entity, IGameObject entity1)
        {
        }

        /// <summary>detaches from the given parent object the given child object</summary>
        public void objects_detach(IGameObject entity)
        {
        }

        /// <summary>detaches from the given parent object the given child object</summary>
        public void objects_detach()
        {
        }

        /// <summary>returns minimum distance from any of the specified objects to the specified flag. (returns -1 if there are no objects, or no flag, to check)</summary>
        public float objects_distance_to_flag(GameObjectList list, ScenarioTag.LocationFlagDefinition locationFlag)
        {
            return default(float);
        }

        /// <summary>returns minimum distance from any of the specified objects to the specified flag. (returns -1 if there are no objects, or no flag, to check)</summary>
        public float objects_distance_to_flag(IGameObject entity, ScenarioTag.LocationFlagDefinition locationFlag)
        {
            return default(float);
        }

        /// <summary>returns minimum distance from any of the specified objects to the specified destination object. (returns -1 if there are no objects to check)</summary>
        public float objects_distance_to_object(GameObjectList list, IGameObject entity)
        {
            return default(float);
        }
    }
}
