using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;

namespace OpenH2.Engine.Scripting
{
    public partial class ScriptEngine : IScriptEngine
    {

        /// <summary>converts an ai reference to an object list.</summary>
        public EntityList ai_actors(AI ai)

        {
            return default(EntityList);
        }

        /// <summary>creates an allegiance between two teams.</summary>
        public void ai_allegiance(Team team, Team team1)
        {
        }

        /// <summary>attaches the specified list of units to the specified encounter.</summary>
        public void ai_attach_units(EntityList units, AI ai)
        {
        }

        /// <summary>attaches the specified list of units to the specified encounter.</summary>
        public void ai_attach_units(Unit unit, AI ai)
        {
        }

        /// <summary>forces a group of actors to start or stop berserking</summary>
        public void ai_berserk(AI ai, bool boolean)
        {
        }

        /// <summary>makes a group of actors braindead, or restores them to life (in their initial state)</summary>
        public void ai_braindead(AI ai, bool boolean)
        {
        }

        /// <summary>AI cannot die from damage (as opposed to by scripting)</summary>
        public void ai_cannot_die(AI ai, bool boolean)
        {
        }

        /// <summary>Returns the highest integer combat status in the given squad-group/squad/actor</summary>
        public short ai_combat_status(AI ai)
        {
            return default(short);
        }

        /// <summary>turn combat dialogue on/off</summary>
        public void ai_dialogue_enable(bool boolean)
        {
        }

        /// <summary>enables or disables automatic garbage collection for actors in the specified encounter and/or squad.</summary>
        public void ai_disposable(AI ai, bool boolean)
        {
        }

        /// <summary>if TRUE, forces all actors to completely disregard the specified units, otherwise lets them acknowledge the units again</summary>
        public void ai_disregard(Entity unit, bool boolean)
        {
            System.Diagnostics.Debug.Assert(unit is Unit);
        }

        /// <summary>if TRUE, forces all actors to completely disregard the specified units, otherwise lets them acknowledge the units again</summary>
        public void ai_disregard(EntityList object_list, bool boolean)
        {
        }

        /// <summary>Instructs the ai in the given squad to get in all their vehicles</summary>
        public void ai_enter_squad_vehicles(AI ai)
        {
        }

        /// <summary>erases the specified encounter and/or squad.</summary>
        public void ai_erase(AI ai)
        {
        }

        /// <summary>erases all AI.</summary>
        public void ai_erase_all()
        {
        }

        /// <summary>return the number of actors that are fighting in a squad or squad_group</summary>
        public short ai_fighting_count(AI ai)
        {
            return default(short);
        }

        /// <summary>returns the unit/object corresponding to the given actor</summary>
        public Entity ai_get_object(AI ai)
        {
            return default(Entity);
        }

        /// <summary>returns the unit/object corresponding to the given actor</summary>
        public Unit ai_get_unit(AI ai)
        {
            return default(Unit);
        }

        /// <summary>instantly kills the specified encounter and/or squad.</summary>
        public void ai_kill(AI ai)
        {
        }

        /// <summary>instantly and silently (no animation or sound played) kills the specified encounter and/or squad.</summary>
        public void ai_kill_silent(AI ai)
        {
        }

        /// <summary>return the number of living actors in the specified encounter and/or squad.</summary>
        public short ai_living_count(AI ai)
        {
            return default(short);
        }

        /// <summary>Make one squad magically aware of another.</summary>
        public void ai_magically_see(AI ai, AI ai1)
        {
        }

        /// <summary>Make a squad magically aware of a particular object.</summary>
        public void ai_magically_see_object(AI ai, Entity value)
        {
        }

        /// <summary>makes all or part of an encounter move to another encounter.</summary>
        public void ai_migrate(AI ai, AI ai1)
        {
        }

        /// <summary>makes all or part of an encounter move to another encounter.</summary>
        public void ai_migrate(AI ai)
        {
        }

        /// <summary>return the number of non-swarm actors in the specified encounter and/or squad.</summary>
        public short ai_nonswarm_count(AI ai)
        {
            return default(short);
        }

        /// <summary>Don't use this for anything other than bug 3926.  AI magically cancels vehicle oversteer.</summary>
        public void ai_overcomes_oversteer(AI ai, bool boolean)
        {
        }

        /// <summary>places the specified squad on the map.</summary>
        public void ai_place(AI ai)
        {
        }

        /// <summary>places the specified squad on the map.</summary>
        public void ai_place(AI ai, short value)
        {
        }

        /// <summary>places the specified squad on the map.</summary>
        public void ai_place(AI ai, float value)
        {
        }

        /// <summary>places the specified squad (1st arg) on the map in the vehicles belonging to the specified vehicle squad (2nd arg).</summary>
        public void ai_place_in_vehicle(AI ai, AI ai1)
        {
        }

        /// <summary>Play the given mission dialogue line on the given ai</summary>
        public short ai_play_line(AI ai, string /*id*/ string_id)
        {
            return default(short);
        }

        /// <summary>Play the given mission dialogue line on the given ai, directing the ai's gaze at the nearest visible player</summary>
        public short ai_play_line_at_player(AI ai, string /*id*/ string_id)
        {
            return default(short);
        }

        /// <summary>Play the given mission dialogue line on the given ai, directing the ai's gaze at the nearest visible player</summary>
        public short ai_play_line_at_player(string /*id*/ emotion)
        {
            return default(short);
        }

        /// <summary>Play the given mission dialogue line on the given object (uses first available variant)</summary>
        public short ai_play_line_on_object(Entity entity, string /*id*/ string_id)
        {
            return default(short);
        }

        /// <summary>if TRUE, *ALL* enemies will prefer to attack the specified units. if FALSE, removes the preference.</summary>
        public void ai_prefer_target(EntityList units, bool boolean)
        {
        }

        /// <summary>refreshes the health and grenade count of a group of actors, so they are as good as new</summary>
        public void ai_renew(AI ai)
        {
        }

        /// <summary>Start the named scene, with the named command script on the named squad</summary>
        public bool ai_scene(string /*id*/ string_id, AIScript ai_command_script, AI ai)
        {
            return default(bool);
        }

        /// <summary>Start the named scene, with the named command script on the named set of squads</summary>
        public bool ai_scene(string /*id*/ string_id, AIScript ai_command_script, AI ai, AI ai1)
        {
            return default(bool);
        }

        /// <summary>Start the named scene, with the named command script on the named squad</summary>
        public bool ai_scene(string /*id*/ emotion, AIScript aiScript)
        {
            return default(bool);
        }

        /// <summary>Turn on active camoflage on actor/squad/squad-group</summary>
        public void ai_set_active_camo(AI ai, bool boolean)
        {
        }

        /// <summary>enables or disables sight for actors in the specified encounter.</summary>
        public void ai_set_blind(AI ai, bool boolean)
        {
        }

        /// <summary>enables or disables hearing for actors in the specified encounter.</summary>
        public void ai_set_deaf(AI ai, bool boolean)
        {
        }

        /// <summary>Takes the squad or squad group (arg1) and gives it the order (arg3) in zone (arg2). Use the zone_name/order_name format</summary>
        public void ai_set_orders(AI ai, ScenarioTag.AiOrderDefinition ai_orders)
        {
        }

        /// <summary>returns the number of actors spawned in the given squad or squad group</summary>
        public short ai_spawn_count(AI ai)
        {
            return default(short);
        }

        /// <summary>return the current strength (average body vitality from 0-1) of the specified encounter and/or squad.</summary>
        public float ai_strength(AI ai)
        {
            return default(float);
        }

        /// <summary>Turn on/off combat suppression on actor/squad/squad-group</summary>
        public void ai_suppress_combat(AI ai, bool boolean)
        {
        }

        /// <summary>return the number of swarm actors in the specified encounter and/or squad.</summary>
        public short ai_swarm_count(AI ai)
        {
            return default(short);
        }

        /// <summary>teleports a group of actors to the starting locations of their current squad(s) if they are currently outside the world.</summary>
        public void ai_teleport_to_starting_location_if_outside_bsp(AI ai)
        {
        }

        /// <summary>Tests the named trigger on the named squad</summary>
        public bool ai_trigger_test(string value, AI ai)
        {
            return default(bool);
        }

        /// <summary>tells a group of actors to get into a vehicle, in the substring-specified seats (e.g. passenger for pelican)... does not interrupt any actors who are already going to vehicles</summary>
        public void ai_vehicle_enter(AI ai)
        {
        }

        /// <summary>tells a group of actors to get into a vehicle, in the substring-specified seats (e.g. passenger for pelican)... does not interrupt any actors who are already going to vehicles</summary>
        public void ai_vehicle_enter(AI ai, Unit unit, /*VehicleSeat*/ string unit_seat_mapping = null)
        {
        }

        /// <summary>tells a group of actors to get into a vehicle... does not interrupt any actors who are already going to vehicles</summary>
        public void ai_vehicle_enter(AI ai, /*VehicleSeat*/ string unit)
        {
        }

        /// <summary>the given group of actors is snapped into a vehicle, in the substring-specified seats (e.g. passenger for pelican)... does not interrupt any actors who are already going to vehicles</summary>
        public void ai_vehicle_enter_immediate(AI ai, Unit unit, /*VehicleSeat*/ string seat = null)
        {
        }

        /// <summary>tells a group of actors to get out of any vehicles that they are in</summary>
        public void ai_vehicle_exit(AI ai)
        {
        }


        /// <summary>Returns the vehicle that the given actor is in.</summary>
        public Vehicle ai_vehicle_get(AI ai)
        {
            return default(Vehicle);
        }

        /// <summary>Returns the vehicle that was spawned at the given starting location.</summary>
        public Vehicle ai_vehicle_get_from_starting_location(AI ai)
        {
            return default(Vehicle);
        }

        /// <summary>Reserves the given vehicle (so that AI may not enter it</summary>
        public void ai_vehicle_reserve(Vehicle vehicle, bool boolean)
        {
        }

        /// <summary>Reserves the given seat on the given vehicle (so that AI may not enter it</summary>
        public void ai_vehicle_reserve_seat(Vehicle vehicle, string /*id*/ string_id, bool boolean)
        {
        }

        /// <summary>Reserves the given seat on the given vehicle (so that AI may not enter it</summary>
        public void ai_vehicle_reserve_seat(string /*id*/ emotion, bool boolean)
        {
        }

        /// <summary>Returns true if the ai's units are ALL vitality pinned (see object_vitality_pinned)</summary>
        public bool ai_vitality_pinned(AI ai)
        {
            return default(bool);
        }

    }
}
