using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;

namespace OpenH2.Engine.Scripting
{
    public partial class ScriptEngine : IScriptEngine
    {
        /// <summary>converts an object to a unit.</summary>
        public Unit unit(Entity entity)
        {
            return entity as Unit;
        }

        /// <summary>adds/resets the unit's health, shield, and inventory (weapons and grenades) to the named profile. resets if third parameter is true, adds if false. weapons will be marked as garbage if fourth parameter is true (for respawning equipment).</summary>
        public void unit_add_equipment(Unit unit, ScenarioTag.StartingProfileDefinition starting_profile, bool reset, bool isGarbage)
        {
        }

        /// <summary>prevents any of the given units from dropping weapons or grenades when they die</summary>
        public void unit_doesnt_drop_items(EntityList entities)
        {
        }

        /// <summary>makes a unit exit its vehicle</summary>
        public void unit_exit_vehicle(Unit unit, short value)
        {
        }

        /// <summary>returns the number of ticks remaining in a unit's custom animation (or zero, if the animation is over).</summary>
        public short unit_get_custom_animation_time(Unit unit)
        {
            return default(short);
        }

        /// <summary>returns the health [0,1] of the unit, returns -1 if the unit does not exist</summary>
        public float unit_get_health(Unit unit)
        {
            return default(float);
        }

        /// <summary>returns the shield [0,1] of the unit, returns -1 if the unit does not exist</summary>
        public float unit_get_shield(Unit unit)
        {
            return default(float);
        }

        /// <summary>returns the shield [0,1] of the unit, returns -1 if the unit does not exist</summary>
        public float unit_get_shield()
        {
            return default(float);
        }

        /// <summary>returns TRUE if the <unit> has <object> as a weapon, FALSE otherwise</summary>
        public bool unit_has_weapon(Unit unit, ScenarioTag.WeaponPlacement weapon)
        {
            return default(bool);
        }

        /// <summary>prevents any of the given units from being knocked around or playing ping animations</summary>
        public void unit_impervious(Entity unit, bool boolean)
        {
        }

        /// <summary>prevents any of the given units from being knocked around or playing ping animations</summary>
        public void unit_impervious(EntityList object_list, bool boolean)
        {
        }

        /// <summary>returns true if the given unit is seated on a parent unit</summary>
        public bool unit_in_vehicle()
        {
            return default(bool);
        }

        /// <summary>returns true if the given unit is seated on a parent unit</summary>
        public bool unit_in_vehicle(Unit unit)
        {
            return default(bool);
        }

        /// <summary>returns whether or not the given unit is current emitting an ai</summary>
        public bool unit_is_emitting(Unit unit)
        {
            return default(bool);
        }

        /// <summary>kills a given unit, no saving throw</summary>
        public void unit_kill()
        {
        }

        /// <summary>kills a given unit, no saving throw</summary>
        public void unit_kill(Unit unit)
        {
        }

        /// <summary>kills a given unit silently (doesn't make them play their normal death animation or sound)</summary>
        public void unit_kill_silent(Unit unit)
        {
        }

        /// <summary>used for the tartarus boss fight</summary>
        public void unit_only_takes_damage_from_players_team(Unit unit, bool boolean)
        {
        }

        /// <summary>enable or disable active camo for the given unit over the specified number of seconds</summary>
        public void unit_set_active_camo(Unit unit, bool boolean, float real)
        {
        }

        /// <summary>sets a unit's current body and shield vitality</summary>
        public void unit_set_current_vitality(Unit unit, float real, float real1)
        {
        }

        /// <summary>sets a unit's current body and shield vitality</summary>
        public void unit_set_current_vitality(float body, float shield)
        {
        }

        /// <summary>sets a unit's facial expression by name with weight and transition time</summary>
        public void unit_set_emotional_state(Unit unit, string /*id*/ string_id, float real, short value)
        {
        }

        /// <summary>sets a unit's facial expression by name with weight and transition time</summary>
        public void unit_set_emotional_state(string /*id*/ emotion, float floatValue, short valueValue)
        {
        }

        /// <summary>can be used to prevent the player from entering a vehicle</summary>
        public void unit_set_enterable_by_player(Unit unit, bool boolean)
        {
        }

        /// <summary>sets a unit's maximum body and shield vitality</summary>
        public void unit_set_maximum_vitality(Unit unit, float real, float real1)
        {
        }

        /// <summary>sets a unit's maximum body and shield vitality</summary>
        public void unit_set_maximum_vitality(float body, float shield)
        {
        }

        /// <summary>stops the custom animation running on the given unit.</summary>
        public void unit_stop_custom_animation(Unit unit)
        {
        }

        /// <summary>sets a group of units' current body and shield vitality</summary>
        public void units_set_current_vitality(EntityList units, float body, float shield)
        {
        }

        /// <summary>sets a group of units' maximum body and shield vitality</summary>
        public void units_set_maximum_vitality(EntityList units, float body, float shield)
        {
        }
    }
}
