using OpenH2.Core.GameObjects;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;

namespace OpenH2.Engine.Scripting
{
    public partial class ScriptEngine : IScriptEngine
    {
        /// <summary>converts an object to a unit.</summary>
        public IUnit unit(IGameObject entity)
        {
            return entity as IUnit;
        }

        /// <summary>adds/resets the unit's health, shield, and inventory (weapons and grenades) to the named profile. resets if third parameter is true, adds if false. weapons will be marked as garbage if fourth parameter is true (for respawning equipment).</summary>
        public void unit_add_equipment(IUnit unit, ScenarioTag.StartingProfileDefinition starting_profile, bool reset, bool isGarbage)
        {
        }

        /// <summary>prevents any of the given units from dropping weapons or grenades when they die</summary>
        public void unit_doesnt_drop_items(GameObjectList entities)
        {
        }

        /// <summary>makes a unit exit its vehicle</summary>
        public void unit_exit_vehicle(IUnit unit, short value)
        {
        }

        /// <summary>returns the number of ticks remaining in a unit's custom animation (or zero, if the animation is over).</summary>
        public short unit_get_custom_animation_time(IUnit unit)
        {
            return default(short);
        }

        /// <summary>returns the health [0,1] of the unit, returns -1 if the unit does not exist</summary>
        public float unit_get_health(IUnit unit)
        {
            return default(float);
        }

        /// <summary>returns the shield [0,1] of the unit, returns -1 if the unit does not exist</summary>
        public float unit_get_shield(IUnit unit)
        {
            return default(float);
        }

        /// <summary>returns the shield [0,1] of the unit, returns -1 if the unit does not exist</summary>
        public float unit_get_shield()
        {
            return default(float);
        }

        /// <summary>returns TRUE if the <unit> has <object> as a weapon, FALSE otherwise</summary>
        public bool unit_has_weapon(IUnit unit, ScenarioTag.WeaponPlacement weapon)
        {
            return default(bool);
        }

        /// <summary>prevents any of the given units from being knocked around or playing ping animations</summary>
        public void unit_impervious(IGameObject unit, bool boolean)
        {
        }

        /// <summary>prevents any of the given units from being knocked around or playing ping animations</summary>
        public void unit_impervious(GameObjectList object_list, bool boolean)
        {
        }

        /// <summary>returns true if the given unit is seated on a parent unit</summary>
        public bool unit_in_vehicle()
        {
            return default(bool);
        }

        /// <summary>returns true if the given unit is seated on a parent unit</summary>
        public bool unit_in_vehicle(IUnit unit)
        {
            return default(bool);
        }

        /// <summary>returns whether or not the given unit is current emitting an ai</summary>
        public bool unit_is_emitting(IUnit unit)
        {
            return default(bool);
        }

        /// <summary>kills a given unit, no saving throw</summary>
        public void unit_kill()
        {
        }

        /// <summary>kills a given unit, no saving throw</summary>
        public void unit_kill(IUnit unit)
        {
        }

        /// <summary>kills a given unit silently (doesn't make them play their normal death animation or sound)</summary>
        public void unit_kill_silent(IUnit unit)
        {
        }

        /// <summary>used for the tartarus boss fight</summary>
        public void unit_only_takes_damage_from_players_team(IUnit unit, bool boolean)
        {
        }

        /// <summary>enable or disable active camo for the given unit over the specified number of seconds</summary>
        public void unit_set_active_camo(IUnit unit, bool boolean, float real)
        {
        }

        /// <summary>sets a unit's current body and shield vitality</summary>
        public void unit_set_current_vitality(IUnit unit, float real, float real1)
        {
        }

        /// <summary>sets a unit's current body and shield vitality</summary>
        public void unit_set_current_vitality(float body, float shield)
        {
        }

        /// <summary>sets a unit's facial expression by name with weight and transition time</summary>
        public void unit_set_emotional_state(IUnit unit, string /*id*/ string_id, float real, short value)
        {
        }

        /// <summary>sets a unit's facial expression by name with weight and transition time</summary>
        public void unit_set_emotional_state(string /*id*/ emotion, float floatValue, short valueValue)
        {
        }

        /// <summary>can be used to prevent the player from entering a vehicle</summary>
        public void unit_set_enterable_by_player(IUnit unit, bool boolean)
        {
        }

        /// <summary>sets a unit's maximum body and shield vitality</summary>
        public void unit_set_maximum_vitality(IUnit unit, float real, float real1)
        {
        }

        /// <summary>sets a unit's maximum body and shield vitality</summary>
        public void unit_set_maximum_vitality(float body, float shield)
        {
        }

        /// <summary>stops the custom animation running on the given unit.</summary>
        public void unit_stop_custom_animation(IUnit unit)
        {
        }

        /// <summary>sets a group of units' current body and shield vitality</summary>
        public void units_set_current_vitality(GameObjectList units, float body, float shield)
        {
        }

        /// <summary>sets a group of units' maximum body and shield vitality</summary>
        public void units_set_maximum_vitality(GameObjectList units, float body, float shield)
        {
        }
    }
}
