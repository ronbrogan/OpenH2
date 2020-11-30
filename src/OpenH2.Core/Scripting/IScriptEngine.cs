using OpenH2.Core.GameObjects;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Threading.Tasks;

namespace OpenH2.Core.Scripting
{
    public interface IScriptEngine
    {

        /// <summary>activates a nav point type <string> attached to a team anchored to a flag with a vertical offset <real>. If the player is not local to the machine, this will fail</summary>
        void activate_team_nav_point_flag(INavigationPoint navpoint, ITeam team, ILocationFlag cutscene_flag, float real);

        /// <summary>activates a nav point type <string> attached to a team anchored to an object with a vertical offset <real>. If the player is not local to the machine, this will fail</summary>
        void activate_team_nav_point_object(INavigationPoint navpoint, ITeam team, IGameObject entity, float real);

        /// <summary>converts an ai reference to an object list.</summary>
        GameObjectList ai_actors(IAiActorDefinition ai);

        /// <summary>creates an allegiance between two teams.</summary>
        void ai_allegiance(ITeam team, ITeam team1);

        /// <summary>attaches the specified list of units to the specified encounter.</summary>
        void ai_attach_units(GameObjectList units, IAiActorDefinition ai);

        /// <summary>attaches the specified list of units to the specified encounter.</summary>
        void ai_attach_units(IUnit unit, IAiActorDefinition ai);

        /// <summary>forces a group of actors to start or stop berserking</summary>
        void ai_berserk(IAiActorDefinition ai, bool boolean);

        /// <summary>makes a group of actors braindead, or restores them to life (in their initial state)</summary>
        void ai_braindead(IAiActorDefinition ai, bool boolean);

        /// <summary>AI cannot die from damage (as opposed to by scripting)</summary>
        void ai_cannot_die(IAiActorDefinition ai, bool boolean);

        /// <summary>Returns the highest integer combat status in the given squad-group/squad/actor</summary>
        short ai_combat_status(IAiActorDefinition ai);

        /// <summary>turn combat dialogue on/off</summary>
        void ai_dialogue_enable(bool boolean);

        /// <summary>enables or disables automatic garbage collection for actors in the specified encounter and/or squad.</summary>
        void ai_disposable(IAiActorDefinition ai, bool boolean);

        // TODO: Instead of overloads for single objects (rather than a list) there should be
        // a conversion of the single object to list

        /// <summary>if TRUE, forces all actors to completely disregard the specified units, otherwise lets them acknowledge the units again</summary>
        void ai_disregard(IGameObject unit, bool boolean);

        /// <summary>if TRUE, forces all actors to completely disregard the specified units, otherwise lets them acknowledge the units again</summary>
        void ai_disregard(IAiActorDefinition actor, bool boolean);

        /// <summary>if TRUE, forces all actors to completely disregard the specified units, otherwise lets them acknowledge the units again</summary>
        void ai_disregard(GameObjectList object_list, bool boolean);

        /// <summary>Instructs the ai in the given squad to get in all their vehicles</summary>
        void ai_enter_squad_vehicles(IAiActorDefinition ai);

        /// <summary>erases the specified encounter and/or squad.</summary>
        void ai_erase(IAiActorDefinition ai);

        /// <summary>erases all AI.</summary>
        void ai_erase_all();

        /// <summary>return the number of actors that are fighting in a squad or squad_group</summary>
        short ai_fighting_count(IAiActorDefinition ai);

        /// <summary>returns the unit/object corresponding to the given actor</summary>
        IGameObject ai_get_object(IAiActorDefinition ai);

        /// <summary>returns the unit/object corresponding to the given actor</summary>
        IUnit ai_get_unit(IAiActorDefinition ai);

        /// <summary>instantly kills the specified encounter and/or squad.</summary>
        void ai_kill(IAiActorDefinition ai);

        /// <summary>instantly and silently (no animation or sound played) kills the specified encounter and/or squad.</summary>
        void ai_kill_silent(IAiActorDefinition ai);

        /// <summary>return the number of living actors in the specified encounter and/or squad.</summary>
        short ai_living_count(IAiActorDefinition ai);

        /// <summary>Make one squad magically aware of another.</summary>
        void ai_magically_see(IAiActorDefinition ai, IAiActorDefinition ai1);

        /// <summary>Make a squad magically aware of a particular object.</summary>
        void ai_magically_see_object(IAiActorDefinition ai, IGameObject value);

        /// <summary>makes all or part of an encounter move to another encounter.</summary>
        void ai_migrate(IAiActorDefinition ai);

        /// <summary>makes all or part of an encounter move to another encounter.</summary>
        void ai_migrate(IAiActorDefinition ai, IAiActorDefinition ai1);

        /// <summary>return the number of non-swarm actors in the specified encounter and/or squad.</summary>
        short ai_nonswarm_count(IAiActorDefinition ai);

        /// <summary>Don't use this for anything other than bug 3926.  AI magically cancels vehicle oversteer.</summary>
        void ai_overcomes_oversteer(IAiActorDefinition ai, bool boolean);

        /// <summary>places the specified squad on the map.</summary>
        void ai_place(IAiActorDefinition ai);

        /// <summary>places the specified squad on the map.</summary>
        void ai_place(IAiActorDefinition ai, int value);

        /// <summary>places the specified squad (1st arg) on the map in the vehicles belonging to the specified vehicle squad (2nd arg).</summary>
        void ai_place_in_vehicle(IAiActorDefinition ai, IAiActorDefinition ai1);

        /// <summary>Play the given mission dialogue line on the given ai</summary>
        short ai_play_line(IAiActorDefinition ai, string string_id);

        /// <summary>Play the given mission dialogue line on the given ai, directing the ai's gaze at the nearest visible player</summary>
        short ai_play_line_at_player(IAiActorDefinition ai, string string_id);

        /// <summary>Play the given mission dialogue line on the given ai, directing the ai's gaze at the nearest visible player</summary>
        short ai_play_line_at_player(string emotion);

        /// <summary>Play the given mission dialogue line on the given object (uses first available variant)</summary>
        short ai_play_line_on_object(IGameObject entity, string string_id);

        /// <summary>if TRUE, *ALL* enemies will prefer to attack the specified units. if FALSE, removes the preference.</summary>
        void ai_prefer_target(GameObjectList units, bool boolean);

        /// <summary>refreshes the health and grenade count of a group of actors, so they are as good as new</summary>
        void ai_renew(IAiActorDefinition ai);

        /// <summary>Start the named scene, with the named command script on the named squad</summary>
        bool ai_scene(string emotion, AIScript aiScript);

        /// <summary>Start the named scene, with the named command script on the named squad</summary>
        bool ai_scene(string string_id, AIScript ai_command_script, IAiActorDefinition ai);

        /// <summary>Start the named scene, with the named command script on the named squad</summary>
        bool ai_scene(string string_id, AIScript ai_command_script, IAiActorDefinition ai, IAiActorDefinition ai1);

        /// <summary>Turn on active camoflage on actor/squad/squad-group</summary>
        void ai_set_active_camo(IAiActorDefinition ai, bool boolean);

        /// <summary>enables or disables sight for actors in the specified encounter.</summary>
        void ai_set_blind(IAiActorDefinition ai, bool boolean);

        /// <summary>enables or disables hearing for actors in the specified encounter.</summary>
        void ai_set_deaf(IAiActorDefinition ai, bool boolean);

        /// <summary>Takes the squad or squad group (arg1) and gives it the order (arg3) in zone (arg2). Use the zone_name/order_name format</summary>
        void ai_set_orders(IAiActorDefinition ai, IAiOrders ai_orders);

        /// <summary>returns the number of actors spawned in the given squad or squad group</summary>
        short ai_spawn_count(IAiActorDefinition ai);

        /// <summary>return the current strength (average body vitality from 0-1) of the specified encounter and/or squad.</summary>
        float ai_strength(IAiActorDefinition ai);

        /// <summary>Turn on/off combat suppression on actor/squad/squad-group</summary>
        void ai_suppress_combat(IAiActorDefinition ai, bool boolean);

        /// <summary>return the number of swarm actors in the specified encounter and/or squad.</summary>
        short ai_swarm_count(IAiActorDefinition ai);

        /// <summary>teleports a group of actors to the starting locations of their current squad(s) if they are currently outside the world.</summary>
        void ai_teleport_to_starting_location_if_outside_bsp(IAiActorDefinition ai);

        /// <summary>Tests the named trigger on the named squad</summary>
        bool ai_trigger_test(string value, IAiActorDefinition ai);

        /// <summary>tells a group of actors to get into a vehicle... does not interrupt any actors who are already going to vehicles</summary>
        void ai_vehicle_enter(IAiActorDefinition ai);

        /// <summary>tells a group of actors to get into a vehicle... does not interrupt any actors who are already going to vehicles</summary>
        void ai_vehicle_enter(IAiActorDefinition ai, string unit);

        /// <summary>tells a group of actors to get into a vehicle... does not interrupt any actors who are already going to vehicles</summary>
        void ai_vehicle_enter(IAiActorDefinition ai, IUnit unit, string unit_seat_mapping = null);

        /// <summary>the given group of actors is snapped into a vehicle, in the substring-specified seats (e.g. passenger for pelican)... does not interrupt any actors who are already going to vehicles</summary>
        void ai_vehicle_enter_immediate(IAiActorDefinition ai, IUnit unit, string seat = null);

        /// <summary>tells a group of actors to get out of any vehicles that they are in</summary>
        void ai_vehicle_exit(IAiActorDefinition ai);


        /// <summary>Returns the vehicle that the given actor is in.</summary>
        IVehicle ai_vehicle_get(IAiActorDefinition ai);

        /// <summary>Returns the vehicle that was spawned at the given starting location.</summary>
        IVehicle ai_vehicle_get_from_starting_location(IAiActorDefinition ai);

        /// <summary>Reserves the given vehicle (so that AI may not enter it</summary>
        void ai_vehicle_reserve(IVehicle vehicle, bool boolean);

        /// <summary>Reserves the given seat on the given vehicle (so that AI may not enter it</summary>
        void ai_vehicle_reserve_seat(string emotion, bool boolean);

        /// <summary>Reserves the given seat on the given vehicle (so that AI may not enter it</summary>
        void ai_vehicle_reserve_seat(IVehicle vehicle, string string_id, bool boolean);

        /// <summary>Returns true if the ai's units are ALL vitality pinned (see object_vitality_pinned)</summary>
        bool ai_vitality_pinned(IAiActorDefinition ai);

        /// <summary>evaluates the sequence of expressions in random order and returns the last value evaluated.</summary>
        void begin_random(params Func<Task>[] expressions);

        /// <summary>returns true if the movie is done playing</summary>
        bool bink_done();

        /// <summary>given a dead biped, turns on ragdoll</summary>
        void biped_ragdoll(IUnit unit);

        /// <summary>call this to force texture and geometry cache to block until satiated</summary>
        Task cache_block_for_one_frame();

        /// <summary>toggles script control of the camera.</summary>
        void camera_control(bool boolean);

        /// <summary>predict resources at a frame in camera animation.</summary>
        void camera_predict_resources_at_frame(AnimationGraphTag animation, string emotion, IUnit unit, ILocationFlag locationFlag, int intValue);

        /// <summary>predict resources given a camera point</summary>
        void camera_predict_resources_at_point(ICameraPathTarget cutscene_camera_point);

        /// <summary>moves the camera to the specified camera point over the specified number of ticks.</summary>
        void camera_set(ICameraPathTarget cutscene_camera_point, short value);

        /// <summary>begins a prerecorded camera animation synchronized to unit relative to cutscene flag.</summary>
        void camera_set_animation_relative(AnimationGraphTag animation, string id, IUnit unit, ILocationFlag locationFlag);

        /// <summary>sets the field of view</summary>
        void camera_set_field_of_view(float degrees, short ticks);

        /// <summary>returns the number of ticks remaining in the current camera interpolation.</summary>
        short camera_time();

        /// <summary>gives a specific player active camouflage</summary>
        void cheat_active_camouflage_by_player(short value, bool boolean);

        /// <summary>clone the first player's most reasonable weapon and attach it to the specified object's marker</summary>
        void cinematic_clone_players_weapon(IGameObject entity, string string_id, string string_id1);

        /// <summary>enable/disable ambience details in cinematics</summary>
        void cinematic_enable_ambience_details(bool boolean);

        /// <summary>sets the color (red, green, blue) of the cinematic ambient light.</summary>
        void cinematic_lighting_set_ambient_light(float real, float real1, float real12);

        /// <summary>sets the pitch, yaw, and color (red, green, blue) of the cinematic shadowing diffuse and specular directional light.</summary>
        void cinematic_lighting_set_primary_light(float real, float real1, float real12, float real123, float real1234);

        /// <summary>sets the pitch, yaw, and color (red, green, blue) of the cinematic non-shadowing diffuse directional light.</summary>
        void cinematic_lighting_set_secondary_light(float real, float real1, float real12, float real123, float real1234);

        /// <summary>turn off lightmap shadow in cinematics</summary>
        void cinematic_lightmap_shadow_disable();

        /// <summary>turn on lightmap shadow in cinematics</summary>
        void cinematic_lightmap_shadow_enable();

        /// <summary>flag this cutscene as an outro cutscene</summary>
        void cinematic_outro_start();

        /// <summary>transition-time</summary>
        void cinematic_screen_effect_set_crossfade(float real);

        /// <summary>sets dof: <seperation dist>, <near blur lower bound> <upper bound> <time> <far blur lower bound> <upper bound> <time></summary>
        void cinematic_screen_effect_set_depth_of_field(float real, float real1, float real12, float real123, float real1234, float real12345, float real123456);

        /// <summary>starts screen effect pass TRUE to clear</summary>
        void cinematic_screen_effect_start(bool boolean);

        /// <summary>returns control of the screen effects to the rest of the game</summary>
        void cinematic_screen_effect_stop();

        /// <summary></summary>
        void cinematic_set_far_clip_distance(float real);

        /// <summary></summary>
        void cinematic_set_near_clip_distance(float real);

        /// <summary>activates the chapter title</summary>
        void cinematic_set_title(ICinematicTitle cutscene_title);

        /// <summary>sets or removes the letterbox bars</summary>
        void cinematic_show_letterbox(bool boolean);

        /// <summary>sets or removes the letterbox bars</summary>
        void cinematic_show_letterbox_immediate(bool boolean);

        /// <summary></summary>
        void cinematic_skip_start_internal();

        /// <summary></summary>
        void cinematic_skip_stop_internal();

        /// <summary>initializes game to start a cinematic (interruptive) cutscene</summary>
        void cinematic_start();

        /// <summary>initializes the game to end a cinematic (interruptive) cutscene</summary>
        void cinematic_stop();

        /// <summary>displays the named subtitle for <real> seconds</summary>
        void cinematic_subtitle(string string_id, float real);

        /// <summary>Starts the pre-rendered cinematic movie. Introduced by MCC </summary>
        void cinematic_start_movie(string name);

        /// <summary>returns TRUE if player0's look pitch is inverted</summary>
        bool controller_get_look_invert();

        /// <summary>invert player0's look</summary>
        void controller_set_look_invert();

        /// <summary>invert player0's look</summary>
        void controller_set_look_invert(bool boolean);

        /// <summary>Command script ends prematurely when actor's combat status raises to 'alert' or higher</summary>
        void cs_abort_on_alert(bool boolean);

        /// <summary>Command script ends prematurely when actor's combat status rises to given level</summary>
        void cs_abort_on_combat_status(short value);

        /// <summary>Command script ends prematurely when actor is damaged</summary>
        void cs_abort_on_damage(bool boolean);

        /// <summary>Actor aims at the point for the remainder of the cs, or until overridden (overrides look)</summary>
        void cs_aim(bool boolean, ISpatialPoint point);

        /// <summary>Actor aims at the object for the duration of the cs, or until overridden (overrides look)</summary>
        void cs_aim_object(bool boolean);

        /// <summary>Actor aims at the object for the duration of the cs, or until overridden (overrides look)</summary>
        void cs_aim_object(bool boolean, IGameObject entity);

        /// <summary>Actor aims at nearest player for the duration of the cs, or until overridden (overrides look)</summary>
        void cs_aim_player(bool boolean);

        /// <summary></summary>
        void cs_approach(IGameObject entity, float real, float real1, float real12);

        /// <summary></summary>
        void cs_approach(float floatValue, float floatValue1, float floatValue2);

        /// <summary></summary>
        void cs_approach_player(float real, float real1, float real12);

        /// <summary>Actor stops approaching</summary>
        void cs_approach_stop();

        /// <summary>Returns true if the command script is in the ai's cs queue</summary>
        bool cs_command_script_queued(IAiActorDefinition ai, AIScript ai_command_script);

        /// <summary>Returns true if the ai is running the command script in question</summary>
        bool cs_command_script_running(IAiActorDefinition ai, AIScript ai_command_script);

        /// <summary>Actor crouches for the remainder of the command script, or until overridden</summary>
        void cs_crouch(bool boolean);

        /// <summary>starts a custom animation playing on the unit (interpolates into animation if last parameter is TRUE)</summary>
        void cs_custom_animation(AnimationGraphTag animation, string emotion, float floatValue, bool interpolate);

        /// <summary>Deploy a turret at the given script point</summary>
        void cs_deploy_turret(ISpatialPoint point);

        /// <summary>Actor combat dialogue enabled/disabled.</summary>
        void cs_enable_dialogue(bool boolean);

        /// <summary>Actor autonomous looking enabled/disabled.</summary>
        void cs_enable_looking(bool boolean);

        /// <summary>Actor autonomous moving enabled/disabled.</summary>
        void cs_enable_moving(bool boolean);

        /// <summary>Actor blocks until pathfinding calls succeed</summary>
        void cs_enable_pathfinding_failsafe(bool boolean);

        /// <summary>Actor autonomous target selection enabled/disabled.</summary>
        void cs_enable_targeting(bool boolean);

        /// <summary>Actor faces exactly the point for the remainder of the cs, or until overridden (overrides aim, look)</summary>
        void cs_face(bool boolean, ISpatialPoint point = null);

        /// <summary>Actor faces exactly the given object for the duration of the cs, or until overridden (overrides aim, look)</summary>
        void cs_face_object(bool boolean, IGameObject entity);

        /// <summary>Actor faces exactly the nearest player for the duration of the cs, or until overridden (overrides aim, look)</summary>
        void cs_face_player(bool boolean);

        /// <summary>Flies the actor through the given point</summary>
        void cs_fly_by();

        /// <summary>Flies the actor through the given point</summary>
        void cs_fly_by(ISpatialPoint point, float tolerance = 0);

        /// <summary>Flies the actor to the given point (within the given tolerance)</summary>
        void cs_fly_to(ISpatialPoint point, float tolerance = 0);

        /// <summary>Flies the actor to the given point and orients him in the appropriate direction (within the given tolerance)</summary>
        void cs_fly_to_and_face(ISpatialPoint point, ISpatialPoint face, float tolerance = 0);

        /// <summary>Force the actor's combat status (0= no override, 1= asleep, 2=idle, 3= alert, 4= active)</summary>
        void cs_force_combat_status(short value);

        /// <summary>Actor moves toward the point, and considers it hit when it breaks the indicated plane</summary>
        void cs_go_by(ISpatialPoint point, ISpatialPoint planeP, float planeD = 0);

        /// <summary>Moves the actor to a specified point</summary>
        void cs_go_to(ISpatialPoint point, float tolerance = 1);

        /// <summary>Moves the actor to a specified point and has him face the second point</summary>
        void cs_go_to_and_face(ISpatialPoint point, ISpatialPoint faceTowards);

        /// <summary>Given a point set, AI goes toward the nearest point</summary>
        void cs_go_to_nearest(ISpatialPoint destination);

        /// <summary>Actor gets in the appropriate vehicle</summary>
        void cs_go_to_vehicle();

        /// <summary>Actor gets in the appropriate vehicle</summary>
        void cs_go_to_vehicle(IVehicle vehicle);

        /// <summary>Actor throws a grenade, either by tossing (arg2=0), lobbing (1) or bouncing (2)</summary>
        void cs_grenade(ISpatialPoint point, int action);

        /// <summary>Actor does not avoid obstacles when true</summary>
        void cs_ignore_obstacles(bool boolean);

        /// <summary>Actor jumps in direction of angle at the given velocity (angle, velocity)</summary>
        void cs_jump(float real, float real1);

        /// <summary>Actor jumps with given horizontal and vertical velocity</summary>
        void cs_jump_to_point(float real, float real1);

        /// <summary>Actor looks at the point for the remainder of the cs, or until overridden</summary>
        void cs_look(bool boolean, ISpatialPoint point = null);

        /// <summary>Actor looks at the object for the duration of the cs, or until overridden</summary>
        void cs_look_object(bool boolean);

        /// <summary>Actor looks at the object for the duration of the cs, or until overridden</summary>
        void cs_look_object(bool boolean, IGameObject entity);

        /// <summary>Actor looks at nearest player for the duration of the cs, or until overridden</summary>
        void cs_look_player(bool boolean);

        /// <summary>Actor switches to given animation mode</summary>
        void cs_movement_mode(short value);

        /// <summary>Actor moves at given angle, for the given distance, optionally with the given facing (angle, distance, facing)</summary>
        void cs_move_in_direction(float real, float real1, float real12);

        /// <summary>Returns TRUE if the actor is currently following a path</summary>
        bool cs_moving();

        /// <summary>The actor does nothing for the given number of seconds</summary>
        void cs_pause(float real);

        /// <summary>Play the named line in the current scene</summary>
        void cs_play_line(string string_id);

        /// <summary>Add a command script onto the end of an actor's command script queue</summary>
        void cs_queue_command_script(IAiActorDefinition ai, AIScript ai_command_script);

        /// <summary>Causes the specified actor(s) to start executing a command script immediately (discarding any other command scripts in the queue)</summary>
        void cs_run_command_script(IAiActorDefinition ai, AIScript ai_command_script);

        /// <summary>Actor performs the indicated behavior</summary>
        void cs_set_behavior(IAIBehavior ai_behavior);

        /// <summary>Actor is allowed to shoot at its target or not</summary>
        void cs_shoot(bool boolean);

        /// <summary>Actor is allowed to shoot at its target or not</summary>
        void cs_shoot(bool boolean, IGameObject entity);

        /// <summary>Actor shoots at given point</summary>
        void cs_shoot_point(bool boolean, ISpatialPoint point);

        /// <summary>Push a command script to the top of the actor's command script queue</summary>
        void cs_stack_command_script(IAiActorDefinition ai, AIScript ai_command_script);

        /// <summary></summary>
        void cs_start_approach(IGameObject entity, float real, float real1, float real12);

        /// <summary></summary>
        void cs_start_approach_player(float real, float real1, float real12);

        /// <summary>Moves the actor to a specified point. DOES NOT BLOCK SCRIPT EXECUTION.</summary>
        void cs_start_to(ISpatialPoint destination);

        /// <summary>Stop running a custom animation</summary>
        void cs_stop_custom_animation();

        /// <summary>Combat dialogue is suppressed for the remainder of the command script</summary>
        void cs_suppress_dialogue_global(bool boolean);

        /// <summary>Switch control of the joint command script to the given member</summary>
        void cs_switch(string string_id);
        /// <summary>Actor teleports to point1 facing point2</summary>
        void cs_teleport(ISpatialPoint destination, ISpatialPoint facing);

        /// <summary>Set the sharpness of a vehicle turn (values 0 -> 1). Only applicable to nondirectional flying vehicles (e.g. dropships)</summary>
        void cs_turn_sharpness(bool boolean, float real);

        /// <summary>Enables or disables boost</summary>
        void cs_vehicle_boost(bool boolean);

        /// <summary>Set the speed at which the actor will drive a vehicle, expressed as a multiplier 0-1</summary>
        void cs_vehicle_speed(float real);

        /// <summary>starts a custom animation playing on a unit (interpolates into animation if last parameter is TRUE)</summary>
        void custom_animation(IUnit unit, AnimationGraphTag animation, string stringid, bool interpolate);

        /// <summary>starts a custom animation playing on a unit (interpolates into animation if last parameter is TRUE)</summary>
        void custom_animation(IUnit unit, string emotion, bool interpolate);

        /// <summary>starts a custom animation playing on a unit (interpolates into animation if last parameter is TRUE)</summary>
        void custom_animation_loop(string emotion, bool interpolate);

        /// <summary>starts a custom animation playing on a unit (interpolates into animation if last parameter is TRUE)</summary>
        void custom_animation_loop(IUnit unit, AnimationGraphTag animation1, string emotion, bool interpolate);

        /// <summary>starts a custom animation relative to some other object (interpolates into animation if last parameter is TRUE)</summary>
        void custom_animation_relative(IUnit entity, AnimationGraphTag animation, string emotion, bool boolean, IGameObject other);

        /// <summary>starts a custom animation relative to some other object (interpolates into animation if last parameter is TRUE)</summary>
        void custom_animation_relative(IUnit unit, string emotion, bool interpolate, IGameObject entity);

        /// <summary>starts a custom animation relative to some other object (interpolates into animation if last parameter is TRUE)</summary>
        void custom_animation_relative_loop(IUnit unit, AnimationGraphTag animation2, string emotion, bool boolean, IGameObject entity);

        /// <summary>causes the specified damage at the specified flag.</summary>
        void damage_new(DamageEffectTag damage, ILocationFlag cutscene_flag);

        /// <summary>causes the specified damage at the specified object.</summary>
        void damage_object(DamageEffectTag damage, IGameObject entity);

        /// <summary>damages all players with the given damage effect</summary>
        void damage_players(DamageEffectTag damage);

        /// <summary>sets the mission segment for single player data mine events</summary>
        void data_mine_set_mission_segment(string value);

        /// <summary>deactivates a nav point type attached to a team anchored to a flag</summary>
        void deactivate_team_nav_point_flag(ITeam team, ILocationFlag cutscene_flag);

        /// <summary>deactivates a nav point type attached to a team anchored to an object</summary>
        void deactivate_team_nav_point_object(ITeam team);

        /// <summary>deactivates a nav point type attached to a team anchored to an object</summary>
        void deactivate_team_nav_point_object(ITeam team, IGameObject entity);

        /// <summary>animate the overlay over time</summary>
        void device_animate_overlay(IDevice device, float real, float real1, float real12, float real123);

        /// <summary>animate the position over time</summary>
        void device_animate_position(IDevice device, float desiredPosition, float seconds, float real12, float real123, bool boolean);

        /// <summary>TRUE makes the given device close automatically after it has opened, FALSE makes it not</summary>
        void device_closes_automatically_set(IDevice device, bool boolean);

        /// <summary>gets the current position of the given device (used for devices without explicit device groups)</summary>
        float device_get_position(IDevice device);

        /// <summary>TRUE allows a device to change states only once</summary>
        void device_group_change_only_once_more_set(IDeviceGroup device_group, bool boolean);

        /// <summary>returns the desired value of the specified device group.</summary>
        float device_group_get(IDeviceGroup device_group);

        /// <summary>changes the desired value of the specified device group.</summary>
        void device_group_set(IDevice device, IDeviceGroup device_group, float real);

        /// <summary>instantaneously changes the value of the specified device group.</summary>
        void device_group_set_immediate(IDeviceGroup device_group, float real);

        /// <summary>TRUE makes the given device one-sided (only able to be opened from one direction), FALSE makes it two-sided</summary>
        void device_one_sided_set(IDevice device, bool boolean);

        /// <summary>TRUE makes the given device open automatically when any biped is nearby, FALSE makes it not</summary>
        void device_operates_automatically_set(IDevice device, bool boolean);

        /// <summary>changes a machine's never_appears_locked flag, but only if paul is a bastard</summary>
        void device_set_never_appears_locked(IDevice device, bool boolean);

        /// <summary>set the desired overlay animation to use</summary>
        void device_set_overlay_track(IDevice device, string string_id);

        /// <summary>set the desired position of the given device (used for devices without explicit device groups)</summary>
        void device_set_position(IDevice device, float real);

        /// <summary>instantaneously changes the position of the given device (used for devices without explicit device groups</summary>
        void device_set_position_immediate(IDevice device, float real);

        /// <summary>set the desired position track animation to use (optional interpolation time onto track)</summary>
        void device_set_position_track(IDevice device, string string_id, float real);

        /// <summary>immediately sets the power of a named device to the given value</summary>
        void device_set_power(IDevice device, float real);

        void disable_render_light_suppressor();

        /// <summary>drops the named tag e.g. objects\vehicles\banshee\banshee.vehicle</summary>
        void drop(string value);

        /// <summary>starts the specified effect at the specified flag.</summary>
        void effect_new(EffectTag effect, ILocationFlag cutscene_flag);

        /// <summary>starts the specified effect on the specified object at the specified marker.</summary>
        void effect_new_on_object_marker(EffectTag effect, IGameObject entity, string string_id);

        /// <summary>starts the specified effect on the specified object at the specified marker.</summary>
        void effect_new_on_object_marker(EffectTag effect, string emotion);

        /// <summary>enables the code that constrains the max # active lights</summary>
        void enable_render_light_suppressor();

        /// <summary>does a screen fade in from a particular color</summary>
        void fade_in(float real, float real1, float real12, short value);

        /// <summary>does a screen fade out to a particular color</summary>
        void fade_out(float real, float real1, float real12, short value);

        /// <summary>The flock starts producing boids</summary>
        void flock_start(string string_id);

        /// <summary>The flock stops producing boids</summary>
        void flock_stop(string string_id);

        /// <summary>allows or disallows the user of player flashlights</summary>
        void game_can_use_flashlights(bool boolean);

        /// <summary>returns the current difficulty setting, but lies to you and will never return easy, instead returning normal</summary>
        string game_difficulty_get();

        /// <summary>returns the actual current difficulty setting without lying</summary>
        string game_difficulty_get_real();

        /// <summary>returns TRUE if the game is cooperative</summary>
        bool game_is_cooperative();

        /// <summary>returns the hs global boolean 'global_playtest_mode' which can be set in your init.txt</summary>
        bool game_is_playtest();

        /// <summary>causes the player to revert to his previous saved game (for testing, the first bastard that does this to me gets it in the head)</summary>
        void game_revert();

        /// <summary>don't use this for anything, you black-hearted bastards.</summary>
        bool game_reverted();

        /// <summary>returns FALSE if it would be a bad idea to save the player's game right now</summary>
        bool game_safe_to_save();

        /// <summary>checks to see if it is safe to save game, then saves (gives up after 8 seconds)</summary>
        void game_save();

        /// <summary>cancels any pending game_save, timeout or not</summary>
        void game_save_cancel();

        /// <summary>don't use this, except in one place.</summary>
        void game_save_cinematic_skip();

        /// <summary>disregards player's current situation and saves (BE VERY CAREFUL!)</summary>
        void game_save_immediate();

        /// <summary>checks to see if it is safe to save game, then saves (this version never gives up)</summary>
        void game_save_no_timeout();

        /// <summary>checks to see if the game is trying to save the map.</summary>
        bool game_saving();

        /// <summary>causes the player to successfully finish the current level and move to the next</summary>
        void game_won();

        /// <summary>causes all garbage objects except those visible to a player to be collected immediately</summary>
        void garbage_collect_now();

        /// <summary>forces all garbage objects to be collected immediately, even those visible to a player (dangerous!)</summary>
        void garbage_collect_unsafe();

        /// <summary>we fear change</summary>
        void geometry_cache_flush();

        T GetReference<T>(string reference);

        T GetTag<T>(string name, uint id) where T: BaseTag;

        /// <summary>parameter 1 is how, parameter 2 is when</summary>
        void hud_cinematic_fade(float real, float real1);

        /// <summary>true turns training on, false turns it off.</summary>
        void hud_enable_training(bool boolean);

        /// <summary>sets the string id fo the scripted training text</summary>
        void hud_set_training_text(string string_id);

        /// <summary>true turns on scripted training text</summary>
        void hud_show_training_text(bool boolean);

        /// <summary></summary>
        bool ice_cream_flavor_available(int value);

        /// <summary></summary>
        void ice_cream_flavor_stock(int value);

        /// <summary><name> <final value> <time></summary>
        void interpolator_start(string string_id, float real, float real1);

        /// <summary>disables a kill volume</summary>
        void kill_volume_disable(ITriggerVolume trigger_volume);

        /// <summary>enables a kill volume</summary>
        void kill_volume_enable(ITriggerVolume trigger_volume);

        /// <summary>returns the number of objects in a list</summary>
        short list_count(IGameObject e);

        /// <summary>returns the number of objects in a list</summary>
        short list_count(GameObjectList object_list);

        /// <summary>returns the number of objects in a list that aren't dead</summary>
        short list_count_not_dead(GameObjectList objects);

        /// <summary>returns an item in an object list.</summary>
        IGameObject list_get(GameObjectList object_list, int index);

        /// <summary>sets the next loading screen to just fade to white</summary>
        void loading_screen_fade_to_white();

        /// <summary>starts the map from the beginning.</summary>
        void map_reset();

        /// <summary>returns the maximum of all specified expressions.</summary>
        float max(float a, float b);

        /// <summary>returns the minimum of all specified expressions.</summary>
        float min(float a, float b);

        /// <summary>clears the mission objectives.</summary>
        void objectives_clear();

        /// <summary>mark objectives 0..n as complete</summary>
        void objectives_finish_up_to(int value);

        /// <summary>show objectives 0..n</summary>
        void objectives_show_up_to(int value);

        /// <summary>attaches the second object to the first both strings can be empty</summary>
        void objects_attach(IGameObject entity, string string_id, IGameObject entity1, string string_id1);

        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the flag.</summary>
        bool objects_can_see_flag(GameObjectList list, ILocationFlag locationFlag, float floatValue);

        // TODO: this overload shouldn't exist. Entities are supposed to be implicitly singleton lists
        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the object.</summary>
        bool objects_can_see_object(IGameObject entity, IGameObject target, float degrees);
        
        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the object.</summary>
        bool objects_can_see_object(GameObjectList list, IGameObject target, float degrees);

        /// <summary>detaches from the given parent object the given child object</summary>
        void objects_detach(IGameObject entity, IGameObject entity1);

        /// <summary>returns minimum distance from any of the specified objects to the specified flag. (returns -1 if there are no objects, or no flag, to check)</summary>
        float objects_distance_to_flag(IGameObject entity, ILocationFlag locationFlag);

        /// <summary>returns minimum distance from any of the specified objects to the specified flag. (returns -1 if there are no objects, or no flag, to check)</summary>
        float objects_distance_to_flag(GameObjectList list, ILocationFlag locationFlag);

        /// <summary>returns minimum distance from any of the specified objects to the specified destination object. (returns -1 if there are no objects to check)</summary>
        float objects_distance_to_object(GameObjectList list, IGameObject entity);

        /// <summary>loads textures/geometry/sounds necessary to present objects that are about to come on-screen</summary>
        void objects_predict(IGameObject entity);

        /// <summary>loads textures/geometry/sounds necessary to present objects that are about to come on-screen</summary>
        void objects_predict(GameObjectList object_list);

        /// <summary>loads textures/geometry/sounds necessary to present objects that are about to come on-screen</summary>
        void objects_predict_high(IGameObject entity);

        IGameObject object_at_marker(IGameObject entity, string stringId);

        /// <summary>Set whether the object can die from damage or not (as opposed to by scripting)</summary>
        void object_cannot_die(IGameObject entity, bool boolean);

        /// <summary>
        /// Set whether the object can die from damage or not (as opposed to by scripting)
        /// Overload introduced by MCC, used in 03b_newmombasa
        /// </summary>
        void object_cannot_die(IUnit entity, IGameObject vehicle, string seat);

        /// <summary>prevents an object from taking damage</summary>
        void object_cannot_take_damage(IGameObject entity);

        /// <summary>prevents an object from taking damage</summary>
        void object_cannot_take_damage(GameObjectList object_list);

        /// <summary>allows an object to take damage again</summary>
        void object_can_take_damage(IGameObject entity);

        /// <summary>allows an object to take damage again</summary>
        void object_can_take_damage(GameObjectList object_list);

        /// <summary>makes an object use the highest lod for the remainder of the levels' cutscenes.</summary>
        void object_cinematic_lod(IGameObject entity, bool boolean);

        /// <summary>makes an object bypass visibility and always render during cinematics.</summary>
        void object_cinematic_visibility(IGameObject entity, bool boolean);

        /// <summary>clears all funciton variables for sin-o-matic use</summary>
        void object_clear_all_function_variables(IGameObject entity);

        /// <summary>clears one funciton variables for sin-o-matic use</summary>
        void object_clear_function_variable(IGameObject entity, string string_id);

        /// <summary>creates an object from the scenario.</summary>
        void object_create(IEntityIdentifier object_name);

        /// <summary>creates an object, destroying it first if it already exists.</summary>
        void object_create_anew(IEntityIdentifier object_name);

        /// <summary>creates anew all objects from the scenario whose names contain the given substring.</summary>
        void object_create_anew_containing(string value);

        /// <summary>creates an object, potentially resulting in multiple objects if it already exists.</summary>
        void object_create_clone(IEntityIdentifier object_name);

        /// <summary>creates all objects from the scenario whose names contain the given substring.</summary>
        void object_create_containing(string value);

        /// <summary>applies damage to a damage section, causing all manner of effects/constraint breakage to occur</summary>
        void object_damage_damage_section(IGameObject entity, string string_id, float real);

        /// <summary>destroys an object.</summary>
        void object_destroy(IGameObject entity);

        /// <summary>destroys all objects from the scenario whose names contain the given substring.</summary>
        void object_destroy_containing(string value);

        /// <summary>destroys all objects matching the type mask</summary>
        void object_destroy_type_mask(int value);

        /// <summary>disabled dynamic simulation for this object (makes it fixed)</summary>
        void object_dynamic_simulation_disable(IGameObject entity, bool boolean);

        /// <summary>returns the ai attached to this object, if any</summary>
        IAiActorDefinition object_get_ai(IGameObject entity);

        /// <summary>returns the health [0,1] of the object, returns -1 if the object does not exist</summary>
        float object_get_health(IGameObject entity);

        /// <summary>returns the parent of the given object</summary>
        IGameObject object_get_parent(IGameObject entity);

        /// <summary>returns the shield [0,1] of the object, returns -1 if the object does not exist</summary>
        float object_get_shield(IGameObject entity);

        /// <summary>hides or shows the object passed in</summary>
        void object_hide(IGameObject entity, bool boolean);

        /// <summary>returns TRUE if the specified model target is destroyed</summary>
        short object_model_targets_destroyed(IGameObject entity, string target);

        /// <summary>when this object deactivates it will be deleted</summary>
        void object_set_deleted_when_deactivated(IGameObject entity);

        /// <summary>sets funciton variable for sin-o-matic use</summary>
        void object_set_function_variable(IGameObject entity, string string_id, float real, float real1);

        /// <summary>sets the desired region (use "" for all regions) to the permutation with the given name, e.g. (object_set_permutation flood "right arm" ~damaged)</summary>
        void object_set_permutation(IGameObject entity, string string_id, string string_id1);

        /// <summary>sets phantom power to be latched at 1.0f or 0.0f</summary>
        void object_set_phantom_power(IGameObject entity, bool boolean);

        /// <summary>sets the desired region (use "" for all regions) to the model state with the given name, e.g. (object_set_region_state marine head destroyed)</summary>
        void object_set_region_state(IGameObject entity, string string_id, IDamageState model_state);

        /// <summary>sets the scale for a given object and interpolates over the given number of frames to achieve that scale</summary>
        void object_set_scale(IGameObject entity, float real, short value);

        /// <summary>sets the shield vitality of the specified object (between 0 and 1).</summary>
        void object_set_shield(IGameObject entity, float real);

        /// <summary>make this objects shield be stunned permanently</summary>
        void object_set_shield_stun_infinite(IGameObject entity);

        /// <summary>Sets the (object-relative) forward velocity of the given object</summary>
        void object_set_velocity(IGameObject entity, float real);

        /// <summary>Sets the (object-relative) forward velocity of the given object</summary>
        void object_set_velocity(IGameObject entity, float real, float real1, float real12);

        /// <summary>moves the specified object to the specified flag.</summary>
        void object_teleport(IGameObject entity, ILocationFlag cutscene_flag);

        /// <summary>loads textures necessary to draw an object that's about to come on-screen.</summary>
        void object_type_predict(BaseTag entity);

        /// <summary>loads textures necessary to draw an object that's about to come on-screen.</summary>
        void object_type_predict_high(BaseTag entity);

        /// <summary>makes an object use the cinematic directional and ambient lights instead of sampling the lightmap.</summary>
        void object_uses_cinematic_lighting(IGameObject entity, bool boolean);

        /// <summary>turn off ground adhesion forces so you can play tricks with gravity</summary>
        void physics_disable_character_ground_adhesion_forces(float real);

        /// <summary>set global gravity acceleration relative to halo standard gravity</summary>
        void physics_set_gravity(float real);

        /// <summary>sets a local frame of motion for updating physics of things that wish to respect it</summary>
        void physics_set_velocity_frame(float real, float real1, float real12);

        /// <summary>returns the first value pinned between the second two</summary>
        float pin(float value, float min, float max);

        /// <summary>true if the first player is looking down</summary>
        bool player0_looking_down();

        /// <summary>true if the first player is looking up</summary>
        bool player0_looking_up();

        /// <summary>returns a list of the players</summary>
        GameObjectList players();

        /// <summary>returns true if any player has hit accept since the last call to (player_action_test_reset).</summary>
        bool player_action_test_accept();

        /// <summary>returns true if any player has hit cancel key since the last call to (player_action_test_reset).</summary>
        bool player_action_test_cancel();

        /// <summary>returns true if any player has used grenade trigger since the last call to (player_action_test_reset).</summary>
        bool player_action_test_grenade_trigger();

        /// <summary>true if the first player pushed backward on lookstick</summary>
        bool player_action_test_lookstick_backward();

        /// <summary>true if the first player pushed forward on lookstick</summary>
        bool player_action_test_lookstick_forward();

        /// <summary>sets down player look down test</summary>
        void player_action_test_look_down_begin();

        /// <summary>ends the look pitch testing</summary>
        void player_action_test_look_pitch_end();

        /// <summary>sets up player look up test</summary>
        void player_action_test_look_up_begin();

        /// <summary>returns true if any player has hit the melee button since the last call to (player_action_test_reset).</summary>
        bool player_action_test_melee();

        /// <summary>returns true if any player has used primary trigger since the last call to (player_action_test_reset).</summary>
        bool player_action_test_primary_trigger();

        /// <summary>resets the player action test state so that all tests will return false.</summary>
        void player_action_test_reset();

        /// <summary>returns true if any player has used vision trigger since the last call to (player_action_test_reset).</summary>
        bool player_action_test_vision_trigger();

        /// <summary>enables/disables camera control globally</summary>
        void player_camera_control(bool boolean);

        /// <summary>toggle player input. the look stick works, but nothing else.</summary>
        void player_disable_movement(bool boolean);

        /// <summary><yaw> <pitch> <roll></summary>
        void player_effect_set_max_rotation(float real, float real1, float real12);

        /// <summary><left> <right></summary>
        void player_effect_set_max_vibration(float real, float real1);

        /// <summary><max_intensity> <attack time></summary>
        void player_effect_start(float real, float real1);

        /// <summary><decay></summary>
        void player_effect_stop(float real);

        /// <summary>toggle player input. the player can still free-look, but nothing else.</summary>
        void player_enable_input(bool boolean);

        /// <summary>returns true if any player has a flashlight on</summary>
        bool player_flashlight_on();

        /// <summary>guess</summary>
        void player_training_activate_flashlight();

        /// <summary>guess</summary>
        void player_training_activate_stealth();

        /// <summary>ur...</summary>
        void play_credits();

        /// <summary>predict a geometry block.</summary>
        void predict_model_section(RenderModelTag render_model, int value);

        /// <summary>predict a geometry block.</summary>
        void predict_structure_section(IBsp structure_bsp, int value, bool boolean);

        /// <summary>prints a string to the console.</summary>
        void print(string value);

        /// <summary>removes the special place that activates everything it sees.</summary>
        void pvs_clear();

        /// <summary>sets the specified object as the special place that activates everything it sees.</summary>
        void pvs_set_object(IGameObject entity);

        /// <summary>returns a random value in the range [lower bound, upper bound)</summary>
        int random_range(int value, int value1);

        /// <summary>enable</summary>
        void rasterizer_bloom_override(bool boolean);

        /// <summary>brightness</summary>
        void rasterizer_bloom_override_brightness(float real);

        /// <summary>threshold</summary>
        void rasterizer_bloom_override_threshold(float real);

        /// <summary>returns a random value in the range [lower bound, upper bound)</summary>
        float real_random_range(float real, float real1);

        /// <summary>enable/disable the specified unit to receive cinematic shadows where the shadow is focused about a radius around a marker name</summary>
        void render_lights_enable_cinematic_shadow(bool boolean, IGameObject entity, string string_id, float real);

        /// <summary>starts a custom looping animation playing on a piece of scenery</summary>
        void scenery_animation_start_loop(IScenery scenery, AnimationGraphTag animation, string emotion);

        /// <summary>starts a custom animation playing on a piece of scenery relative to a parent object</summary>
        void scenery_animation_start_relative(IScenery scenery, AnimationGraphTag animation, string emotion, IGameObject entity);

        /// <summary>starts a custom animation playing on a piece of scenery relative to a parent object</summary>
        void scenery_animation_start_relative(IScenery scenery, string emotion, IGameObject entity);

        /// <summary>returns the number of ticks remaining in a custom animation (or zero, if the animation is over).</summary>
        short scenery_get_animation_time(IScenery scenery);

        /// <summary>this is your brain on drugs</summary>
        void set_global_sound_environment(float real, float real1, float real12, float real123, int value, float real1234);

        /// <summary>pauses execution of this script (or, optionally, another script) for the specified number of ticks.</summary>
        Task sleep(int ticks);

        /// <summary>pauses execution of this script (or, optionally, another script) for the specified number of ticks.</summary>
        Task sleep(short ticks);

        /// <summary>pauses execution of this script forever.</summary>
        void sleep_forever();

        /// <summary>pauses execution of another script forever.</summary>
        void sleep_forever(ScriptReference script);

        /// <summary>pauses execution of this script until the specified condition is true, checking once per second unless a different number of ticks is specified.</summary>
        Task sleep_until(Func<Task<bool>> condition, int ticks = 60, int timeout = -1);

        /// <summary>changes the gain on the specified sound class(es) to the specified gain over the specified number of ticks.</summary>
        void sound_class_set_gain(string value, float gain, int ticks);

        /// <summary>returns the time remaining for the specified impulse sound. DO NOT CALL IN CUTSCENES.</summary>
        int sound_impulse_language_time(SoundTag soundRef);

        /// <summary>your mom part 2.</summary>
        void sound_impulse_predict(SoundTag soundRef);

        /// <summary>plays an impulse sound from the specified source object (or "none"), with the specified scale.</summary>
        void sound_impulse_start(SoundTag sound, IGameObject entity, float floatValue);

        /// <summary>plays an impulse sound from the specified source object (or "none"), with the specified scale and effect.</summary>
        void sound_impulse_start_effect(SoundTag sound, IGameObject entity, float floatValue, string effect);

        /// <summary>stops the specified impulse sound.</summary>
        void sound_impulse_stop(SoundTag sound);

        /// <summary>plays an impulse sound from the specified source object (or "none"), with the specified scale.</summary>
        void sound_impulse_trigger(SoundTag sound, IGameObject source, float floatValue, int intValue);

        /// <summary>enables or disables the alternate loop/alternate end for a looping sound.</summary>
        void sound_looping_set_alternate(LoopingSoundTag looping_sound, bool boolean);

        /// <summary>plays a looping sound from the specified source object (or "none"), with the specified scale.</summary>
        void sound_looping_start(LoopingSoundTag looping_sound, IGameObject entity, float real);

        /// <summary>stops the specified looping sound.</summary>
        void sound_looping_stop(LoopingSoundTag looping_sound);

        /// <summary>call this when transitioning between two cinematics so ambience won't fade in between the skips</summary>
        void sound_suppress_ambience_update_on_revert();

        /// <summary>returns the current structure bsp index</summary>
        short structure_bsp_index();

        /// <summary>takes off your condom and changes to a different structure bsp</summary>
        void switch_bsp(short value);

        /// <summary>leaves your condom on and changes to a different structure bsp by name</summary>
        void switch_bsp_by_name(IBsp structure_bsp);

        /// <summary>don't make me kick your ass</summary>
        void texture_cache_flush();

        /// <summary>turns off the render texture camera</summary>
        void texture_camera_off();

        /// <summary>sets the render texture camera to a given object marker</summary>
        void texture_camera_set_object_marker(IGameObject entity, string string_id, float real);

        /// <summary>resets the time code timer</summary>
        void time_code_reset();

        /// <summary>shows the time code timer</summary>
        void time_code_show(bool boolean);

        /// <summary>starts/stops the time code timer</summary>
        void time_code_start(bool boolean);

        /// <summary>converts an object to a unit.</summary>
        IUnit unit(IGameObject entity);

        /// <summary>sets a group of units' current body and shield vitality</summary>
        void units_set_current_vitality(GameObjectList units, float body, float shield);

        /// <summary>sets a group of units' maximum body and shield vitality</summary>
        void units_set_maximum_vitality(GameObjectList units, float body, float shield);

        /// <summary>adds/resets the unit's health, shield, and inventory (weapons and grenades) to the named profile. resets if third parameter is true, adds if false. weapons will be marked as garbage if fourth parameter is true (for respawning equipment).</summary>
        void unit_add_equipment(IUnit unit, IStartingProfile starting_profile, bool reset, bool isGarbage);
        
        /// <summary>adds/resets the unit's health, shield, and inventory (weapons and grenades) to the named profile. resets if third parameter is true, adds if false. weapons will be marked as garbage if fourth parameter is true (for respawning equipment).</summary>
        void unit_add_equipment(IUnit unit, IEquipment equipment, bool reset, bool isGarbage);

        /// <summary>prevents any of the given units from dropping weapons or grenades when they die</summary>
        void unit_doesnt_drop_items(GameObjectList entities);

        /// <summary>makes a unit exit its vehicle</summary>
        void unit_exit_vehicle(IUnit unit, short value);

        /// <summary>returns the number of ticks remaining in a unit's custom animation (or zero, if the animation is over).</summary>
        short unit_get_custom_animation_time(IUnit unit);

        /// <summary>returns the health [0,1] of the unit, returns -1 if the unit does not exist</summary>
        float unit_get_health(IUnit unit);

        /// <summary>returns the shield [0,1] of the unit, returns -1 if the unit does not exist</summary>
        float unit_get_shield(IUnit unit);

        /// <summary>returns TRUE if the <unit> has <object> as a weapon, FALSE otherwise</summary>
        bool unit_has_weapon(IUnit unit, BaseTag weapon);

        // TODO: Instead of overloads for single objects (rather than a list) there should be
        // a conversion of the single object to list

        /// <summary>prevents any of the given units from being knocked around or playing ping animations</summary>
        void unit_impervious(IGameObject unit, bool boolean);

        /// <summary>prevents any of the given units from being knocked around or playing ping animations</summary>
        void unit_impervious(IAiActorDefinition actor, bool boolean);

        /// <summary>prevents any of the given units from being knocked around or playing ping animations</summary>
        void unit_impervious(GameObjectList object_list, bool boolean);

        /// <summary>returns true if the given unit is seated on a parent unit</summary>
        bool unit_in_vehicle();

        /// <summary>returns true if the given unit is seated on a parent unit</summary>
        bool unit_in_vehicle(IUnit unit);

        /// <summary>returns whether or not the given unit is current emitting an ai</summary>
        bool unit_is_emitting(IUnit unit);

        /// <summary>kills a given unit, no saving throw</summary>
        void unit_kill();

        /// <summary>kills a given unit, no saving throw</summary>
        void unit_kill(IUnit unit);

        /// <summary>kills a given unit silently (doesn't make them play their normal death animation or sound)</summary>
        void unit_kill_silent(IUnit unit);

        /// <summary>used for the tartarus boss fight</summary>
        void unit_only_takes_damage_from_players_team(IUnit unit, bool boolean);

        /// <summary>enable or disable active camo for the given unit over the specified number of seconds</summary>
        void unit_set_active_camo(IUnit unit, bool boolean, float real);

        /// <summary>sets a unit's current body and shield vitality</summary>
        void unit_set_current_vitality(IUnit unit, float body, float shield);

        /// <summary>sets a unit's facial expression by name with weight and transition time</summary>
        void unit_set_emotional_state(IUnit unit, string string_id, float weight, short transitionTime);

        /// <summary>can be used to prevent the player from entering a vehicle</summary>
        void unit_set_enterable_by_player(IUnit unit, bool boolean);

        /// <summary>sets a unit's maximum body and shield vitality</summary>
        void unit_set_maximum_vitality(IUnit unit, float real, float real1);

        /// <summary>stops the custom animation running on the given unit.</summary>
        void unit_stop_custom_animation(IUnit unit);

        /// <summary>returns the driver of a vehicle</summary>
        IGameObject vehicle_driver(IUnit unit);

        /// <summary>makes a list of units (named or by encounter) magically get into a vehicle, in the substring-specified seats (e.g. CD-passenger... empty string matches all seats)</summary>
        void vehicle_load_magic(IGameObject vehicle, string vehicleSeat, IGameObject unit);

        /// <summary>makes a list of units (named or by encounter) magically get into a vehicle, in the substring-specified seats (e.g. CD-passenger... empty string matches all seats)</summary>
        void vehicle_load_magic(IGameObject vehicle, string vehicleSeat, GameObjectList units);

        /// <summary>tests whether the named seat has a specified unit in it (use "" to test all seats for this unit)</summary>
        bool vehicle_test_seat(IVehicle vehicle, string seat, IUnit unit);

        /// <summary>tests whether the named seat has an object in the object list (use "" to test all seats for any unit in the list)</summary>
        bool vehicle_test_seat_list(IVehicle vehicle, string seat, IGameObject subject);

        /// <summary>tests whether the named seat has an object in the object list (use "" to test all seats for any unit in the list)</summary>
        bool vehicle_test_seat_list(IVehicle vehicle, string seat, GameObjectList subjects);

        /// <summary>makes units get out of an object from the substring-specified seats (e.g. CD-passenger... empty string matches all seats)</summary>
        void vehicle_unload(IGameObject entity, string unit_seat_mapping);

        /// <summary>returns list of objects in volume or (max 128).</summary>
        GameObjectList volume_return_objects(ITriggerVolume trigger_volume);

        /// <summary>returns list of objects in volume or (max 128).</summary>
        GameObjectList volume_return_objects_by_type(ITriggerVolume trigger_volume, int value);

        /// <summary>moves all players outside a specified trigger volume to a specified flag.</summary>
        void volume_teleport_players_not_inside(ITriggerVolume trigger_volume, ILocationFlag cutscene_flag);

        /// <summary>returns true if the specified object is within the specified volume.</summary>
        bool volume_test_object(ITriggerVolume trigger_volume, IGameObject entity);

        // TODO: Instead of overloads for single objects (rather than a list) there should be
        // a conversion of the single object to list

        /// <summary>returns true if any of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        bool volume_test_objects(ITriggerVolume trigger, IGameObject entity);

        /// <summary>returns true if any of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        bool volume_test_objects(ITriggerVolume trigger, IAiActorDefinition actor);

        /// <summary>returns true if any of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        bool volume_test_objects(ITriggerVolume trigger_volume, GameObjectList object_list);

        /// <summary>returns true if any (rb: all?) of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        bool volume_test_objects_all(ITriggerVolume trigger, IGameObject entity);

        /// <summary>returns true if any (rb: all?) of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        bool volume_test_objects_all(ITriggerVolume trigger, IAiActorDefinition actor);

        /// <summary>returns true if any (rb: all?) of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        bool volume_test_objects_all(ITriggerVolume trigger, GameObjectList object_list);

        /// <summary>wakes a sleeping script in the next update.</summary>
        void wake(ScriptReference script_name);

        /// <summary>turns the trigger for a weapon  on/off</summary>
        void weapon_enable_warthog_chaingun_light(bool boolean);

        /// <summary>turns the trigger for a weapon  on/off</summary>
        void weapon_hold_trigger(IWeaponReference weapon, int triggerIndex, bool boolean);

        /// <summary><time> <intensity></summary>
        void weather_change_intensity(float time, float intensity);

        /// <summary><time></summary>
        void weather_start(float time);

        /// <summary><time></summary>
        void weather_stop(float time);
    }
}