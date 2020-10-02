using OpenH2.Core.GameObjects;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using System;

namespace OpenH2.Engine.Scripting
{
    public partial class ScriptEngine : IScriptEngine
    {

        /// <summary>converts an ai reference to an object list.</summary>
        public GameObjectList ai_actors(IAiActor ai)
        {
            return GameObjectList.Empty;
        }

        /// <summary>creates an allegiance between two teams.</summary>
        public void ai_allegiance(ITeam team, ITeam team1)
        {
        }

        /// <summary>attaches the specified list of units to the specified encounter.</summary>
        public void ai_attach_units(GameObjectList units, IAiActor ai)
        {
        }

        /// <summary>attaches the specified list of units to the specified encounter.</summary>
        public void ai_attach_units(IUnit unit, IAiActor ai)
        {
        }

        /// <summary>forces a group of actors to start or stop berserking</summary>
        public void ai_berserk(IAiActor ai, bool boolean)
        {
        }

        /// <summary>makes a group of actors braindead, or restores them to life (in their initial state)</summary>
        public void ai_braindead(IAiActor ai, bool boolean)
        {
        }

        /// <summary>AI cannot die from damage (as opposed to by scripting)</summary>
        public void ai_cannot_die(IAiActor ai, bool boolean)
        {
        }

        /// <summary>Returns the highest integer combat status in the given squad-group/squad/actor</summary>
        public short ai_combat_status(IAiActor ai)
        {
            return default(short);
        }

        /// <summary>turn combat dialogue on/off</summary>
        public void ai_dialogue_enable(bool boolean)
        {
        }

        /// <summary>enables or disables automatic garbage collection for actors in the specified encounter and/or squad.</summary>
        public void ai_disposable(IAiActor ai, bool boolean)
        {
        }

        /// <summary>if TRUE, forces all actors to completely disregard the specified units, otherwise lets them acknowledge the units again</summary>
        public void ai_disregard(IGameObject unit, bool boolean)
        {
            System.Diagnostics.Debug.Assert(unit is IUnit);
        }

        /// <summary>if TRUE, forces all actors to completely disregard the specified units, otherwise lets them acknowledge the units again</summary>
        public void ai_disregard(GameObjectList object_list, bool boolean)
        {
        }

        /// <summary>Instructs the ai in the given squad to get in all their vehicles</summary>
        public void ai_enter_squad_vehicles(IAiActor ai)
        {
        }

        /// <summary>erases the specified encounter and/or squad.</summary>
        public void ai_erase(IAiActor ai)
        {
        }

        /// <summary>erases all AI.</summary>
        public void ai_erase_all()
        {
        }

        /// <summary>return the number of actors that are fighting in a squad or squad_group</summary>
        public short ai_fighting_count(IAiActor ai)
        {
            return default(short);
        }

        /// <summary>returns the unit/object corresponding to the given actor</summary>
        public IGameObject ai_get_object(IAiActor ai)
        {
            return default(IGameObject);
        }

        /// <summary>returns the unit/object corresponding to the given actor</summary>
        public IUnit ai_get_unit(IAiActor ai)
        {
            return default(IUnit);
        }

        /// <summary>instantly kills the specified encounter and/or squad.</summary>
        public void ai_kill(IAiActor ai)
        {
        }

        /// <summary>instantly and silently (no animation or sound played) kills the specified encounter and/or squad.</summary>
        public void ai_kill_silent(IAiActor ai)
        {
        }

        /// <summary>return the number of living actors in the specified encounter and/or squad.</summary>
        public short ai_living_count(IAiActor ai)
        {
            return default(short);
        }

        /// <summary>Make one squad magically aware of another.</summary>
        public void ai_magically_see(IAiActor ai, IAiActor ai1)
        {
        }

        /// <summary>Make a squad magically aware of a particular object.</summary>
        public void ai_magically_see_object(IAiActor ai, IGameObject value)
        {
        }

        /// <summary>makes all or part of an encounter move to another encounter.</summary>
        public void ai_migrate(IAiActor ai, IAiActor ai1)
        {
        }

        /// <summary>makes all or part of an encounter move to another encounter.</summary>
        public void ai_migrate(IAiActor ai)
        {
        }

        /// <summary>return the number of non-swarm actors in the specified encounter and/or squad.</summary>
        public short ai_nonswarm_count(IAiActor ai)
        {
            return default(short);
        }

        /// <summary>Don't use this for anything other than bug 3926.  AI magically cancels vehicle oversteer.</summary>
        public void ai_overcomes_oversteer(IAiActor ai, bool boolean)
        {
        }

        /// <summary>places the specified squad on the map.</summary>
        public void ai_place(IAiActor ai)
        {
        }

        /// <summary>places the specified squad on the map.</summary>
        public void ai_place(IAiActor ai, short value)
        {
        }

        /// <summary>places the specified squad on the map.</summary>
        public void ai_place(IAiActor ai, float value)
        {
        }

        /// <summary>places the specified squad (1st arg) on the map in the vehicles belonging to the specified vehicle squad (2nd arg).</summary>
        public void ai_place_in_vehicle(IAiActor ai, IAiActor ai1)
        {
        }

        /// <summary>Play the given mission dialogue line on the given ai</summary>
        public short ai_play_line(IAiActor ai, string /*id*/ string_id)
        {
            return default(short);
        }

        /// <summary>Play the given mission dialogue line on the given ai, directing the ai's gaze at the nearest visible player</summary>
        public short ai_play_line_at_player(IAiActor ai, string /*id*/ string_id)
        {
            return default(short);
        }

        /// <summary>Play the given mission dialogue line on the given ai, directing the ai's gaze at the nearest visible player</summary>
        public short ai_play_line_at_player(string /*id*/ emotion)
        {
            return default(short);
        }

        /// <summary>Play the given mission dialogue line on the given object (uses first available variant)</summary>
        public short ai_play_line_on_object(IGameObject entity, string /*id*/ string_id)
        {
            return default(short);
        }

        /// <summary>if TRUE, *ALL* enemies will prefer to attack the specified units. if FALSE, removes the preference.</summary>
        public void ai_prefer_target(GameObjectList units, bool boolean)
        {
        }

        /// <summary>refreshes the health and grenade count of a group of actors, so they are as good as new</summary>
        public void ai_renew(IAiActor ai)
        {
        }

        /// <summary>Start the named scene, with the named command script on the named squad</summary>
        public bool ai_scene(string /*id*/ string_id, AIScript ai_command_script, IAiActor ai)
        {
            return default(bool);
        }

        /// <summary>Start the named scene, with the named command script on the named set of squads</summary>
        public bool ai_scene(string /*id*/ string_id, AIScript ai_command_script, IAiActor ai, IAiActor ai1)
        {
            return default(bool);
        }

        /// <summary>Start the named scene, with the named command script on the named squad</summary>
        public bool ai_scene(string /*id*/ emotion, AIScript aiScript)
        {
            return default(bool);
        }

        /// <summary>Turn on active camoflage on actor/squad/squad-group</summary>
        public void ai_set_active_camo(IAiActor ai, bool boolean)
        {
        }

        /// <summary>enables or disables sight for actors in the specified encounter.</summary>
        public void ai_set_blind(IAiActor ai, bool boolean)
        {
        }

        /// <summary>enables or disables hearing for actors in the specified encounter.</summary>
        public void ai_set_deaf(IAiActor ai, bool boolean)
        {
        }

        /// <summary>Takes the squad or squad group (arg1) and gives it the order (arg3) in zone (arg2). Use the zone_name/order_name format</summary>
        public void ai_set_orders(IAiActor ai, IAiOrders ai_orders)
        {
        }

        /// <summary>returns the number of actors spawned in the given squad or squad group</summary>
        public short ai_spawn_count(IAiActor ai)
        {
            return default(short);
        }

        /// <summary>return the current strength (average body vitality from 0-1) of the specified encounter and/or squad.</summary>
        public float ai_strength(IAiActor ai)
        {
            return default(float);
        }

        /// <summary>Turn on/off combat suppression on actor/squad/squad-group</summary>
        public void ai_suppress_combat(IAiActor ai, bool boolean)
        {
        }

        /// <summary>return the number of swarm actors in the specified encounter and/or squad.</summary>
        public short ai_swarm_count(IAiActor ai)
        {
            return default(short);
        }

        /// <summary>teleports a group of actors to the starting locations of their current squad(s) if they are currently outside the world.</summary>
        public void ai_teleport_to_starting_location_if_outside_bsp(IAiActor ai)
        {
        }

        /// <summary>Tests the named trigger on the named squad</summary>
        public bool ai_trigger_test(string value, IAiActor ai)
        {
            return default(bool);
        }

        /// <summary>tells a group of actors to get into a vehicle, in the substring-specified seats (e.g. passenger for pelican)... does not interrupt any actors who are already going to vehicles</summary>
        public void ai_vehicle_enter(IAiActor ai)
        {
        }

        /// <summary>tells a group of actors to get into a vehicle, in the substring-specified seats (e.g. passenger for pelican)... does not interrupt any actors who are already going to vehicles</summary>
        public void ai_vehicle_enter(IAiActor ai, IUnit unit, /*VehicleSeat*/ string unit_seat_mapping = null)
        {
        }

        /// <summary>tells a group of actors to get into a vehicle... does not interrupt any actors who are already going to vehicles</summary>
        public void ai_vehicle_enter(IAiActor ai, /*VehicleSeat*/ string unit)
        {
        }

        /// <summary>the given group of actors is snapped into a vehicle, in the substring-specified seats (e.g. passenger for pelican)... does not interrupt any actors who are already going to vehicles</summary>
        public void ai_vehicle_enter_immediate(IAiActor ai, IUnit unit, /*VehicleSeat*/ string seat = null)
        {
        }

        /// <summary>tells a group of actors to get out of any vehicles that they are in</summary>
        public void ai_vehicle_exit(IAiActor ai)
        {
        }


        /// <summary>Returns the vehicle that the given actor is in.</summary>
        public IVehicle ai_vehicle_get(IAiActor ai)
        {
            return default(IVehicle);
        }

        /// <summary>Returns the vehicle that was spawned at the given starting location.</summary>
        public IVehicle ai_vehicle_get_from_starting_location(IAiActor ai)
        {
            return default(IVehicle);
        }

        /// <summary>Reserves the given vehicle (so that AI may not enter it</summary>
        public void ai_vehicle_reserve(IVehicle vehicle, bool boolean)
        {
        }

        /// <summary>Reserves the given seat on the given vehicle (so that AI may not enter it</summary>
        public void ai_vehicle_reserve_seat(IVehicle vehicle, string /*id*/ string_id, bool boolean)
        {
        }

        /// <summary>Reserves the given seat on the given vehicle (so that AI may not enter it</summary>
        public void ai_vehicle_reserve_seat(string /*id*/ emotion, bool boolean)
        {
        }

        /// <summary>Returns true if the ai's units are ALL vitality pinned (see object_vitality_pinned)</summary>
        public bool ai_vitality_pinned(IAiActor ai)
        {
            return default(bool);
        }

    }
}
