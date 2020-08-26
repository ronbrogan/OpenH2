namespace OpenH2.Engine.Scripting
{
    using OpenH2.Core.Scripting;
    using System;

    public class ScriptEngine
    {
        public const short TicksPerSecond = 60;
        public static T GetReference<T>(string reference)
        {
            return default(T);
        }

        /// <summary>activates a nav point type <string> attached to a team anchored to a flag with a vertical offset <real>. If the player is not local to the machine, this will fail</summary>
        public static void activate_team_nav_point_flag(NavigationPoint navpoint, Team team, LocationFlag cutscene_flag, float real)
        {
        }

        /// <summary>activates a nav point type <string> attached to a team anchored to an object with a vertical offset <real>. If the player is not local to the machine, this will fail</summary>
        public static void activate_team_nav_point_object(NavigationPoint navpoint, Team team, Entity entity, float real)
        {
        }

        /// <summary>converts an ai reference to an object list.</summary>
        public static ObjectList ai_actors(AI ai)
        {
            return default(ObjectList);
        }

        /// <summary>creates an allegiance between two teams.</summary>
        public static void ai_allegiance(Team team, Team team1)
        {
        }

        /// <summary>attaches the specified list of units to the specified encounter.</summary>
        public static void ai_attach_units(ObjectList units, AI ai)
        {
        }

        /// <summary>attaches the specified list of units to the specified encounter.</summary>
        public static void ai_attach_units(Unit unit, AI ai)
        {
        }

        /// <summary>forces a group of actors to start or stop berserking</summary>
        public static void ai_berserk(AI ai, bool boolean)
        {
        }

        /// <summary>makes a group of actors braindead, or restores them to life (in their initial state)</summary>
        public static void ai_braindead(AI ai, bool boolean)
        {
        }

        /// <summary>AI cannot die from damage (as opposed to by scripting)</summary>
        public static void ai_cannot_die(AI ai, bool boolean)
        {
        }

        /// <summary>Returns the highest integer combat status in the given squad-group/squad/actor</summary>
        public static short ai_combat_status(AI ai)
        {
            return default(short);
        }

        /// <summary>turn combat dialogue on/off</summary>
        public static void ai_dialogue_enable(bool boolean)
        {
        }

        /// <summary>enables or disables automatic garbage collection for actors in the specified encounter and/or squad.</summary>
        public static void ai_disposable(AI ai, bool boolean)
        {
        }

        /// <summary>if TRUE, forces all actors to completely disregard the specified units, otherwise lets them acknowledge the units again</summary>
        public static void ai_disregard(Entity unit, bool boolean)
        {
            System.Diagnostics.Debug.Assert(unit is Unit);
        }

        /// <summary>if TRUE, forces all actors to completely disregard the specified units, otherwise lets them acknowledge the units again</summary>
        public static void ai_disregard(ObjectList object_list, bool boolean)
        {
        }

        /// <summary>Instructs the ai in the given squad to get in all their vehicles</summary>
        public static void ai_enter_squad_vehicles(AI ai)
        {
        }

        /// <summary>erases the specified encounter and/or squad.</summary>
        public static void ai_erase(AI ai)
        {
        }

        /// <summary>erases all AI.</summary>
        public static void ai_erase_all()
        {
        }

        /// <summary>return the number of actors that are fighting in a squad or squad_group</summary>
        public static short ai_fighting_count(AI ai)
        {
            return default(short);
        }

        /// <summary>returns the unit/object corresponding to the given actor</summary>
        public static Entity ai_get_object(AI ai)
        {
            return default(Entity);
        }

        /// <summary>returns the unit/object corresponding to the given actor</summary>
        public static Unit ai_get_unit(AI ai)
        {
            return default(Unit);
        }

        /// <summary>instantly kills the specified encounter and/or squad.</summary>
        public static void ai_kill(AI ai)
        {
        }

        /// <summary>instantly and silently (no animation or sound played) kills the specified encounter and/or squad.</summary>
        public static void ai_kill_silent(AI ai)
        {
        }

        /// <summary>return the number of living actors in the specified encounter and/or squad.</summary>
        public static short ai_living_count(AI ai)
        {
            return default(short);
        }

        /// <summary>Make one squad magically aware of another.</summary>
        public static void ai_magically_see(AI ai, AI ai1)
        {
        }

        /// <summary>Make a squad magically aware of a particular object.</summary>
        public static void ai_magically_see_object(AI ai, Entity value)
        {
        }

        /// <summary>makes all or part of an encounter move to another encounter.</summary>
        public static void ai_migrate(AI ai, AI ai1)
        {
        }

        /// <summary>makes all or part of an encounter move to another encounter.</summary>
        public static void ai_migrate(AI ai)
        {
        }

        /// <summary>return the number of non-swarm actors in the specified encounter and/or squad.</summary>
        public static short ai_nonswarm_count(AI ai)
        {
            return default(short);
        }

        /// <summary>Don't use this for anything other than bug 3926.  AI magically cancels vehicle oversteer.</summary>
        public static void ai_overcomes_oversteer(AI ai, bool boolean)
        {
        }

        /// <summary>places the specified squad on the map.</summary>
        public static void ai_place(AI ai)
        {
        }

        /// <summary>places the specified squad on the map.</summary>
        public static void ai_place(AI ai, short value)
        {
        }

        /// <summary>places the specified squad on the map.</summary>
        public static void ai_place(AI ai, float value)
        {
        }

        /// <summary>places the specified squad (1st arg) on the map in the vehicles belonging to the specified vehicle squad (2nd arg).</summary>
        public static void ai_place_in_vehicle(AI ai, AI ai1)
        {
        }

        /// <summary>Play the given mission dialogue line on the given ai</summary>
        public static short ai_play_line(AI ai, string /*id*/ string_id)
        {
            return default(short);
        }

        /// <summary>Play the given mission dialogue line on the given ai, directing the ai's gaze at the nearest visible player</summary>
        public static short ai_play_line_at_player(AI ai, string /*id*/ string_id)
        {
            return default(short);
        }

        /// <summary>Play the given mission dialogue line on the given ai, directing the ai's gaze at the nearest visible player</summary>
        public static short ai_play_line_at_player(string /*id*/ emotion)
        {
            return default(short);
        }

        /// <summary>Play the given mission dialogue line on the given object (uses first available variant)</summary>
        public static short ai_play_line_on_object(Entity entity, string /*id*/ string_id)
        {
            return default(short);
        }

        /// <summary>if TRUE, *ALL* enemies will prefer to attack the specified units. if FALSE, removes the preference.</summary>
        public static void ai_prefer_target(ObjectList units, bool boolean)
        {
        }

        /// <summary>refreshes the health and grenade count of a group of actors, so they are as good as new</summary>
        public static void ai_renew(AI ai)
        {
        }

        /// <summary>Start the named scene, with the named command script on the named squad</summary>
        public static bool ai_scene(string /*id*/ string_id, AIScript ai_command_script, AI ai)
        {
            return default(bool);
        }

        /// <summary>Start the named scene, with the named command script on the named set of squads</summary>
        public static bool ai_scene(string /*id*/ string_id, AIScript ai_command_script, AI ai, AI ai1)
        {
            return default(bool);
        }

        /// <summary>Start the named scene, with the named command script on the named squad</summary>
        public static bool ai_scene(string /*id*/ emotion, AIScript aiScript)
        {
            return default(bool);
        }

        /// <summary>Turn on active camoflage on actor/squad/squad-group</summary>
        public static void ai_set_active_camo(AI ai, bool boolean)
        {
        }

        /// <summary>enables or disables sight for actors in the specified encounter.</summary>
        public static void ai_set_blind(AI ai, bool boolean)
        {
        }

        /// <summary>enables or disables hearing for actors in the specified encounter.</summary>
        public static void ai_set_deaf(AI ai, bool boolean)
        {
        }

        /// <summary>Takes the squad or squad group (arg1) and gives it the order (arg3) in zone (arg2). Use the zone_name/order_name format</summary>
        public static void ai_set_orders(AI ai, AIOrders ai_orders)
        {
        }

        /// <summary>returns the number of actors spawned in the given squad or squad group</summary>
        public static short ai_spawn_count(AI ai)
        {
            return default(short);
        }

        /// <summary>return the current strength (average body vitality from 0-1) of the specified encounter and/or squad.</summary>
        public static float ai_strength(AI ai)
        {
            return default(float);
        }

        /// <summary>Turn on/off combat suppression on actor/squad/squad-group</summary>
        public static void ai_suppress_combat(AI ai, bool boolean)
        {
        }

        /// <summary>return the number of swarm actors in the specified encounter and/or squad.</summary>
        public static short ai_swarm_count(AI ai)
        {
            return default(short);
        }

        /// <summary>teleports a group of actors to the starting locations of their current squad(s) if they are currently outside the world.</summary>
        public static void ai_teleport_to_starting_location_if_outside_bsp(AI ai)
        {
        }

        /// <summary>Tests the named trigger on the named squad</summary>
        public static bool ai_trigger_test(string value, AI ai)
        {
            return default(bool);
        }

        /// <summary>tells a group of actors to get into a vehicle, in the substring-specified seats (e.g. passenger for pelican)... does not interrupt any actors who are already going to vehicles</summary>
        public static void ai_vehicle_enter(AI ai)
        {
        }

        /// <summary>tells a group of actors to get into a vehicle, in the substring-specified seats (e.g. passenger for pelican)... does not interrupt any actors who are already going to vehicles</summary>
        public static void ai_vehicle_enter(AI ai, Unit unit, /*VehicleSeat*/ string unit_seat_mapping = null)
        {
        }

        /// <summary>tells a group of actors to get into a vehicle... does not interrupt any actors who are already going to vehicles</summary>
        public static void ai_vehicle_enter(AI ai, /*VehicleSeat*/ string unit)
        {
        }

        /// <summary>the given group of actors is snapped into a vehicle, in the substring-specified seats (e.g. passenger for pelican)... does not interrupt any actors who are already going to vehicles</summary>
        public static void ai_vehicle_enter_immediate(AI ai, Unit unit, /*VehicleSeat*/ string seat = null)
        {
        }

        /// <summary>tells a group of actors to get out of any vehicles that they are in</summary>
        public static void ai_vehicle_exit(AI ai)
        {
        }


        /// <summary>Returns the vehicle that the given actor is in.</summary>
        public static Vehicle ai_vehicle_get(AI ai)
        {
            return default(Vehicle);
        }

        /// <summary>Returns the vehicle that was spawned at the given starting location.</summary>
        public static Vehicle ai_vehicle_get_from_starting_location(AI ai)
        {
            return default(Vehicle);
        }

        /// <summary>Reserves the given vehicle (so that AI may not enter it</summary>
        public static void ai_vehicle_reserve(Vehicle vehicle, bool boolean)
        {
        }

        /// <summary>Reserves the given seat on the given vehicle (so that AI may not enter it</summary>
        public static void ai_vehicle_reserve_seat(Vehicle vehicle, string /*id*/ string_id, bool boolean)
        {
        }

        /// <summary>Reserves the given seat on the given vehicle (so that AI may not enter it</summary>
        public static void ai_vehicle_reserve_seat(string /*id*/ emotion, bool boolean)
        {
        }

        /// <summary>Returns true if the ai's units are ALL vitality pinned (see object_vitality_pinned)</summary>
        public static bool ai_vitality_pinned(AI ai)
        {
            return default(bool);
        }

        /// <summary>evaluates the sequence of expressions in random order and returns the last value evaluated.</summary>
        public static void begin_random(params System.Action[] expressions)
        {
        }

        /// <summary>evaluates the sequence of expressions in random order and returns the last value evaluated.</summary>
        public static T begin_random<T>(params System.Func<T>[] expressions)
        {
            return default(T);
        }

        /// <summary>returns true if the movie is done playing</summary>
        public static bool bink_done()
        {
            return default(bool);
        }

        /// <summary>given a dead biped, turns on ragdoll</summary>
        public static void biped_ragdoll(Unit unit)
        {
        }

        /// <summary>call this to force texture and geometry cache to block until satiated</summary>
        public static void cache_block_for_one_frame()
        {
        }

        /// <summary>toggles script control of the camera.</summary>
        public static void camera_control(bool boolean)
        {
        }

        /// <summary>predict resources at a frame in camera animation.</summary>
        public static void camera_predict_resources_at_frame(Animation animation, string /*id*/ emotion, Unit unit, LocationFlag locationFlag, int intValue)
        {
        }

        /// <summary>predict resources given a camera point</summary>
        public static void camera_predict_resources_at_point(CameraPathTarget cutscene_camera_point)
        {
        }

        /// <summary>moves the camera to the specified camera point over the specified number of ticks.</summary>
        public static void camera_set(CameraPathTarget cutscene_camera_point, short value)
        {
        }

        /// <summary>begins a prerecorded camera animation synchronized to unit relative to cutscene flag.</summary>
        public static void camera_set_animation_relative(Animation animation, string /*id*/ id, Unit unit, LocationFlag locationFlag)
        {
        }

        /// <summary>sets the field of view</summary>
        public static void camera_set_field_of_view(float real, short value)
        {
        }

        /// <summary>returns the number of ticks remaining in the current camera interpolation.</summary>
        public static short camera_time()
        {
            return default(short);
        }

        /// <summary>gives a specific player active camouflage</summary>
        public static void cheat_active_camouflage_by_player(short value, bool boolean)
        {
        }

        /// <summary>clone the first player's most reasonable weapon and attach it to the specified object's marker</summary>
        public static void cinematic_clone_players_weapon(Entity entity, string /*id*/ string_id, string /*id*/ string_id1)
        {
        }

        /// <summary>enable/disable ambience details in cinematics</summary>
        public static void cinematic_enable_ambience_details(bool boolean)
        {
        }

        /// <summary>sets the color (red, green, blue) of the cinematic ambient light.</summary>
        public static void cinematic_lighting_set_ambient_light(float real, float real1, float real12)
        {
        }

        /// <summary>sets the pitch, yaw, and color (red, green, blue) of the cinematic shadowing diffuse and specular directional light.</summary>
        public static void cinematic_lighting_set_primary_light(float real, float real1, float real12, float real123, float real1234)
        {
        }

        /// <summary>sets the pitch, yaw, and color (red, green, blue) of the cinematic non-shadowing diffuse directional light.</summary>
        public static void cinematic_lighting_set_secondary_light(float real, float real1, float real12, float real123, float real1234)
        {
        }

        /// <summary>turn off lightmap shadow in cinematics</summary>
        public static void cinematic_lightmap_shadow_disable()
        {
        }

        /// <summary>turn on lightmap shadow in cinematics</summary>
        public static void cinematic_lightmap_shadow_enable()
        {
        }

        /// <summary>flag this cutscene as an outro cutscene</summary>
        public static void cinematic_outro_start()
        {
        }

        /// <summary>transition-time</summary>
        public static void cinematic_screen_effect_set_crossfade(float real)
        {
        }

        /// <summary>sets dof: <seperation dist>, <near blur lower bound> <upper bound> <time> <far blur lower bound> <upper bound> <time></summary>
        public static void cinematic_screen_effect_set_depth_of_field(float real, float real1, float real12, float real123, float real1234, float real12345, float real123456)
        {
        }

        /// <summary>starts screen effect pass TRUE to clear</summary>
        public static void cinematic_screen_effect_start(bool boolean)
        {
        }

        /// <summary>returns control of the screen effects to the rest of the game</summary>
        public static void cinematic_screen_effect_stop()
        {
        }

        /// <summary></summary>
        public static void cinematic_set_far_clip_distance(float real)
        {
        }

        /// <summary></summary>
        public static void cinematic_set_near_clip_distance(float real)
        {
        }

        /// <summary>activates the chapter title</summary>
        public static void cinematic_set_title(CinematicTitle cutscene_title)
        {
        }

        /// <summary>sets or removes the letterbox bars</summary>
        public static void cinematic_show_letterbox(bool boolean)
        {
        }

        /// <summary>sets or removes the letterbox bars</summary>
        public static void cinematic_show_letterbox_immediate(bool boolean)
        {
        }

        /// <summary></summary>
        public static void cinematic_skip_start_internal()
        {
        }

        /// <summary></summary>
        public static void cinematic_skip_stop_internal()
        {
        }

        /// <summary>initializes game to start a cinematic (interruptive) cutscene</summary>
        public static void cinematic_start()
        {
        }

        /// <summary>initializes the game to end a cinematic (interruptive) cutscene</summary>
        public static void cinematic_stop()
        {
        }

        /// <summary>displays the named subtitle for <real> seconds</summary>
        public static void cinematic_subtitle(string /*id*/ string_id, float real)
        {
        }

        /// <summary>returns TRUE if player0's look pitch is inverted</summary>
        public static bool controller_get_look_invert()
        {
            return default(bool);
        }

        /// <summary>invert player0's look</summary>
        public static void controller_set_look_invert()
        {
        }

        /// <summary>invert player0's look</summary>
        public static void controller_set_look_invert(bool boolean)
        {
        }

        /// <summary>Command script ends prematurely when actor's combat status raises to 'alert' or higher</summary>
        public static void cs_abort_on_alert(bool boolean)
        {
        }

        /// <summary>Command script ends prematurely when actor's combat status rises to given level</summary>
        public static void cs_abort_on_combat_status(short value)
        {
        }

        /// <summary>Command script ends prematurely when actor is damaged</summary>
        public static void cs_abort_on_damage(bool boolean)
        {
        }

        /// <summary>Actor aims at the point for the remainder of the cs, or until overridden (overrides look)</summary>
        public static void cs_aim(bool boolean, SpatialPoint point)
        {
        }

        /// <summary>Actor aims at the object for the duration of the cs, or until overridden (overrides look)</summary>
        public static void cs_aim_object(bool boolean, Entity entity)
        {
        }

        /// <summary>Actor aims at the object for the duration of the cs, or until overridden (overrides look)</summary>
        public static void cs_aim_object(bool boolean)
        {
        }

        /// <summary>Actor aims at nearest player for the duration of the cs, or until overridden (overrides look)</summary>
        public static void cs_aim_player(bool boolean)
        {
        }

        /// <summary></summary>
        public static void cs_approach(Entity entity, float real, float real1, float real12)
        {
        }

        /// <summary></summary>
        public static void cs_approach(float floatValue, float floatValue1, float floatValue2)
        {
        }

        /// <summary></summary>
        public static void cs_approach_player(float real, float real1, float real12)
        {
        }

        /// <summary>Actor stops approaching</summary>
        public static void cs_approach_stop()
        {
        }

        /// <summary>Returns true if the command script is in the ai's cs queue</summary>
        public static bool cs_command_script_queued(AI ai, AIScript ai_command_script)
        {
            return default(bool);
        }

        /// <summary>Returns true if the ai is running the command script in question</summary>
        public static bool cs_command_script_running(AI ai, AIScript ai_command_script)
        {
            return default(bool);
        }

        /// <summary>Actor crouches for the remainder of the command script, or until overridden</summary>
        public static void cs_crouch(bool boolean)
        {
        }

        /// <summary>starts a custom animation playing on the unit (interpolates into animation if last parameter is TRUE)</summary>
        public static void cs_custom_animation(Animation animation, string /*id*/ emotion, float floatValue, bool interpolate)
        {
        }

        /// <summary>Deploy a turret at the given script point</summary>
        public static void cs_deploy_turret(SpatialPoint point)
        {
        }

        /// <summary>Actor combat dialogue enabled/disabled.</summary>
        public static void cs_enable_dialogue(bool boolean)
        {
        }

        /// <summary>Actor autonomous looking enabled/disabled.</summary>
        public static void cs_enable_looking(bool boolean)
        {
        }

        /// <summary>Actor autonomous moving enabled/disabled.</summary>
        public static void cs_enable_moving(bool boolean)
        {
        }

        /// <summary>Actor blocks until pathfinding calls succeed</summary>
        public static void cs_enable_pathfinding_failsafe(bool boolean)
        {
        }

        /// <summary>Actor autonomous target selection enabled/disabled.</summary>
        public static void cs_enable_targeting(bool boolean)
        {
        }

        /// <summary>Actor faces exactly the point for the remainder of the cs, or until overridden (overrides aim, look)</summary>
        public static void cs_face(bool boolean, SpatialPoint point = null)
        {
        }

        /// <summary>Actor faces exactly the given object for the duration of the cs, or until overridden (overrides aim, look)</summary>
        public static void cs_face_object(bool boolean, Entity entity)
        {
        }

        /// <summary>Actor faces exactly the nearest player for the duration of the cs, or until overridden (overrides aim, look)</summary>
        public static void cs_face_player(bool boolean)
        {
        }

        /// <summary>Flies the actor through the given point</summary>
        public static void cs_fly_by()
        {
        }

        /// <summary>Flies the actor through the given point</summary>
        public static void cs_fly_by(SpatialPoint point, float tolerance = 0f)
        {
        }

        /// <summary>Flies the actor to the given point (within the given tolerance)</summary>
        public static void cs_fly_to(SpatialPoint point, float tolerance = 0f)
        {
        }

        /// <summary>Flies the actor to the given point and orients him in the appropriate direction (within the given tolerance)</summary>
        public static void cs_fly_to_and_face(SpatialPoint point, SpatialPoint face, float tolerance = 0f)
        {
        }

        /// <summary>Force the actor's combat status (0= no override, 1= asleep, 2=idle, 3= alert, 4= active)</summary>
        public static void cs_force_combat_status(short value)
        {
        }

        /// <summary>Actor moves toward the point, and considers it hit when it breaks the indicated plane</summary>
        public static void cs_go_by(SpatialPoint point, SpatialPoint planeP, float planeD = 0f)
        {
        }

        /// <summary>Moves the actor to a specified point</summary>
        public static void cs_go_to(SpatialPoint point, float tolerance = 1f)
        {
        }

        /// <summary>Moves the actor to a specified point and has him face the second point</summary>
        public static void cs_go_to_and_face(SpatialPoint point, SpatialPoint faceTowards)
        {
        }

        /// <summary>Given a point set, AI goes toward the nearest point</summary>
        public static void cs_go_to_nearest(SpatialPoint destination)
        {
        }

        /// <summary>Actor gets in the appropriate vehicle</summary>
        public static void cs_go_to_vehicle()
        {
        }

        /// <summary>Actor gets in the appropriate vehicle</summary>
        public static void cs_go_to_vehicle(Vehicle vehicle)
        {
        }

        /// <summary>Actor throws a grenade, either by tossing (arg2=0), lobbing (1) or bouncing (2)</summary>
        public static void cs_grenade(SpatialPoint point, int action)
        {
        }

        /// <summary>Actor does not avoid obstacles when true</summary>
        public static void cs_ignore_obstacles(bool boolean)
        {
        }

        /// <summary>Actor jumps in direction of angle at the given velocity (angle, velocity)</summary>
        public static void cs_jump(float real, float real1)
        {
        }

        /// <summary>Actor jumps with given horizontal and vertical velocity</summary>
        public static void cs_jump_to_point(float real, float real1)
        {
        }

        /// <summary>Actor looks at the point for the remainder of the cs, or until overridden</summary>
        public static void cs_look(bool boolean, SpatialPoint point = null)
        {
        }

        /// <summary>Actor looks at the object for the duration of the cs, or until overridden</summary>
        public static void cs_look_object(bool boolean)
        {
        }

        /// <summary>Actor looks at the object for the duration of the cs, or until overridden</summary>
        public static void cs_look_object(bool boolean, Entity entity)
        {
        }

        /// <summary>Actor looks at nearest player for the duration of the cs, or until overridden</summary>
        public static void cs_look_player(bool boolean)
        {
        }

        /// <summary>Actor moves at given angle, for the given distance, optionally with the given facing (angle, distance, facing)</summary>
        public static void cs_move_in_direction(float real, float real1, float real12)
        {
        }

        /// <summary>Actor switches to given animation mode</summary>
        public static void cs_movement_mode(short value)
        {
        }

        /// <summary>Returns TRUE if the actor is currently following a path</summary>
        public static bool cs_moving()
        {
            return default(bool);
        }

        /// <summary>The actor does nothing for the given number of seconds</summary>
        public static void cs_pause(float real)
        {
        }

        /// <summary>Play the named line in the current scene</summary>
        public static void cs_play_line(string /*id*/ string_id)
        {
        }

        /// <summary>Add a command script onto the end of an actor's command script queue</summary>
        public static void cs_queue_command_script(AI ai, AIScript ai_command_script)
        {
        }

        /// <summary>Causes the specified actor(s) to start executing a command script immediately (discarding any other command scripts in the queue)</summary>
        public static void cs_run_command_script(AI ai, AIScript ai_command_script)
        {
        }

        /// <summary>Actor performs the indicated behavior</summary>
        public static void cs_set_behavior(AIBehavior ai_behavior)
        {
        }

        /// <summary>Actor is allowed to shoot at its target or not</summary>
        public static void cs_shoot(bool boolean)
        {
        }

        /// <summary>Actor is allowed to shoot at its target or not</summary>
        public static void cs_shoot(bool boolean, Entity entity)
        {
        }

        /// <summary>Actor shoots at given point</summary>
        public static void cs_shoot_point(bool boolean, SpatialPoint point)
        {
        }

        /// <summary>Push a command script to the top of the actor's command script queue</summary>
        public static void cs_stack_command_script(AI ai, AIScript ai_command_script)
        {
        }

        /// <summary></summary>
        public static void cs_start_approach(Entity entity, float real, float real1, float real12)
        {
        }

        /// <summary></summary>
        public static void cs_start_approach_player(float real, float real1, float real12)
        {
        }

        /// <summary>Moves the actor to a specified point. DOES NOT BLOCK SCRIPT EXECUTION.</summary>
        public static void cs_start_to(SpatialPoint destination)
        {
        }

        /// <summary>Stop running a custom animation</summary>
        public static void cs_stop_custom_animation()
        {
        }

        /// <summary>Combat dialogue is suppressed for the remainder of the command script</summary>
        public static void cs_suppress_dialogue_global(bool boolean)
        {
        }

        /// <summary>Switch control of the joint command script to the given member</summary>
        public static void cs_switch(string /*id*/ string_id)
        {

        }
        /// <summary>Actor teleports to point1 facing point2</summary>
        public static void cs_teleport(SpatialPoint destination, SpatialPoint facing)
        {
        }

        /// <summary>Set the sharpness of a vehicle turn (values 0 -> 1). Only applicable to nondirectional flying vehicles (e.g. dropships)</summary>
        public static void cs_turn_sharpness(bool boolean, float real)
        {
        }

        /// <summary>Enables or disables boost</summary>
        public static void cs_vehicle_boost(bool boolean)
        {
        }

        /// <summary>Set the speed at which the actor will drive a vehicle, expressed as a multiplier 0-1</summary>
        public static void cs_vehicle_speed(float real)
        {
        }

        /// <summary>starts a custom animation playing on a unit (interpolates into animation if last parameter is TRUE)</summary>
        public static void custom_animation(Unit unit, Animation animation, string /*id*/ stringid, bool interpolate)
        {
        }

        /// <summary>starts a custom animation playing on a unit (interpolates into animation if last parameter is TRUE)</summary>
        public static void custom_animation(Unit unit, string /*id*/ emotion, bool interpolate)
        {
        }

        /// <summary>starts a custom animation playing on a unit (interpolates into animation if last parameter is TRUE)</summary>
        public static void custom_animation_loop(Unit unit, Animation animation1, string /*id*/ emotion, bool interpolate)
        {
        }

        /// <summary>starts a custom animation playing on a unit (interpolates into animation if last parameter is TRUE)</summary>
        public static void custom_animation_loop(string /*id*/ emotion, bool interpolate)
        {
        }

        /// <summary>starts a custom animation relative to some other object (interpolates into animation if last parameter is TRUE)</summary>
        public static void custom_animation_relative(Unit unit, string /*id*/ emotion, bool interpolate, Entity entity)
        {
        }

        /// <summary>starts a custom animation relative to some other object (interpolates into animation if last parameter is TRUE)</summary>
        public static void custom_animation_relative(Unit entity, Animation animation, string /*id*/ emotion, bool boolean, Entity other)
        {
        }

        /// <summary>starts a custom animation relative to some other object (interpolates into animation if last parameter is TRUE)</summary>
        public static void custom_animation_relative_loop(Unit unit, Animation animation2, string /*id*/ emotion, bool boolean, Entity entity)
        {
        }

        /// <summary>causes the specified damage at the specified flag.</summary>
        public static void damage_new(Damage damage, LocationFlag cutscene_flag)
        {
        }

        /// <summary>causes the specified damage at the specified object.</summary>
        public static void damage_object(Damage damage, Entity entity)
        {
        }

        /// <summary>damages all players with the given damage effect</summary>
        public static void damage_players(Damage damage)
        {
        }

        /// <summary>sets the mission segment for single player data mine events</summary>
        public static void data_mine_set_mission_segment(string value)
        {
        }

        /// <summary>deactivates a nav point type attached to a team anchored to a flag</summary>
        public static void deactivate_team_nav_point_flag(Team team, LocationFlag cutscene_flag)
        {
        }

        /// <summary>deactivates a nav point type attached to a team anchored to an object</summary>
        public static void deactivate_team_nav_point_object(Team team)
        {
        }

        /// <summary>deactivates a nav point type attached to a team anchored to an object</summary>
        public static void deactivate_team_nav_point_object(Team team, Entity entity)
        {
        }

        /// <summary>animate the overlay over time</summary>
        public static void device_animate_overlay(Device device, float real, float real1, float real12, float real123)
        {
        }

        /// <summary>animate the position over time</summary>
        public static void device_animate_position(Device device, float real, float real1, float real12, float real123, bool boolean)
        {
        }

        /// <summary>animate the position over time</summary>
        public static void device_animate_position(Device device, float floatValue, float floatValue0, float floatValue1, bool boolean)
        {
        }

        /// <summary>TRUE makes the given device close automatically after it has opened, FALSE makes it not</summary>
        public static void device_closes_automatically_set(Device device, bool boolean)
        {
        }

        /// <summary>gets the current position of the given device (used for devices without explicit device groups)</summary>
        public static float device_get_position(Device device)
        {
            return default(float);
        }

        /// <summary>TRUE allows a device to change states only once</summary>
        public static void device_group_change_only_once_more_set(DeviceGroup device_group, bool boolean)
        {
        }

        /// <summary>returns the desired value of the specified device group.</summary>
        public static float device_group_get(DeviceGroup device_group)
        {
            return default(float);
        }

        /// <summary>changes the desired value of the specified device group.</summary>
        public static void device_group_set(Device device, DeviceGroup device_group, float real)
        {
        }

        /// <summary>instantaneously changes the value of the specified device group.</summary>
        public static void device_group_set_immediate(DeviceGroup device_group, float real)
        {
        }

        /// <summary>TRUE makes the given device one-sided (only able to be opened from one direction), FALSE makes it two-sided</summary>
        public static void device_one_sided_set(Device device, bool boolean)
        {
        }

        /// <summary>TRUE makes the given device open automatically when any biped is nearby, FALSE makes it not</summary>
        public static void device_operates_automatically_set(Device device, bool boolean)
        {
        }

        /// <summary>changes a machine's never_appears_locked flag, but only if paul is a bastard</summary>
        public static void device_set_never_appears_locked(Device device, bool boolean)
        {
        }

        /// <summary>set the desired overlay animation to use</summary>
        public static void device_set_overlay_track(Device device, string /*id*/ string_id)
        {
        }

        /// <summary>set the desired position of the given device (used for devices without explicit device groups)</summary>
        public static void device_set_position(Device device, float real)
        {
        }

        /// <summary>instantaneously changes the position of the given device (used for devices without explicit device groups</summary>
        public static void device_set_position_immediate(Device device, float real)
        {
        }

        /// <summary>set the desired position track animation to use (optional interpolation time onto track)</summary>
        public static void device_set_position_track(Device device, string /*id*/ string_id, float real)
        {
        }

        /// <summary>immediately sets the power of a named device to the given value</summary>
        public static void device_set_power(Device device, float real)
        {
        }

        public static void disable_render_light_suppressor()
        {
        }

        /// <summary>drops the named tag e.g. objects\vehicles\banshee\banshee.vehicle</summary>
        public static void drop(string value)
        {
        }

        /// <summary>starts the specified effect at the specified flag.</summary>
        public static void effect_new(Effect effect, LocationFlag cutscene_flag)
        {
        }

        /// <summary>starts the specified effect on the specified object at the specified marker.</summary>
        public static void effect_new_on_object_marker(Effect effect, Entity entity, string /*id*/ string_id)
        {
        }

        /// <summary>starts the specified effect on the specified object at the specified marker.</summary>
        public static void effect_new_on_object_marker(Effect effect, string /*id*/ emotion)
        {
        }

        /// <summary>enables the code that constrains the max # active lights</summary>
        public static void enable_render_light_suppressor()
        {
        }

        /// <summary>does a screen fade in from a particular color</summary>
        public static void fade_in(float real, float real1, float real12, short value)
        {
        }

        /// <summary>does a screen fade out to a particular color</summary>
        public static void fade_out(float real, float real1, float real12, short value)
        {
        }

        /// <summary>The flock starts producing boids</summary>
        public static void flock_start(string /*id*/ string_id)
        {
        }

        /// <summary>The flock stops producing boids</summary>
        public static void flock_stop(string /*id*/ string_id)
        {
        }

        /// <summary>allows or disallows the user of player flashlights</summary>
        public static void game_can_use_flashlights(bool boolean)
        {
        }

        /// <summary>returns the current difficulty setting, but lies to you and will never return easy, instead returning normal</summary>
        // public static GameDifficulty game_difficulty_get()
        // {
        //     return default(GameDifficulty);
        // }
        public static string game_difficulty_get()
        {
            return "";
        }

        /// <summary>returns the actual current difficulty setting without lying</summary>
        public static string game_difficulty_get_real()
        {
            return default(string);
        }

        /// <summary>returns TRUE if the game is cooperative</summary>
        public static bool game_is_cooperative()
        {
            return default(bool);
        }

        /// <summary>returns the hs global boolean 'global_playtest_mode' which can be set in your init.txt</summary>
        public static bool game_is_playtest()
        {
            return default(bool);
        }

        /// <summary>causes the player to revert to his previous saved game (for testing, the first bastard that does this to me gets it in the head)</summary>
        public static void game_revert()
        {
        }

        /// <summary>don't use this for anything, you black-hearted bastards.</summary>
        public static bool game_reverted()
        {
            return default(bool);
        }

        /// <summary>returns FALSE if it would be a bad idea to save the player's game right now</summary>
        public static bool game_safe_to_save()
        {
            return default(bool);
        }

        /// <summary>checks to see if it is safe to save game, then saves (gives up after 8 seconds)</summary>
        public static void game_save()
        {
        }

        /// <summary>cancels any pending game_save, timeout or not</summary>
        public static void game_save_cancel()
        {
        }

        /// <summary>don't use this, except in one place.</summary>
        public static void game_save_cinematic_skip()
        {
        }

        /// <summary>disregards player's current situation and saves (BE VERY CAREFUL!)</summary>
        public static void game_save_immediate()
        {
        }

        /// <summary>checks to see if it is safe to save game, then saves (this version never gives up)</summary>
        public static void game_save_no_timeout()
        {
        }

        /// <summary>checks to see if the game is trying to save the map.</summary>
        public static bool game_saving()
        {
            return default(bool);
        }

        /// <summary>causes the player to successfully finish the current level and move to the next</summary>
        public static void game_won()
        {
        }

        /// <summary>causes all garbage objects except those visible to a player to be collected immediately</summary>
        public static void garbage_collect_now()
        {
        }

        /// <summary>forces all garbage objects to be collected immediately, even those visible to a player (dangerous!)</summary>
        public static void garbage_collect_unsafe()
        {
        }

        /// <summary>we fear change</summary>
        public static void geometry_cache_flush()
        {
        }

        /// <summary>parameter 1 is how, parameter 2 is when</summary>
        public static void hud_cinematic_fade(float real, float real1)
        {
        }

        /// <summary>true turns training on, false turns it off.</summary>
        public static void hud_enable_training(bool boolean)
        {
        }

        /// <summary>sets the string id fo the scripted training text</summary>
        public static void hud_set_training_text(string /*id*/ string_id)
        {
        }

        /// <summary>true turns on scripted training text</summary>
        public static void hud_show_training_text(bool boolean)
        {
        }

        /// <summary></summary>
        public static bool ice_cream_flavor_available(int value)
        {
            return default(bool);
        }

        /// <summary></summary>
        public static void ice_cream_flavor_stock(int value)
        {
        }

        /// <summary><name> <final value> <time></summary>
        public static void interpolator_start(string /*id*/ string_id, float real, float real1)
        {
        }

        /// <summary>disables a kill volume</summary>
        public static void kill_volume_disable(Trigger trigger_volume)
        {
        }

        /// <summary>enables a kill volume</summary>
        public static void kill_volume_enable(Trigger trigger_volume)
        {
        }

        /// <summary>returns the number of objects in a list</summary>
        public static short list_count(Entity e)
        {
            return (short)(e == null ? 0 : 1);
        }

        /// <summary>returns the number of objects in a list</summary>
        public static short list_count(ObjectList object_list)
        {
            return default(short);
        }

        /// <summary>returns the number of objects in a list that aren't dead</summary>
        public static short list_count_not_dead(ObjectList objects)
        {
            return default(short);
        }

        /// <summary>returns an item in an object list.</summary>
        public static Entity list_get(ObjectList object_list, int index)
        {
            return default(Entity);
        }

        /// <summary>sets the next loading screen to just fade to white</summary>
        public static void loading_screen_fade_to_white()
        {
        }

        /// <summary>starts the map from the beginning.</summary>
        public static void map_reset()
        {
        }

        /// <summary>returns the maximum of all specified expressions.</summary>
        public static float max(float a, float b)
        {
            return default(float);
        }

        /// <summary>returns the maximum of all specified expressions.</summary>
        public static short max(short a, short b)
        {
            return default(short);
        }

        /// <summary>returns the minimum of all specified expressions.</summary>
        public static float min(float a, float b)
        {
            return default(float);
        }

        /// <summary>returns the minimum of all specified expressions.</summary>
        public static short min(short a, short b)
        {
            return default(short);
        }

        /// <summary>returns the object attached to the marker of the given parent object</summary>
        public static Entity entity_at_marker(Entity entity, string /*id*/ string_id)
        {
            return default(Entity);
        }

        public static Entity object_at_marker(Entity entity, string stringId)
        {
            return default(Entity);
        }

        /// <summary>allows an object to take damage again</summary>
        public static void object_can_take_damage(Entity entity)
        {
        }

        /// <summary>allows an object to take damage again</summary>
        public static void object_can_take_damage(ObjectList object_list)
        {
        }

        /// <summary>Set whether the object can die from damage or not (as opposed to by scripting)</summary>
        public static void object_cannot_die(Entity entity, bool boolean)
        {
        }

        /// <summary>Set whether the object can die from damage or not (as opposed to by scripting)</summary>
        public static void object_cannot_die(bool boolean)
        {
        }

        /// <summary>prevents an object from taking damage</summary>
        public static void object_cannot_take_damage(Entity entity) // Unit?
        {
        }

        /// <summary>prevents an object from taking damage</summary>
        public static void object_cannot_take_damage(ObjectList object_list)
        {
        }

        /// <summary>makes an object use the highest lod for the remainder of the levels' cutscenes.</summary>
        public static void object_cinematic_lod(Entity entity, bool boolean)
        {
        }

        /// <summary>makes an object use the highest lod for the remainder of the levels' cutscenes.</summary>
        public static void object_cinematic_lod(bool boolean)
        {
        }

        /// <summary>makes an object bypass visibility and always render during cinematics.</summary>
        public static void object_cinematic_visibility(Entity entity, bool boolean)
        {
        }

        /// <summary>clears all funciton variables for sin-o-matic use</summary>
        public static void object_clear_all_function_variables(Entity entity)
        {
        }

        /// <summary>clears one funciton variables for sin-o-matic use</summary>
        public static void object_clear_function_variable(Entity entity, string /*id*/ string_id)
        {
        }

        /// <summary>creates an object from the scenario.</summary>
        public static void object_create(EntityIdentifier object_name)
        {
        }

        /// <summary>creates an object from the scenario.</summary>
        public static void object_create(Entity object_name)
        {
        }

        /// <summary>creates an object, destroying it first if it already exists.</summary>
        public static void object_create_anew(EntityIdentifier object_name)
        {
        }

        /// <summary>creates an object, destroying it first if it already exists.</summary>
        public static void object_create_anew(Entity entity)
        {
        }

        /// <summary>creates anew all objects from the scenario whose names contain the given substring.</summary>
        public static void object_create_anew_containing(string value)
        {
        }

        /// <summary>creates an object, potentially resulting in multiple objects if it already exists.</summary>
        public static void object_create_clone(EntityIdentifier object_name)
        {
        }

        /// <summary>creates all objects from the scenario whose names contain the given substring.</summary>
        public static void object_create_containing(string value)
        {
        }

        /// <summary>applies damage to a damage section, causing all manner of effects/constraint breakage to occur</summary>
        public static void object_damage_damage_section(Entity entity, string /*id*/ string_id, float real)
        {
        }

        /// <summary>applies damage to a damage section, causing all manner of effects/constraint breakage to occur</summary>
        public static void object_damage_damage_section(string /*id*/ emotion, float floatValue)
        {
        }

        /// <summary>destroys an object.</summary>
        public static void object_destroy(Entity entity)
        {
        }

        /// <summary>destroys an object.</summary>
        public static void object_destroy()
        {
        }

        /// <summary>destroys all objects from the scenario whose names contain the given substring.</summary>
        public static void object_destroy_containing(string value)
        {
        }

        /// <summary>destroys all objects matching the type mask</summary>
        public static void object_destroy_type_mask(int value)
        {
        }

        /// <summary>disabled dynamic simulation for this object (makes it fixed)</summary>
        public static void object_dynamic_simulation_disable(bool boolean)
        {
        }

        /// <summary>disabled dynamic simulation for this object (makes it fixed)</summary>
        public static void object_dynamic_simulation_disable(Entity entity, bool boolean)
        {
        }

        /// <summary>returns the parent of the given object</summary>
        public static Entity object_get_parent(Entity entity)
        {
            return default(Entity);
        }

        /// <summary>returns the ai attached to this object, if any</summary>
        public static AI object_get_ai()
        {
            return default(AI);
        }

        /// <summary>returns the ai attached to this object, if any</summary>
        public static AI object_get_ai(Entity entity)
        {
            return default(AI);
        }

        /// <summary>returns the health [0,1] of the object, returns -1 if the object does not exist</summary>
        public static float object_get_health(Entity entity)
        {
            return default(float);
        }

        /// <summary>returns the health [0,1] of the object, returns -1 if the object does not exist</summary>
        public static float object_get_health()
        {
            return default(float);
        }

        /// <summary>returns the parent of the given object</summary>
        public static Entity entity_get_parent()
        {
            return default(Entity);
        }

        /// <summary>returns the parent of the given object</summary>
        public static Entity entity_get_parent(Entity entity)
        {
            return default(Entity);
        }

        /// <summary>returns the shield [0,1] of the object, returns -1 if the object does not exist</summary>
        public static float object_get_shield(Entity entity)
        {
            return default(float);
        }

        /// <summary>returns the shield [0,1] of the object, returns -1 if the object does not exist</summary>
        public static float object_get_shield()
        {
            return default(float);
        }

        /// <summary>hides or shows the object passed in</summary>
        public static void object_hide(Entity entity, bool boolean)
        {
        }

        /// <summary>returns TRUE if the specified model target is destroyed</summary>
        public static short object_model_targets_destroyed(Entity entity, string /*id*/ target)
        {
            return default(short);
        }

        /// <summary>when this object deactivates it will be deleted</summary>
        public static void object_set_deleted_when_deactivated(Entity entity)
        {
        }

        /// <summary>sets funciton variable for sin-o-matic use</summary>
        public static void object_set_function_variable(Entity entity, string /*id*/ string_id, float real, float real1)
        {
        }

        /// <summary>sets funciton variable for sin-o-matic use</summary>
        public static void object_set_function_variable(string /*id*/ emotion, float floatValue0, float floatValue1)
        {
        }

        /// <summary>sets the desired region (use "" for all regions) to the permutation with the given name, e.g. (object_set_permutation flood "right arm" ~damaged)</summary>
        public static void object_set_permutation(Entity entity, string /*id*/ string_id, string /*id*/ string_id1)
        {
        }

        /// <summary>sets the desired region (use "" for all regions) to the permutation with the given name, e.g. (object_set_permutation flood "right arm" ~damaged)</summary>
        public static void object_set_permutation(string /*id*/ emotion, string /*id*/ emotion1)
        {
        }

        /// <summary>sets phantom power to be latched at 1.0f or 0.0f</summary>
        public static void object_set_phantom_power(bool boolean)
        {
        }

        /// <summary>sets phantom power to be latched at 1.0f or 0.0f</summary>
        public static void object_set_phantom_power(Entity entity, bool boolean)
        {
        }

        /// <summary>sets the desired region (use "" for all regions) to the model state with the given name, e.g. (object_set_region_state marine head destroyed)</summary>
        public static void object_set_region_state(Entity entity, string /*id*/ string_id, DamageState model_state)
        {
        }

        /// <summary>sets the scale for a given object and interpolates over the given number of frames to achieve that scale</summary>
        public static void object_set_scale(float floatValue, short valueValue)
        {
        }

        /// <summary>sets the scale for a given object and interpolates over the given number of frames to achieve that scale</summary>
        public static void object_set_scale(Entity entity, float real, short value)
        {
        }

        /// <summary>sets the shield vitality of the specified object (between 0 and 1).</summary>
        public static void object_set_shield(Entity entity, float real)
        {
        }

        /// <summary>sets the shield vitality of the specified object (between 0 and 1).</summary>
        public static void object_set_shield()
        {
        }

        /// <summary>make this objects shield be stunned permanently</summary>
        public static void object_set_shield_stun_infinite(Entity entity)
        {
        }

        /// <summary>Sets the (object-relative) forward velocity of the given object</summary>
        public static void object_set_velocity(Entity entity, float real)
        {
        }

        /// <summary>Sets the (object-relative) forward velocity of the given object</summary>
        public static void object_set_velocity(Entity entity, float real, float real1, float real12)
        {
        }

        /// <summary>moves the specified object to the specified flag.</summary>
        public static void object_teleport(Entity entity, LocationFlag cutscene_flag)
        {
        }

        /// <summary>loads textures necessary to draw an object that's about to come on-screen.</summary>
        public static void object_type_predict(Entity entity)
        {
        }

        /// <summary>loads textures necessary to draw an object that's about to come on-screen.</summary>
        public static void object_type_predict_high(Entity entity)
        {
        }

        /// <summary>makes an object use the cinematic directional and ambient lights instead of sampling the lightmap.</summary>
        public static void object_uses_cinematic_lighting(Entity entity, bool boolean)
        {
        }

        /// <summary>makes an object use the cinematic directional and ambient lights instead of sampling the lightmap.</summary>
        public static void object_uses_cinematic_lighting(bool boolean)
        {
        }

        /// <summary>clears the mission objectives.</summary>
        public static void objectives_clear()
        {
        }

        /// <summary>mark objectives 0..n as complete</summary>
        public static void objectives_finish_up_to(int value)
        {
        }

        /// <summary>show objectives 0..n</summary>
        public static void objectives_show_up_to(int value)
        {
        }

        /// <summary>attaches the second object to the first both strings can be empty</summary>
        public static void objects_attach(Entity entity, string /*id*/ string_id, Entity entity1, string /*id*/ string_id1)
        {
        }

        /// <summary>attaches the second object to the first both strings can be empty</summary>
        public static void objects_attach(Entity entity, string /*id*/ emotion0, string /*id*/ emotion1)
        {
        }

        /// <summary>attaches the second object to the first both strings can be empty</summary>
        public static void objects_attach(string /*id*/ emotion0, Entity entity, string /*id*/ emotion1)
        {
        }

        /// <summary>attaches the second object to the first both strings can be empty</summary>
        public static void objects_attach(string /*id*/ emotion0, string /*id*/ emotion1)
        {
        }

        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the flag.</summary>
        public static bool objects_can_see_flag(Entity entity, LocationFlag locationFlag, float floatValue)
        {
            return default(bool);
        }

        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the flag.</summary>
        public static bool objects_can_see_flag(ObjectList list, LocationFlag locationFlag, float floatValue)
        {
            return default(bool);
        }

        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the object.</summary>
        public static bool objects_can_see_object(Entity entity, EntityIdentifier obj, float degrees)
        {
            return default(bool);
        }

        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the object.</summary>
        public static bool objects_can_see_object(ObjectList list, EntityIdentifier obj, float degrees)
        {
            return default(bool);
        }

        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the object.</summary>
        public static bool objects_can_see_object(float floatValue)
        {
            return default(bool);
        }

        /// <summary>detaches from the given parent object the given child object</summary>
        public static void objects_detach(Entity entity, Entity entity1)
        {
        }

        /// <summary>detaches from the given parent object the given child object</summary>
        public static void objects_detach(Entity entity)
        {
        }

        /// <summary>detaches from the given parent object the given child object</summary>
        public static void objects_detach()
        {
        }

        /// <summary>returns minimum distance from any of the specified objects to the specified flag. (returns -1 if there are no objects, or no flag, to check)</summary>
        public static float objects_distance_to_flag(ObjectList list, LocationFlag locationFlag)
        {
            return default(float);
        }

        /// <summary>returns minimum distance from any of the specified objects to the specified flag. (returns -1 if there are no objects, or no flag, to check)</summary>
        public static float objects_distance_to_flag(Entity entity, LocationFlag locationFlag)
        {
            return default(float);
        }

        /// <summary>returns minimum distance from any of the specified objects to the specified destination object. (returns -1 if there are no objects to check)</summary>
        public static float objects_distance_to_object(ObjectList list, Entity entity)
        {
            return default(float);
        }

        /// <summary>loads textures/geometry/sounds necessary to present objects that are about to come on-screen</summary>
        public static void objects_predict(ObjectList object_list)
        {
        }

        /// <summary>loads textures/geometry/sounds necessary to present objects that are about to come on-screen</summary>
        public static void objects_predict(Entity entity)
        {
        }

        /// <summary>loads textures/geometry/sounds necessary to present objects that are about to come on-screen</summary>
        public static void objects_predict_high(Entity entity)
        {
        }

        /// <summary>turn off ground adhesion forces so you can play tricks with gravity</summary>
        public static void physics_disable_character_ground_adhesion_forces(float real)
        {
        }

        /// <summary>set global gravity acceleration relative to halo standard gravity</summary>
        public static void physics_set_gravity(float real)
        {
        }

        /// <summary>sets a local frame of motion for updating physics of things that wish to respect it</summary>
        public static void physics_set_velocity_frame(float real, float real1, float real12)
        {
        }

        /// <summary>returns the first value pinned between the second two</summary>
        public static short pin(short value, short min, short max)
        {
            return default(short);
        }

        /// <summary>returns the first value pinned between the second two</summary>
        public static float pin(float value, float min, float max)
        {
            return default(float);
        }


        /// <summary>ur...</summary>
        public static void play_credits()
        {
        }

        /// <summary>returns true if any player has hit accept since the last call to (player_action_test_reset).</summary>
        public static bool player_action_test_accept()
        {
            return default(bool);
        }

        /// <summary>returns true if any player has hit cancel key since the last call to (player_action_test_reset).</summary>
        public static bool player_action_test_cancel()
        {
            return default(bool);
        }

        /// <summary>returns true if any player has used grenade trigger since the last call to (player_action_test_reset).</summary>
        public static bool player_action_test_grenade_trigger()
        {
            return default(bool);
        }

        /// <summary>sets down player look down test</summary>
        public static void player_action_test_look_down_begin()
        {
        }

        /// <summary>ends the look pitch testing</summary>
        public static void player_action_test_look_pitch_end()
        {
        }

        /// <summary>sets up player look up test</summary>
        public static void player_action_test_look_up_begin()
        {
        }

        /// <summary>true if the first player pushed backward on lookstick</summary>
        public static bool player_action_test_lookstick_backward()
        {
            return default(bool);
        }

        /// <summary>true if the first player pushed forward on lookstick</summary>
        public static bool player_action_test_lookstick_forward()
        {
            return default(bool);
        }

        /// <summary>returns true if any player has hit the melee button since the last call to (player_action_test_reset).</summary>
        public static bool player_action_test_melee()
        {
            return default(bool);
        }

        /// <summary>returns true if any player has used primary trigger since the last call to (player_action_test_reset).</summary>
        public static bool player_action_test_primary_trigger()
        {
            return default(bool);
        }

        /// <summary>resets the player action test state so that all tests will return false.</summary>
        public static void player_action_test_reset()
        {
        }

        /// <summary>returns true if any player has used vision trigger since the last call to (player_action_test_reset).</summary>
        public static bool player_action_test_vision_trigger()
        {
            return default(bool);
        }

        /// <summary>enables/disables camera control globally</summary>
        public static void player_camera_control(bool boolean)
        {
        }

        /// <summary>toggle player input. the look stick works, but nothing else.</summary>
        public static void player_disable_movement(bool boolean)
        {
        }

        /// <summary><yaw> <pitch> <roll></summary>
        public static void player_effect_set_max_rotation(float real, float real1, float real12)
        {
        }

        /// <summary><left> <right></summary>
        public static void player_effect_set_max_vibration(float real, float real1)
        {
        }

        /// <summary><max_intensity> <attack time></summary>
        public static void player_effect_start(float real, float real1)
        {
        }

        /// <summary><max_intensity> <attack time></summary>
        public static void player_effect_start()
        {
        }

        /// <summary><decay></summary>
        public static void player_effect_stop(float real)
        {
        }

        /// <summary><decay></summary>
        public static void player_effect_stop()
        {
        }

        /// <summary>toggle player input. the player can still free-look, but nothing else.</summary>
        public static void player_enable_input(bool boolean)
        {
        }

        /// <summary>returns true if any player has a flashlight on</summary>
        public static bool player_flashlight_on()
        {
            return default(bool);
        }

        /// <summary>guess</summary>
        public static void player_training_activate_flashlight()
        {
        }

        /// <summary>guess</summary>
        public static void player_training_activate_stealth()
        {
        }

        /// <summary>true if the first player is looking down</summary>
        public static bool player0_looking_down()
        {
            return default(bool);
        }

        /// <summary>true if the first player is looking up</summary>
        public static bool player0_looking_up()
        {
            return default(bool);
        }

        /// <summary>returns a list of the players</summary>
        public static ObjectList players()
        {
            return default(ObjectList);
        }

        /// <summary>predict a geometry block.</summary>
        public static void predict_model_section(Model render_model, int value)
        {
        }

        /// <summary>predict a geometry block.</summary>
        public static void predict_structure_section(Bsp structure_bsp, int value, bool boolean)
        {
        }

        /// <summary>prints a string to the console.</summary>
        public static void print(string value)
        {
        }

        /// <summary>removes the special place that activates everything it sees.</summary>
        public static void pvs_clear()
        {
        }

        /// <summary>sets the specified object as the special place that activates everything it sees.</summary>
        public static void pvs_set_object(Entity entity)
        {
        }

        /// <summary>returns a random value in the range [lower bound, upper bound)</summary>
        public static float random_range(float value, float value1)
        {
            return default(float);
        }

        /// <summary>returns a random value in the range [lower bound, upper bound)</summary>
        public static int random_range(int value, int value1)
        {
            return default(int);
        }

        /// <summary>enable</summary>
        public static void rasterizer_bloom_override(bool boolean)
        {
        }

        /// <summary>brightness</summary>
        public static void rasterizer_bloom_override_brightness(float real)
        {
        }

        /// <summary>threshold</summary>
        public static void rasterizer_bloom_override_threshold(float real)
        {
        }

        /// <summary>returns a random value in the range [lower bound, upper bound)</summary>
        public static float real_random_range(float real, float real1)
        {
            return default(float);
        }

        /// <summary>enable/disable the specified unit to receive cinematic shadows where the shadow is focused about a radius around a marker name</summary>
        public static void render_lights_enable_cinematic_shadow(bool boolean, Entity entity, string /*id*/ string_id, float real)
        {
        }

        /// <summary>starts a custom looping animation playing on a piece of scenery</summary>
        public static void scenery_animation_start_loop(Scenery scenery, Animation animation, string /*id*/ emotion)
        {
        }

        /// <summary>starts a custom animation playing on a piece of scenery relative to a parent object</summary>
        public static void scenery_animation_start_relative(Scenery scenery, string /*id*/ emotion, Entity entity)
        {
        }

        /// <summary>starts a custom animation playing on a piece of scenery relative to a parent object</summary>
        public static void scenery_animation_start_relative(Scenery scenery, Animation animation, string /*id*/ emotion, Entity entity)
        {
        }

        /// <summary>returns the number of ticks remaining in a custom animation (or zero, if the animation is over).</summary>
        public static short scenery_get_animation_time(Scenery scenery)
        {
            return default(short);
        }

        /// <summary>this is your brain on drugs</summary>
        public static void set_global_sound_environment(float real, float real1, float real12, float real123, int value, float real1234)
        {
        }

        /// <summary>pauses execution of this script (or, optionally, another script) for the specified number of ticks.</summary>
        public static void sleep(int ticks)
        {
        }

        /// <summary>pauses execution of this script (or, optionally, another script) for the specified number of ticks.</summary>
        public static void sleep(short ticks)
        {
        }

        /// <summary>
        /// pauses execution of this script (or, optionally, another script) for the specified number of ticks.
        /// This overload shouldn't exist, only to support lack of cast detection in code gen
        /// </summary>
        public static void sleep(float ticks)
        {
        }

        /// <summary>pauses execution of this script (or, optionally, another script) forever.</summary>
        public static void sleep_forever()
        {
        }

        /// <summary>pauses execution of this script (or, optionally, another script) forever.</summary>
        public static void sleep_forever(ScriptReference script = null)
        {
        }

        /// <summary>pauses execution of this script until the specified condition is true, checking once per second unless a different number of ticks is specified.</summary>
        public static void sleep_until(Func<bool> condition, int ticks = TicksPerSecond, int timeout = -1)
        {
        }

        /// <summary>changes the gain on the specified sound class(es) to the specified gain over the specified number of ticks.</summary>
        public static void sound_class_set_gain(string value, float gain, int ticks)
        {
        }

        /// <summary>returns the time remaining for the specified impulse sound. DO NOT CALL IN CUTSCENES.</summary>
        public static int sound_impulse_language_time(ReferenceGet soundRef)
        {
            return default;
        }

        /// <summary>your mom part 2.</summary>
        public static void sound_impulse_predict(ReferenceGet soundRef)
        {
        }

        /// <summary>plays an impulse sound from the specified source object (or "none"), with the specified scale.</summary>
        public static void sound_impulse_start(object sound, Entity entity, float floatValue)
        {
        }

        /// <summary>plays an impulse sound from the specified source object (or "none"), with the specified scale and effect.</summary>
        public static void sound_impulse_start_effect(ReferenceGet sound, Entity entity, float floatValue, string /*id*/ effect)
        {
        }

        /// <summary>stops the specified impulse sound.</summary>
        public static void sound_impulse_stop(ReferenceGet sound)
        {
        }

        /// <summary>plays an impulse sound from the specified source object (or "none"), with the specified scale.</summary>
        public static void sound_impulse_trigger(Entity sound, float floatValue, int intValue)
        {
        }

        /// <summary>plays an impulse sound from the specified source object (or "none"), with the specified scale.</summary>
        public static void sound_impulse_trigger(Entity sound, Entity source, float floatValue, int intValue)
        {
        }

        /// <summary>enables or disables the alternate loop/alternate end for a looping sound.</summary>
        public static void sound_looping_set_alternate(LoopingSound looping_sound, bool boolean)
        {
        }

        /// <summary>plays a looping sound from the specified source object (or "none"), with the specified scale.</summary>
        public static void sound_looping_start(LoopingSound looping_sound, Entity entity, float real)
        {
        }

        /// <summary>plays a looping sound from the specified source object (or "none"), with the specified scale.</summary>
        public static void sound_looping_start(LoopingSound loopingSound, float floatValue)
        {
        }

        /// <summary>stops the specified looping sound.</summary>
        public static void sound_looping_stop(LoopingSound looping_sound)
        {
        }

        /// <summary>call this when transitioning between two cinematics so ambience won't fade in between the skips</summary>
        public static void sound_suppress_ambience_update_on_revert()
        {
        }

        /// <summary>returns the current structure bsp index</summary>
        public static short structure_bsp_index()
        {
            return default(short);
        }

        /// <summary>takes off your condom and changes to a different structure bsp</summary>
        public static void switch_bsp(short value)
        {
        }

        /// <summary>leaves your condom on and changes to a different structure bsp by name</summary>
        public static void switch_bsp_by_name(Bsp structure_bsp)
        {
        }

        /// <summary>don't make me kick your ass</summary>
        public static void texture_cache_flush()
        {
        }

        /// <summary>turns off the render texture camera</summary>
        public static void texture_camera_off()
        {
        }

        /// <summary>sets the render texture camera to a given object marker</summary>
        public static void texture_camera_set_object_marker(Entity entity, string /*id*/ string_id, float real)
        {
        }

        /// <summary>resets the time code timer</summary>
        public static void time_code_reset()
        {
        }

        /// <summary>shows the time code timer</summary>
        public static void time_code_show(bool boolean)
        {
        }

        /// <summary>starts/stops the time code timer</summary>
        public static void time_code_start(bool boolean)
        {
        }

        /// <summary>converts an object to a unit.</summary>
        public static Unit unit(Entity entity)
        {
            return default(Unit);
        }

        /// <summary>adds/resets the unit's health, shield, and inventory (weapons and grenades) to the named profile. resets if third parameter is true, adds if false. weapons will be marked as garbage if fourth parameter is true (for respawning equipment).</summary>
        public static void unit_add_equipment(Unit unit, Equipment starting_profile, bool reset, bool isGarbage)
        {
        }

        /// <summary>prevents any of the given units from dropping weapons or grenades when they die</summary>
        public static void unit_doesnt_drop_items(ObjectList entities)
        {
        }

        /// <summary>makes a unit exit its vehicle</summary>
        public static void unit_exit_vehicle(Unit unit, short value)
        {
        }

        /// <summary>returns the number of ticks remaining in a unit's custom animation (or zero, if the animation is over).</summary>
        public static short unit_get_custom_animation_time(Unit unit)
        {
            return default(short);
        }

        /// <summary>returns the health [0,1] of the unit, returns -1 if the unit does not exist</summary>
        public static float unit_get_health(Unit unit)
        {
            return default(float);
        }

        /// <summary>returns the shield [0,1] of the unit, returns -1 if the unit does not exist</summary>
        public static float unit_get_shield(Unit unit)
        {
            return default(float);
        }

        /// <summary>returns the shield [0,1] of the unit, returns -1 if the unit does not exist</summary>
        public static float unit_get_shield()
        {
            return default(float);
        }

        /// <summary>returns TRUE if the <unit> has <object> as a weapon, FALSE otherwise</summary>
        public static bool unit_has_weapon(Unit unit, Weapon weapon)
        {
            return default(bool);
        }

        /// <summary>prevents any of the given units from being knocked around or playing ping animations</summary>
        public static void unit_impervious(Entity unit, bool boolean)
        {
        }

        /// <summary>prevents any of the given units from being knocked around or playing ping animations</summary>
        public static void unit_impervious(ObjectList object_list, bool boolean)
        {
        }

        /// <summary>returns true if the given unit is seated on a parent unit</summary>
        public static bool unit_in_vehicle()
        {
            return default(bool);
        }

        /// <summary>returns true if the given unit is seated on a parent unit</summary>
        public static bool unit_in_vehicle(Unit unit)
        {
            return default(bool);
        }

        /// <summary>returns whether or not the given unit is current emitting an ai</summary>
        public static bool unit_is_emitting(Unit unit)
        {
            return default(bool);
        }

        /// <summary>kills a given unit, no saving throw</summary>
        public static void unit_kill()
        {
        }

        /// <summary>kills a given unit, no saving throw</summary>
        public static void unit_kill(Unit unit)
        {
        }

        /// <summary>kills a given unit silently (doesn't make them play their normal death animation or sound)</summary>
        public static void unit_kill_silent(Unit unit)
        {
        }

        /// <summary>used for the tartarus boss fight</summary>
        public static void unit_only_takes_damage_from_players_team(Unit unit, bool boolean)
        {
        }

        /// <summary>enable or disable active camo for the given unit over the specified number of seconds</summary>
        public static void unit_set_active_camo(Unit unit, bool boolean, float real)
        {
        }

        /// <summary>sets a unit's current body and shield vitality</summary>
        public static void unit_set_current_vitality(Unit unit, float real, float real1)
        {
        }

        /// <summary>sets a unit's current body and shield vitality</summary>
        public static void unit_set_current_vitality(float body, float shield)
        {
        }

        /// <summary>sets a unit's facial expression by name with weight and transition time</summary>
        public static void unit_set_emotional_state(Unit unit, string /*id*/ string_id, float real, short value)
        {
        }

        /// <summary>sets a unit's facial expression by name with weight and transition time</summary>
        public static void unit_set_emotional_state(string /*id*/ emotion, float floatValue, short valueValue)
        {
        }

        /// <summary>can be used to prevent the player from entering a vehicle</summary>
        public static void unit_set_enterable_by_player(Unit unit, bool boolean)
        {
        }

        /// <summary>sets a unit's maximum body and shield vitality</summary>
        public static void unit_set_maximum_vitality(Unit unit, float real, float real1)
        {
        }

        /// <summary>sets a unit's maximum body and shield vitality</summary>
        public static void unit_set_maximum_vitality(float body, float shield)
        {
        }

        /// <summary>stops the custom animation running on the given unit.</summary>
        public static void unit_stop_custom_animation(Unit unit)
        {
        }

        /// <summary>sets a group of units' current body and shield vitality</summary>
        public static void units_set_current_vitality(ObjectList units, float body, float shield)
        {
        }

        /// <summary>sets a group of units' maximum body and shield vitality</summary>
        public static void units_set_maximum_vitality(ObjectList units, float body, float shield)
        {
        }

        /// <summary>returns the driver of a vehicle</summary>
        public static Entity vehicle_driver(Unit unit)
        {
            return default(Entity);
        }

        /// <summary>makes a list of units (named or by encounter) magically get into a vehicle, in the substring-specified seats (e.g. CD-passenger... empty string matches all seats)</summary>
        public static void vehicle_load_magic(Entity vehicle, /*VehicleSeat*/ string vehicleSeat, ObjectList units)
        {
        }

        /// <summary>makes a list of units (named or by encounter) magically get into a vehicle, in the substring-specified seats (e.g. CD-passenger... empty string matches all seats)</summary>
        public static void vehicle_load_magic(Entity vehicle, /*VehicleSeat*/ string vehicleSeat, Entity unit)
        {
        }

        /// <summary>tests whether the named seat has a specified unit in it (use "" to test all seats for this unit)</summary>
        public static bool vehicle_test_seat(Vehicle vehicle, string  seat, Unit unit)
        {
            return default(bool);
        }

        /// <summary>tests whether the named seat has an object in the object list (use "" to test all seats for any unit in the list)</summary>
        public static bool vehicle_test_seat_list(Vehicle vehicle, string /*id*/ seat, ObjectList subjects)
        {
            return default(bool);
        }

        /// <summary>tests whether the named seat has an object in the object list (use "" to test all seats for any unit in the list)</summary>
        public static bool vehicle_test_seat_list(Vehicle vehicle, string /*id*/ seat, Entity subject)
        {
            return default(bool);
        }

        /// <summary>makes units get out of an object from the substring-specified seats (e.g. CD-passenger... empty string matches all seats)</summary>
        public static void vehicle_unload(Entity entity, /*VehicleSeat*/ string unit_seat_mapping)
        {
        }

        /// <summary>returns list of objects in volume or (max 128).</summary>
        public static ObjectList volume_return_objects(Trigger trigger_volume)
        {
            return default(ObjectList);
        }

        /// <summary>returns list of objects in volume or (max 128).</summary>
        public static ObjectList volume_return_objects_by_type(Trigger trigger_volume, int value)
        {
            return default(ObjectList);
        }

        /// <summary>moves all players outside a specified trigger volume to a specified flag.</summary>
        public static void volume_teleport_players_not_inside(Trigger trigger_volume, LocationFlag cutscene_flag)
        {
        }

        /// <summary>returns true if the specified object is within the specified volume.</summary>
        public static bool volume_test_object(Trigger trigger)
        {
            return default(bool);
        }

        /// <summary>returns true if the specified object is within the specified volume.</summary>
        public static bool volume_test_object(Trigger trigger_volume, Entity entity)
        {
            return default(bool);
        }

        /// <summary>returns true if any of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        public static bool volume_test_objects(Trigger trigger, Entity entity)
        {
            return default(bool);
        }

        /// <summary>returns true if any of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        public static bool volume_test_objects(Trigger trigger_volume, ObjectList object_list)
        {
            return default(bool);
        }

        /// <summary>returns true if any (rb: all?) of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        public static bool volume_test_objects_all(Trigger trigger, ObjectList object_list)
        {
            return default(bool);
        }

        /// <summary>returns true if any (rb: all?) of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        public static bool volume_test_objects_all(Trigger trigger, Entity entity)
        {
            return default(bool);
        }

        /// <summary>wakes a sleeping script in the next update.</summary>
        public static void wake(ScriptReference script_name)
        {
        }

        /// <summary>turns the trigger for a weapon  on/off</summary>
        public static void weapon_enable_warthog_chaingun_light(bool boolean)
        {
        }

        /// <summary>turns the trigger for a weapon  on/off</summary>
        public static void weapon_hold_trigger(WeaponReference weapon, int triggerIndex, bool boolean)
        {
        }

        /// <summary><time> <intensity></summary>
        public static void weather_change_intensity(float real, float real1)
        {
        }

        /// <summary><time> <intensity></summary>
        public static void weather_change_intensity(float floatValue)
        {
        }

        /// <summary><time></summary>
        public static void weather_start(float real)
        {
        }

        /// <summary><time></summary>
        public static void weather_start()
        {
        }

        /// <summary><time></summary>
        public static void weather_stop(float real)
        {
        }

        /// <summary><time></summary>
        public static void weather_stop()
        {
        }

        public static class NavigationPoints
        {
            public static NavigationPoint @default;
            public static NavigationPoint default_red;
        }
    }
}