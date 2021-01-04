using OpenH2.Core.GameObjects;
using OpenH2.Core.Scripting.Execution;
using OpenH2.Core.Tags;
using System;
using System.Threading.Tasks;

namespace OpenH2.Core.Scripting
{
    public interface IScriptEngine
    {
        short TicksPerSecond { get; }

        /// <summary>activates a nav point type <string> attached to a team anchored to a flag with a vertical offset <real>. If the player is not local to the machine, this will fail</summary>
        [ScriptImplementation(639)]
        void activate_team_nav_point_flag(INavigationPoint navpoint, ITeam team, ILocationFlag cutscene_flag, float real);

        /// <summary>activates a nav point type <string> attached to a team anchored to an object with a vertical offset <real>. If the player is not local to the machine, this will fail</summary>
        [ScriptImplementation(640)]
        void activate_team_nav_point_object(INavigationPoint navpoint, ITeam team, IGameObject entity, float real);

        /// <summary>converts an ai reference to an object list.</summary>
        [ScriptImplementation(332)]
        GameObjectList ai_actors(IAiActorDefinition ai);

        /// <summary>creates an allegiance between two teams.</summary>
        [ScriptImplementation(310)]
        void ai_allegiance(ITeam team, ITeam team1);

        /// <summary>attaches the specified list of units to the specified encounter.</summary>
        [ScriptImplementation(287)]
        void ai_attach_units(GameObjectList units, IAiActorDefinition ai);

        /// <summary>attaches the specified list of units to the specified encounter.</summary>
        void ai_attach_units(IUnit unit, IAiActorDefinition ai);

        /// <summary>forces a group of actors to start or stop berserking</summary>
        [ScriptImplementation(322)]
        void ai_berserk(IAiActorDefinition ai, bool boolean);

        /// <summary>makes a group of actors braindead, or restores them to life (in their initial state)</summary>
        [ScriptImplementation(312)]
        void ai_braindead(IAiActorDefinition ai, bool boolean);

        /// <summary>AI cannot die from damage (as opposed to by scripting)</summary>
        [ScriptImplementation(293)]
        void ai_cannot_die(IAiActorDefinition ai, bool boolean);

        /// <summary>Returns the highest integer combat status in the given squad-group/squad/actor</summary>
        [ScriptImplementation(353)]
        short ai_combat_status(IAiActorDefinition ai);

        /// <summary>turn combat dialogue on/off</summary>
        [ScriptImplementation(281)]
        void ai_dialogue_enable(bool boolean);

        /// <summary>enables or disables automatic garbage collection for actors in the specified encounter and/or squad.</summary>
        [ScriptImplementation(300)]
        void ai_disposable(IAiActorDefinition ai, bool boolean);

        // TODO: Instead of overloads for single objects (rather than a list) there should be
        // a conversion of the single object to list

        /// <summary>if TRUE, forces all actors to completely disregard the specified units, otherwise lets them acknowledge the units again</summary>
        void ai_disregard(IGameObject unit, bool boolean);

        /// <summary>if TRUE, forces all actors to completely disregard the specified units, otherwise lets them acknowledge the units again</summary>
        void ai_disregard(IAiActorDefinition actor, bool boolean);

        /// <summary>if TRUE, forces all actors to completely disregard the specified units, otherwise lets them acknowledge the units again</summary>
        [ScriptImplementation(314)]
        void ai_disregard(GameObjectList object_list, bool boolean);

        /// <summary>Instructs the ai in the given squad to get in all their vehicles</summary>
        [ScriptImplementation(348)]
        void ai_enter_squad_vehicles(IAiActorDefinition ai);

        /// <summary>erases the specified encounter and/or squad.</summary>
        [ScriptImplementation(298)]
        void ai_erase(IAiActorDefinition ai);

        /// <summary>erases all AI.</summary>
        [ScriptImplementation(299)]
        void ai_erase_all();

        /// <summary>return the number of actors that are fighting in a squad or squad_group</summary>
        [ScriptImplementation(326)]
        short ai_fighting_count(IAiActorDefinition ai);

        /// <summary>returns the unit/object corresponding to the given actor</summary>
        [ScriptImplementation(284)]
        IGameObject ai_get_object(IAiActorDefinition ai);

        /// <summary>returns the unit/object corresponding to the given actor</summary>
        [ScriptImplementation(285)]
        IUnit ai_get_unit(IAiActorDefinition ai);

        /// <summary>instantly kills the specified encounter and/or squad.</summary>
        [ScriptImplementation(296)]
        void ai_kill(IAiActorDefinition ai);

        /// <summary>instantly and silently (no animation or sound played) kills the specified encounter and/or squad.</summary>
        [ScriptImplementation(297)]
        void ai_kill_silent(IAiActorDefinition ai);

        /// <summary>return the number of living actors in the specified encounter and/or squad.</summary>
        [ScriptImplementation(327)]
        short ai_living_count(IAiActorDefinition ai);

        /// <summary>Make one squad magically aware of another.</summary>
        [ScriptImplementation(305)]
        void ai_magically_see(IAiActorDefinition ai, IAiActorDefinition ai1);

        /// <summary>Make a squad magically aware of a particular object.</summary>
        [ScriptImplementation(306)]
        void ai_magically_see_object(IAiActorDefinition ai, IGameObject value);

        /// <summary>makes all or part of an encounter move to another encounter.</summary>
        [ScriptImplementation(309)]
        void ai_migrate(IAiActorDefinition ai, IAiActorDefinition ai1);

        /// <summary>return the number of non-swarm actors in the specified encounter and/or squad.</summary>
        [ScriptImplementation(331)]
        short ai_nonswarm_count(IAiActorDefinition ai);

        /// <summary>Don't use this for anything other than bug 3926.  AI magically cancels vehicle oversteer.</summary>
        [ScriptImplementation(295)]
        void ai_overcomes_oversteer(IAiActorDefinition ai, bool boolean);

        /// <summary>places the specified squad on the map.</summary>
        [ScriptImplementation(290)]
        void ai_place(IAiActorDefinition ai);

        /// <summary>places the specified squad on the map.</summary>
        [ScriptImplementation(291)]
        void ai_place(IAiActorDefinition ai, int value);

        /// <summary>places the specified squad (1st arg) on the map in the vehicles belonging to the specified vehicle squad (2nd arg).</summary>
        [ScriptImplementation(292)]
        void ai_place_in_vehicle(IAiActorDefinition ai, IAiActorDefinition ai1);

        /// <summary>Play the given mission dialogue line on the given ai</summary>
        [ScriptImplementation(360)]
        short ai_play_line(IAiActorDefinition ai, string string_id);

        /// <summary>Play the given mission dialogue line on the given ai, directing the ai's gaze at the nearest visible player</summary>
        [ScriptImplementation(361)]
        short ai_play_line_at_player(IAiActorDefinition ai, string string_id);

        /// <summary>Play the given mission dialogue line on the given object (uses first available variant)</summary>
        [ScriptImplementation(362)]
        short ai_play_line_on_object(IGameObject entity, string string_id);

        /// <summary>if TRUE, *ALL* enemies will prefer to attack the specified units. if FALSE, removes the preference.</summary>
        [ScriptImplementation(315)]
        void ai_prefer_target(GameObjectList units, bool boolean);

        /// <summary>refreshes the health and grenade count of a group of actors, so they are as good as new</summary>
        [ScriptImplementation(317)]
        void ai_renew(IAiActorDefinition ai);

        /// <summary>Start the named scene, with the named command script on the named squad</summary>
        [ScriptImplementation(363)]
        bool ai_scene(string scene_name, IScriptMethodReference ai_command_script, IAiActorDefinition ai);

        /// <summary>Start the named scene, with the named command script on the named squad</summary>
        [ScriptImplementation(364)]
        bool ai_scene(string scene_name, IScriptMethodReference ai_command_script, IAiActorDefinition ai, IAiActorDefinition ai1);

        /// <summary>Turn on active camoflage on actor/squad/squad-group</summary>
        [ScriptImplementation(307)]
        void ai_set_active_camo(IAiActorDefinition ai, bool boolean);

        /// <summary>enables or disables sight for actors in the specified encounter.</summary>
        [ScriptImplementation(304)]
        void ai_set_blind(IAiActorDefinition ai, bool boolean);

        /// <summary>enables or disables hearing for actors in the specified encounter.</summary>
        [ScriptImplementation(303)]
        void ai_set_deaf(IAiActorDefinition ai, bool boolean);

        /// <summary>Takes the squad or squad group (arg1) and gives it the order (arg3) in zone (arg2). Use the zone_name/order_name format</summary>
        [ScriptImplementation(334)]
        void ai_set_orders(IAiActorDefinition ai, IAiOrders ai_orders);

        /// <summary>returns the number of actors spawned in the given squad or squad group</summary>
        [ScriptImplementation(335)]
        short ai_spawn_count(IAiActorDefinition ai);

        /// <summary>return the current strength (average body vitality from 0-1) of the specified encounter and/or squad.</summary>
        [ScriptImplementation(329)]
        float ai_strength(IAiActorDefinition ai);

        /// <summary>Turn on/off combat suppression on actor/squad/squad-group</summary>
        [ScriptImplementation(308)]
        void ai_suppress_combat(IAiActorDefinition ai, bool boolean);

        /// <summary>return the number of swarm actors in the specified encounter and/or squad.</summary>
        [ScriptImplementation(330)]
        short ai_swarm_count(IAiActorDefinition ai);

        /// <summary>teleports a group of actors to the starting locations of their current squad(s) if they are currently outside the world.</summary>
        [ScriptImplementation(316)]
        void ai_teleport_to_starting_location_if_outside_bsp(IAiActorDefinition ai);

        /// <summary>Tests the named trigger on the named squad</summary>
        [ScriptImplementation(337)]
        bool ai_trigger_test(string value, IAiActorDefinition ai);

        /// <summary>tells a group of actors to get into a vehicle... does not interrupt any actors who are already going to vehicles</summary>
        [ScriptImplementation(345)]
        void ai_vehicle_enter(IAiActorDefinition ai, string unit);

        /// <summary>tells a group of actors to get into a vehicle... does not interrupt any actors who are already going to vehicles</summary>
        [ScriptImplementation(344)]
        void ai_vehicle_enter(IAiActorDefinition ai, IUnit unit, string unit_seat_mapping = null);

        /// <summary>the given group of actors is snapped into a vehicle, in the substring-specified seats (e.g. passenger for pelican)... does not interrupt any actors who are already going to vehicles</summary>
        [ScriptImplementation(346)]
        void ai_vehicle_enter_immediate(IAiActorDefinition ai, IUnit unit, string seat = null);

        /// <summary>tells a group of actors to get out of any vehicles that they are in</summary>
        [ScriptImplementation(350)]
        void ai_vehicle_exit(IAiActorDefinition ai);


        /// <summary>Returns the vehicle that the given actor is in.</summary>
        [ScriptImplementation(340)]
        IVehicle ai_vehicle_get(IAiActorDefinition ai);

        /// <summary>Returns the vehicle that was spawned at the given starting location.</summary>
        [ScriptImplementation(341)]
        IVehicle ai_vehicle_get_from_starting_location(IAiActorDefinition ai);

        /// <summary>Reserves the given vehicle (so that AI may not enter it</summary>
        [ScriptImplementation(343)]
        void ai_vehicle_reserve(IVehicle vehicle, bool boolean);

        /// <summary>Reserves the given seat on the given vehicle (so that AI may not enter it</summary>
        [ScriptImplementation(342)]
        void ai_vehicle_reserve_seat(IVehicle vehicle, string string_id, bool boolean);

        /// <summary>Returns true if the ai's units are ALL vitality pinned (see object_vitality_pinned)</summary>
        [ScriptImplementation(294)]
        bool ai_vitality_pinned(IAiActorDefinition ai);

        /// <summary>evaluates the sequence of expressions in random order and returns the last value evaluated.</summary>
        void begin_random(params Func<Task>[] expressions);

        /// <summary>returns true if the movie is done playing</summary>
        [ScriptImplementation(782)]
        bool bink_done();

        /// <summary>given a dead biped, turns on ragdoll</summary>
        [ScriptImplementation(624)]
        void biped_ragdoll(IUnit unit);

        /// <summary>call this to force texture and geometry cache to block until satiated</summary>
        [ScriptImplementation(897)]
        Task cache_block_for_one_frame();

        /// <summary>toggles script control of the camera.</summary>
        [ScriptImplementation(450)]
        void camera_control(bool boolean);

        /// <summary>predict resources at a frame in camera animation.</summary>
        [ScriptImplementation(455)]
        void camera_predict_resources_at_frame(AnimationGraphTag animation, string emotion, IUnit unit, ILocationFlag locationFlag, int intValue);

        /// <summary>predict resources given a camera point</summary>
        [ScriptImplementation(456)]
        void camera_predict_resources_at_point(ICameraPathTarget cutscene_camera_point);

        /// <summary>moves the camera to the specified camera point over the specified number of ticks.</summary>
        [ScriptImplementation(451)]
        void camera_set(ICameraPathTarget cutscene_camera_point, short value);

        /// <summary>begins a prerecorded camera animation synchronized to unit relative to cutscene flag.</summary>
        [ScriptImplementation(454)]
        void camera_set_animation_relative(AnimationGraphTag animation, string id, IUnit unit, ILocationFlag locationFlag);

        /// <summary>sets the field of view</summary>
        [ScriptImplementation(459)]
        void camera_set_field_of_view(float degrees, short ticks);

        /// <summary>returns the number of ticks remaining in the current camera interpolation.</summary>
        [ScriptImplementation(458)]
        short camera_time();

        /// <summary>gives a specific player active camouflage</summary>
        [ScriptImplementation(275)]
        void cheat_active_camouflage_by_player(short value, bool boolean);

        /// <summary>clone the first player's most reasonable weapon and attach it to the specified object's marker</summary>
        [ScriptImplementation(869)]
        void cinematic_clone_players_weapon(IGameObject entity, string string_id, string string_id1);

        /// <summary>enable/disable ambience details in cinematics</summary>
        [ScriptImplementation(878)]
        void cinematic_enable_ambience_details(bool boolean);

        /// <summary>sets the color (red, green, blue) of the cinematic ambient light.</summary>
        [ScriptImplementation(98)]
        void cinematic_lighting_set_ambient_light(float real, float real1, float real12);

        /// <summary>sets the pitch, yaw, and color (red, green, blue) of the cinematic shadowing diffuse and specular directional light.</summary>
        [ScriptImplementation(96)]
        void cinematic_lighting_set_primary_light(float real, float real1, float real12, float real123, float real1234);

        /// <summary>sets the pitch, yaw, and color (red, green, blue) of the cinematic non-shadowing diffuse directional light.</summary>
        [ScriptImplementation(97)]
        void cinematic_lighting_set_secondary_light(float real, float real1, float real12, float real123, float real1234);

        /// <summary>turn off lightmap shadow in cinematics</summary>
        [ScriptImplementation(902)]
        void cinematic_lightmap_shadow_disable();

        /// <summary>turn on lightmap shadow in cinematics</summary>
        [ScriptImplementation(903)]
        void cinematic_lightmap_shadow_enable();

        /// <summary>flag this cutscene as an outro cutscene</summary>
        [ScriptImplementation(877)]
        void cinematic_outro_start();

        /// <summary>transition-time</summary>
        [ScriptImplementation(710)]
        void cinematic_screen_effect_set_crossfade(float real);

        /// <summary>sets dof: <seperation dist>, <near blur lower bound> <upper bound> <time> <far blur lower bound> <upper bound> <time></summary>
        [ScriptImplementation(709)]
        void cinematic_screen_effect_set_depth_of_field(float real, float real1, float real12, float real123, float real1234, float real12345, float real123456);

        /// <summary>starts screen effect pass TRUE to clear</summary>
        [ScriptImplementation(708)]
        void cinematic_screen_effect_start(bool boolean);

        /// <summary>returns control of the screen effects to the rest of the game</summary>
        [ScriptImplementation(712)]
        void cinematic_screen_effect_stop();

        /// <summary></summary>
        [ScriptImplementation(714)]
        void cinematic_set_far_clip_distance(float real);

        /// <summary></summary>
        [ScriptImplementation(713)]
        void cinematic_set_near_clip_distance(float real);

        /// <summary>activates the chapter title</summary>
        [ScriptImplementation(559)]
        void cinematic_set_title(ICinematicTitle cutscene_title);

        /// <summary>sets or removes the letterbox bars</summary>
        [ScriptImplementation(557)]
        void cinematic_show_letterbox(bool boolean);

        /// <summary>sets or removes the letterbox bars</summary>
        [ScriptImplementation(558)]
        void cinematic_show_letterbox_immediate(bool boolean);

        /// <summary></summary>
        [ScriptImplementation(555)]
        void cinematic_skip_start_internal();

        /// <summary></summary>
        [ScriptImplementation(556)]
        void cinematic_skip_stop_internal();

        /// <summary>initializes game to start a cinematic (interruptive) cutscene</summary>
        [ScriptImplementation(553)]
        void cinematic_start();

        /// <summary>initializes the game to end a cinematic (interruptive) cutscene</summary>
        [ScriptImplementation(554)]
        void cinematic_stop();

        /// <summary>displays the named subtitle for <real> seconds</summary>
        [ScriptImplementation(562)]
        void cinematic_subtitle(string string_id, float real);

        /// <summary>Starts the pre-rendered cinematic movie. Introduced by MCC </summary>
        [ScriptImplementation(927)]
        void cinematic_start_movie(string name);

        /// <summary>returns TRUE if player0's look pitch is inverted</summary>
        [ScriptImplementation(730)]
        bool controller_get_look_invert();

        /// <summary>invert player0's look</summary>
        [ScriptImplementation(729)]
        void controller_set_look_invert(bool boolean);

        /// <summary>Command script ends prematurely when actor's combat status raises to 'alert' or higher</summary>
        [ScriptImplementation(439)]
        void cs_abort_on_alert(bool boolean);

        /// <summary>Command script ends prematurely when actor's combat status rises to given level</summary>
        [ScriptImplementation(441)]
        void cs_abort_on_combat_status(short value);

        /// <summary>Command script ends prematurely when actor is damaged</summary>
        [ScriptImplementation(440)]
        void cs_abort_on_damage(bool boolean);

        /// <summary>Actor aims at the point for the remainder of the cs, or until overridden (overrides look)</summary>
        [ScriptImplementation(396)]
        void cs_aim(bool boolean, ISpatialPoint point);

        /// <summary>Actor aims at the object for the duration of the cs, or until overridden (overrides look)</summary>
        [ScriptImplementation(398)]
        void cs_aim_object(bool boolean, IGameObject entity);

        /// <summary>Actor aims at nearest player for the duration of the cs, or until overridden (overrides look)</summary>
        [ScriptImplementation(397)]
        void cs_aim_player(bool boolean);

        /// <summary></summary>
        [ScriptImplementation(430)]
        void cs_approach(IGameObject entity, float real, float real1, float real12);

        /// <summary></summary>
        [ScriptImplementation(432)]
        void cs_approach_player(float real, float real1, float real12);

        /// <summary>Actor stops approaching</summary>
        [ScriptImplementation(434)]
        void cs_approach_stop();

        /// <summary>Returns true if the command script is in the ai's cs queue</summary>
        [ScriptImplementation(372)]
        bool cs_command_script_queued(IAiActorDefinition ai, IScriptMethodReference ai_command_script);

        /// <summary>Returns true if the ai is running the command script in question</summary>
        [ScriptImplementation(371)]
        bool cs_command_script_running(IAiActorDefinition ai, IScriptMethodReference ai_command_script);

        /// <summary>Actor crouches for the remainder of the command script, or until overridden</summary>
        [ScriptImplementation(423)]
        void cs_crouch(bool boolean);

        /// <summary>starts a custom animation playing on the unit (interpolates into animation if last parameter is TRUE)</summary>
        [ScriptImplementation(416)]
        void cs_custom_animation(AnimationGraphTag animation, string emotion, float floatValue, bool interpolate);

        /// <summary>Deploy a turret at the given script point</summary>
        [ScriptImplementation(429)]
        void cs_deploy_turret(ISpatialPoint point);

        /// <summary>Actor combat dialogue enabled/disabled.</summary>
        [ScriptImplementation(445)]
        void cs_enable_dialogue(bool boolean);

        /// <summary>Actor autonomous looking enabled/disabled.</summary>
        [ScriptImplementation(443)]
        void cs_enable_looking(bool boolean);

        /// <summary>Actor autonomous moving enabled/disabled.</summary>
        [ScriptImplementation(444)]
        void cs_enable_moving(bool boolean);

        /// <summary>Actor blocks until pathfinding calls succeed</summary>
        [ScriptImplementation(449)]
        void cs_enable_pathfinding_failsafe(bool boolean);

        /// <summary>Actor autonomous target selection enabled/disabled.</summary>
        [ScriptImplementation(442)]
        void cs_enable_targeting(bool boolean);

        /// <summary>Actor faces exactly the point for the remainder of the cs, or until overridden (overrides aim, look)</summary>
        [ScriptImplementation(399)]
        void cs_face(bool boolean, ISpatialPoint point = null);

        /// <summary>Actor faces exactly the given object for the duration of the cs, or until overridden (overrides aim, look)</summary>
        [ScriptImplementation(401)]
        void cs_face_object(bool boolean, IGameObject entity);

        /// <summary>Actor faces exactly the nearest player for the duration of the cs, or until overridden (overrides aim, look)</summary>
        [ScriptImplementation(400)]
        void cs_face_player(bool boolean);

        /// <summary>Flies the actor through the given point</summary>
        [ScriptImplementation(383)]
        void cs_fly_by(ISpatialPoint point);

        /// <summary>Flies the actor through the given point</summary>
        [ScriptImplementation(384)]
        void cs_fly_by(ISpatialPoint point, float tolerance);

        /// <summary>Flies the actor to the given point (within the given tolerance)</summary>
        [ScriptImplementation(379)]
        void cs_fly_to(ISpatialPoint point);

        /// <summary>Flies the actor to the given point (within the given tolerance)</summary>
        [ScriptImplementation(380)]
        void cs_fly_to(ISpatialPoint point, float tolerance);

        /// <summary>Flies the actor to the given point and orients him in the appropriate direction (within the given tolerance)</summary>
        [ScriptImplementation(381)]
        void cs_fly_to_and_face(ISpatialPoint point, ISpatialPoint face);

        /// <summary>Flies the actor to the given point and orients him in the appropriate direction (within the given tolerance)</summary>
        [ScriptImplementation(382)]
        void cs_fly_to_and_face(ISpatialPoint point, ISpatialPoint face, float tolerance);

        /// <summary>Force the actor's combat status (0= no override, 1= asleep, 2=idle, 3= alert, 4= active)</summary>
        [ScriptImplementation(448)]
        void cs_force_combat_status(short value);

        /// <summary>Actor moves toward the point, and considers it hit when it breaks the indicated plane</summary>
        [ScriptImplementation(387)]
        void cs_go_by(ISpatialPoint point, ISpatialPoint planeP, float planeD = 0);

        /// <summary>Moves the actor to a specified point</summary>
        [ScriptImplementation(385)]
        void cs_go_to(ISpatialPoint point);

        /// <summary>Moves the actor to a specified point</summary>
        [ScriptImplementation(386)]
        void cs_go_to(ISpatialPoint point, float tolerance);

        /// <summary>Moves the actor to a specified point and has him face the second point</summary>
        [ScriptImplementation(389)]
        void cs_go_to_and_face(ISpatialPoint point, ISpatialPoint faceTowards);

        /// <summary>Given a point set, AI goes toward the nearest point</summary>
        [ScriptImplementation(391)]
        void cs_go_to_nearest(ISpatialPoint destination);

        /// <summary>Actor gets in the appropriate vehicle</summary>
        [ScriptImplementation(426)]
        void cs_go_to_vehicle(IVehicle vehicle);

        /// <summary>Actor throws a grenade, either by tossing (arg2=0), lobbing (1) or bouncing (2)</summary>
        [ScriptImplementation(408)]
        void cs_grenade(ISpatialPoint point, int action);

        /// <summary>Actor does not avoid obstacles when true</summary>
        [ScriptImplementation(435)]
        void cs_ignore_obstacles(bool boolean);

        /// <summary>Actor jumps in direction of angle at the given velocity (angle, velocity)</summary>
        [ScriptImplementation(409)]
        void cs_jump(float real, float real1);

        /// <summary>Actor jumps with given horizontal and vertical velocity</summary>
        [ScriptImplementation(410)]
        void cs_jump_to_point(float real, float real1);

        /// <summary>Actor looks at the point for the remainder of the cs, or until overridden</summary>
        [ScriptImplementation(393)]
        void cs_look(bool boolean, ISpatialPoint point = null);

        /// <summary>Actor looks at the object for the duration of the cs, or until overridden</summary>
        [ScriptImplementation(395)]
        void cs_look_object(bool boolean, IGameObject entity);

        /// <summary>Actor looks at nearest player for the duration of the cs, or until overridden</summary>
        [ScriptImplementation(394)]
        void cs_look_player(bool boolean);

        /// <summary>Actor switches to given animation mode</summary>
        [ScriptImplementation(422)]
        void cs_movement_mode(short value);

        /// <summary>Actor moves at given angle, for the given distance, optionally with the given facing (angle, distance, facing)</summary>
        [ScriptImplementation(402)]
        void cs_move_in_direction(float real, float real1, float real12);

        /// <summary>Returns TRUE if the actor is currently following a path</summary>
        [ScriptImplementation(392)]
        bool cs_moving();

        /// <summary>The actor does nothing for the given number of seconds</summary>
        [ScriptImplementation(403)]
        void cs_pause(float real);

        /// <summary>Play the named line in the current scene</summary>
        [ScriptImplementation(418)]
        void cs_play_line(string string_id);

        /// <summary>Add a command script onto the end of an actor's command script queue</summary>
        [ScriptImplementation(367)]
        void cs_queue_command_script(IAiActorDefinition ai, IScriptMethodReference ai_command_script);

        /// <summary>Causes the specified actor(s) to start executing a command script immediately (discarding any other command scripts in the queue)</summary>
        [ScriptImplementation(366)]
        void cs_run_command_script(IAiActorDefinition ai, IScriptMethodReference ai_command_script);

        /// <summary>Actor performs the indicated behavior</summary>
        [ScriptImplementation(427)]
        void cs_set_behavior(IAIBehavior ai_behavior);

        /// <summary>Actor is allowed to shoot at its target or not</summary>
        [ScriptImplementation(404)]
        void cs_shoot(bool boolean);

        /// <summary>Actor is allowed to shoot at its target or not</summary>
        [ScriptImplementation(405)]
        void cs_shoot(bool boolean, IGameObject entity);

        /// <summary>Actor shoots at given point</summary>
        [ScriptImplementation(406)]
        void cs_shoot_point(bool boolean, ISpatialPoint point);

        /// <summary>Push a command script to the top of the actor's command script queue</summary>
        [ScriptImplementation(368)]
        void cs_stack_command_script(IAiActorDefinition ai, IScriptMethodReference ai_command_script);

        /// <summary></summary>
        [ScriptImplementation(431)]
        void cs_start_approach(IGameObject entity, float real, float real1, float real12);

        /// <summary></summary>
        [ScriptImplementation(433)]
        void cs_start_approach_player(float real, float real1, float real12);

        /// <summary>Moves the actor to a specified point. DOES NOT BLOCK SCRIPT EXECUTION.</summary>
        [ScriptImplementation(390)]
        void cs_start_to(ISpatialPoint destination);

        /// <summary>Stop running a custom animation</summary>
        [ScriptImplementation(417)]
        void cs_stop_custom_animation();

        /// <summary>Combat dialogue is suppressed for the remainder of the command script</summary>
        [ScriptImplementation(446)]
        void cs_suppress_dialogue_global(bool boolean);

        /// <summary>Switch control of the joint command script to the given member</summary>
        [ScriptImplementation(374)]
        void cs_switch(string string_id);
        /// <summary>Actor teleports to point1 facing point2</summary>
        [ScriptImplementation(420)]
        void cs_teleport(ISpatialPoint destination, ISpatialPoint facing);

        /// <summary>Set the sharpness of a vehicle turn (values 0 -> 1). Only applicable to nondirectional flying vehicles (e.g. dropships)</summary>
        [ScriptImplementation(436)]
        void cs_turn_sharpness(bool boolean, float real);

        /// <summary>Enables or disables boost</summary>
        [ScriptImplementation(438)]
        void cs_vehicle_boost(bool boolean);

        /// <summary>Set the speed at which the actor will drive a vehicle, expressed as a multiplier 0-1</summary>
        [ScriptImplementation(407)]
        void cs_vehicle_speed(float real);

        /// <summary>starts a custom animation playing on a unit (interpolates into animation if last parameter is TRUE)</summary>
        [ScriptImplementation(201)]
        void custom_animation(IUnit unit, AnimationGraphTag animation, string stringid, bool interpolate);

        /// <summary>starts a custom animation playing on a unit (interpolates into animation if last parameter is TRUE)</summary>
        [ScriptImplementation(202)]
        void custom_animation_loop(IUnit unit, AnimationGraphTag animation1, string emotion, bool interpolate);

        /// <summary>starts a custom animation relative to some other object (interpolates into animation if last parameter is TRUE)</summary>
        [ScriptImplementation(203)]
        void custom_animation_relative(IUnit entity, AnimationGraphTag animation, string emotion, bool boolean, IGameObject other);

        /// <summary>starts a custom animation relative to some other object (interpolates into animation if last parameter is TRUE)</summary>
        [ScriptImplementation(204)]
        void custom_animation_relative_loop(IUnit unit, AnimationGraphTag animation2, string emotion, bool boolean, IGameObject entity);

        /// <summary>causes the specified damage at the specified flag.</summary>
        [ScriptImplementation(42)]
        void damage_new(DamageEffectTag damage, ILocationFlag cutscene_flag);

        /// <summary>causes the specified damage at the specified object.</summary>
        [ScriptImplementation(43)]
        void damage_object(DamageEffectTag damage, IGameObject entity);

        /// <summary>damages all players with the given damage effect</summary>
        [ScriptImplementation(45)]
        void damage_players(DamageEffectTag damage);

        /// <summary>sets the mission segment for single player data mine events</summary>
        [ScriptImplementation(786)]
        void data_mine_set_mission_segment(string value);

        /// <summary>deactivates a nav point type attached to a team anchored to a flag</summary>
        [ScriptImplementation(643)]
        void deactivate_team_nav_point_flag(ITeam team, ILocationFlag cutscene_flag);

        /// <summary>deactivates a nav point type attached to a team anchored to an object</summary>
        [ScriptImplementation(644)]
        void deactivate_team_nav_point_object(ITeam team, IGameObject entity);

        /// <summary>animate the overlay over time</summary>
        [ScriptImplementation(269)]
        void device_animate_overlay(IDevice device, float real, float real1, float real12, float real123);

        /// <summary>animate the position over time</summary>
        [ScriptImplementation(268)]
        void device_animate_position(IDevice device, float desiredPosition, float seconds, float real12, float real123, bool boolean);

        /// <summary>TRUE makes the given device close automatically after it has opened, FALSE makes it not</summary>
        [ScriptImplementation(264)]
        void device_closes_automatically_set(IDevice device, bool boolean);

        /// <summary>gets the current position of the given device (used for devices without explicit device groups)</summary>
        [ScriptImplementation(257)]
        float device_get_position(IDevice device);

        /// <summary>TRUE allows a device to change states only once</summary>
        [ScriptImplementation(265)]
        void device_group_change_only_once_more_set(IDeviceGroup device_group, bool boolean);

        /// <summary>returns the desired value of the specified device group.</summary>
        [ScriptImplementation(259)]
        float device_group_get(IDeviceGroup device_group);

        /// <summary>changes the desired value of the specified device group.</summary>
        [ScriptImplementation(260)]
        void device_group_set(IDevice device, IDeviceGroup device_group, float real);

        /// <summary>instantaneously changes the value of the specified device group.</summary>
        [ScriptImplementation(261)]
        void device_group_set_immediate(IDeviceGroup device_group, float real);

        /// <summary>TRUE makes the given device one-sided (only able to be opened from one direction), FALSE makes it two-sided</summary>
        [ScriptImplementation(262)]
        void device_one_sided_set(IDevice device, bool boolean);

        /// <summary>TRUE makes the given device open automatically when any biped is nearby, FALSE makes it not</summary>
        [ScriptImplementation(263)]
        void device_operates_automatically_set(IDevice device, bool boolean);

        /// <summary>changes a machine's never_appears_locked flag, but only if paul is a bastard</summary>
        [ScriptImplementation(253)]
        void device_set_never_appears_locked(IDevice device, bool boolean);

        /// <summary>set the desired overlay animation to use</summary>
        [ScriptImplementation(267)]
        void device_set_overlay_track(IDevice device, string string_id);

        /// <summary>set the desired position of the given device (used for devices without explicit device groups)</summary>
        [ScriptImplementation(256)]
        void device_set_position(IDevice device, float real);

        /// <summary>instantaneously changes the position of the given device (used for devices without explicit device groups</summary>
        [ScriptImplementation(258)]
        void device_set_position_immediate(IDevice device, float real);

        /// <summary>set the desired position track animation to use (optional interpolation time onto track)</summary>
        [ScriptImplementation(266)]
        void device_set_position_track(IDevice device, string string_id, float real);

        /// <summary>immediately sets the power of a named device to the given value</summary>
        [ScriptImplementation(254)]
        void device_set_power(IDevice device, float real);

        [ScriptImplementation(926)]
        void disable_render_light_suppressor();

        /// <summary>drops the named tag e.g. objects\vehicles\banshee\banshee.vehicle</summary>
        [ScriptImplementation(277)]
        void drop(string value);

        /// <summary>starts the specified effect at the specified flag.</summary>
        [ScriptImplementation(40)]
        void effect_new(EffectTag effect, ILocationFlag cutscene_flag);

        /// <summary>starts the specified effect on the specified object at the specified marker.</summary>
        [ScriptImplementation(41)]
        void effect_new_on_object_marker(EffectTag effect, IGameObject entity, string string_id);

        /// <summary>enables the code that constrains the max # active lights</summary>
        [ScriptImplementation(925)]
        void enable_render_light_suppressor();

        /// <summary>does a screen fade in from a particular color</summary>
        [ScriptImplementation(551)]
        void fade_in(float real, float real1, float real12, short value);

        /// <summary>does a screen fade out to a particular color</summary>
        [ScriptImplementation(552)]
        void fade_out(float real, float real1, float real12, short value);

        /// <summary>The flock starts producing boids</summary>
        [ScriptImplementation(354)]
        void flock_start(string string_id);

        /// <summary>The flock stops producing boids</summary>
        [ScriptImplementation(355)]
        void flock_stop(string string_id);

        /// <summary>allows or disallows the user of player flashlights</summary>
        [ScriptImplementation(570)]
        void game_can_use_flashlights(bool boolean);

        /// <summary>returns the current difficulty setting, but lies to you and will never return easy, instead returning normal</summary>
        [ScriptImplementation(467)]
        IGameDifficulty game_difficulty_get();

        /// <summary>returns the actual current difficulty setting without lying</summary>
        [ScriptImplementation(468)]
        IGameDifficulty game_difficulty_get_real();

        /// <summary>returns TRUE if the game is cooperative</summary>
        [ScriptImplementation(568)]
        bool game_is_cooperative();

        /// <summary>returns the hs global boolean 'global_playtest_mode' which can be set in your init.txt</summary>
        [ScriptImplementation(569)]
        bool game_is_playtest();

        /// <summary>causes the player to revert to his previous saved game (for testing, the first bastard that does this to me gets it in the head)</summary>
        [ScriptImplementation(567)]
        void game_revert();

        /// <summary>don't use this for anything, you black-hearted bastards.</summary>
        [ScriptImplementation(592)]
        bool game_reverted();

        /// <summary>returns FALSE if it would be a bad idea to save the player's game right now</summary>
        [ScriptImplementation(584)]
        bool game_safe_to_save();

        /// <summary>checks to see if it is safe to save game, then saves (gives up after 8 seconds)</summary>
        [ScriptImplementation(587)]
        void game_save();

        /// <summary>cancels any pending game_save, timeout or not</summary>
        [ScriptImplementation(588)]
        void game_save_cancel();

        /// <summary>don't use this, except in one place.</summary>
        [ScriptImplementation(876)]
        void game_save_cinematic_skip();

        /// <summary>disregards player's current situation and saves (BE VERY CAREFUL!)</summary>
        [ScriptImplementation(590)]
        void game_save_immediate();

        /// <summary>checks to see if it is safe to save game, then saves (this version never gives up)</summary>
        [ScriptImplementation(589)]
        void game_save_no_timeout();

        /// <summary>checks to see if the game is trying to save the map.</summary>
        [ScriptImplementation(591)]
        bool game_saving();

        /// <summary>causes the player to successfully finish the current level and move to the next</summary>
        [ScriptImplementation(565)]
        void game_won();

        /// <summary>causes all garbage objects except those visible to a player to be collected immediately</summary>
        [ScriptImplementation(87)]
        void garbage_collect_now();

        /// <summary>forces all garbage objects to be collected immediately, even those visible to a player (dangerous!)</summary>
        [ScriptImplementation(88)]
        void garbage_collect_unsafe();

        /// <summary>we fear change</summary>
        [ScriptImplementation(531)]
        void geometry_cache_flush();

        T GetReference<T>(string reference);

        T GetTag<T>(string? name, uint id) where T : BaseTag;

        /// <summary>parameter 1 is how, parameter 2 is when</summary>
        [ScriptImplementation(625)]
        void hud_cinematic_fade(float real, float real1);

        /// <summary>true turns training on, false turns it off.</summary>
        [ScriptImplementation(633)]
        void hud_enable_training(bool boolean);

        /// <summary>sets the string id fo the scripted training text</summary>
        [ScriptImplementation(632)]
        void hud_set_training_text(string string_id);

        /// <summary>true turns on scripted training text</summary>
        [ScriptImplementation(631)]
        void hud_show_training_text(bool boolean);

        /// <summary></summary>
        [ScriptImplementation(873)]
        bool ice_cream_flavor_available(int value);

        /// <summary></summary>
        [ScriptImplementation(872)]
        void ice_cream_flavor_stock(int value);

        /// <summary><name> <final value> <time></summary>
        [ScriptImplementation(844)]
        void interpolator_start(string string_id, float real, float real1);

        /// <summary>disables a kill volume</summary>
        [ScriptImplementation(30)]
        void kill_volume_disable(ITriggerVolume trigger_volume);

        /// <summary>enables a kill volume</summary>
        [ScriptImplementation(29)]
        void kill_volume_enable(ITriggerVolume trigger_volume);

        /// <summary>returns the number of objects in a list</summary>
        short list_count(IGameObject e);

        /// <summary>returns the number of objects in a list</summary>
        [ScriptImplementation(38)]
        short list_count(GameObjectList object_list);

        /// <summary>returns the number of objects in a list that aren't dead</summary>
        [ScriptImplementation(39)]
        short list_count_not_dead(GameObjectList objects);

        /// <summary>returns an item in an object list.</summary>
        [ScriptImplementation(37)]
        IGameObject list_get(GameObjectList object_list, int index);

        /// <summary>sets the next loading screen to just fade to white</summary>
        [ScriptImplementation(922)]
        void loading_screen_fade_to_white();

        /// <summary>starts the map from the beginning.</summary>
        [ScriptImplementation(506)]
        void map_reset();

        /// <summary>returns the maximum of all specified expressions.</summary>
        [ScriptImplementation(12)]
        float max(float a, float b);

        /// <summary>returns the minimum of all specified expressions.</summary>
        [ScriptImplementation(11)]
        float min(float a, float b);

        /// <summary>clears the mission objectives.</summary>
        [ScriptImplementation(752)]
        void objectives_clear();

        /// <summary>mark objectives 0..n as complete</summary>
        [ScriptImplementation(754)]
        void objectives_finish_up_to(int value);

        /// <summary>show objectives 0..n</summary>
        [ScriptImplementation(753)]
        void objectives_show_up_to(int value);

        /// <summary>attaches the second object to the first both strings can be empty</summary>
        [ScriptImplementation(74)]
        void objects_attach(IGameObject entity, string string_id, IGameObject entity1, string string_id1);

        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the flag.</summary>
        [ScriptImplementation(113)]
        bool objects_can_see_flag(GameObjectList list, ILocationFlag locationFlag, float floatValue);

        // TODO: this overload shouldn't exist. Entities are supposed to be implicitly singleton lists
        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the object.</summary>
        bool objects_can_see_object(IGameObject entity, IGameObject target, float degrees);

        /// <summary>returns true if any of the specified units are looking within the specified number of degrees of the object.</summary>
        [ScriptImplementation(112)]
        bool objects_can_see_object(GameObjectList list, IGameObject target, float degrees);

        /// <summary>detaches from the given parent object the given child object</summary>
        [ScriptImplementation(76)]
        void objects_detach(IGameObject entity, IGameObject entity1);

        /// <summary>returns minimum distance from any of the specified objects to the specified flag. (returns -1 if there are no objects, or no flag, to check)</summary>
        float objects_distance_to_flag(IGameObject entity, ILocationFlag locationFlag);

        /// <summary>returns minimum distance from any of the specified objects to the specified flag. (returns -1 if there are no objects, or no flag, to check)</summary>
        [ScriptImplementation(115)]
        float objects_distance_to_flag(GameObjectList list, ILocationFlag locationFlag);

        /// <summary>returns minimum distance from any of the specified objects to the specified destination object. (returns -1 if there are no objects to check)</summary>
        [ScriptImplementation(114)]
        float objects_distance_to_object(GameObjectList list, IGameObject entity);

        /// <summary>loads textures/geometry/sounds necessary to present objects that are about to come on-screen</summary>
        void objects_predict(IGameObject entity);

        /// <summary>loads textures/geometry/sounds necessary to present objects that are about to come on-screen</summary>
        [ScriptImplementation(99)]
        void objects_predict(GameObjectList object_list);

        /// <summary>loads textures/geometry/sounds necessary to present objects that are about to come on-screen</summary>
        [ScriptImplementation(100)]
        void objects_predict_high(GameObjectList entity);

        [ScriptImplementation(75)]
        IGameObject object_at_marker(IGameObject entity, string stringId);

        /// <summary>Set whether the object can die from damage or not (as opposed to by scripting)</summary>
        [ScriptImplementation(85)]
        void object_cannot_die(IGameObject entity, bool boolean);

        /// <summary>prevents an object from taking damage</summary>
        void object_cannot_take_damage(IGameObject entity);

        /// <summary>prevents an object from taking damage</summary>
        [ScriptImplementation(90)]
        void object_cannot_take_damage(GameObjectList object_list);

        /// <summary>allows an object to take damage again</summary>
        void object_can_take_damage(IGameObject entity);

        /// <summary>allows an object to take damage again</summary>
        [ScriptImplementation(91)]
        void object_can_take_damage(GameObjectList object_list);

        /// <summary>makes an object use the highest lod for the remainder of the levels' cutscenes.</summary>
        [ScriptImplementation(92)]
        void object_cinematic_lod(IGameObject entity, bool boolean);

        /// <summary>makes an object bypass visibility and always render during cinematics.</summary>
        [ScriptImplementation(94)]
        void object_cinematic_visibility(IGameObject entity, bool boolean);

        /// <summary>clears all funciton variables for sin-o-matic use</summary>
        [ScriptImplementation(62)]
        void object_clear_all_function_variables(IGameObject entity);

        /// <summary>clears one funciton variables for sin-o-matic use</summary>
        [ScriptImplementation(61)]
        void object_clear_function_variable(IGameObject entity, string string_id);

        /// <summary>creates an object from the scenario.</summary>
        [ScriptImplementation(46)]
        void object_create(IEntityIdentifier object_name);

        /// <summary>creates an object, destroying it first if it already exists.</summary>
        [ScriptImplementation(48)]
        void object_create_anew(IEntityIdentifier object_name);

        /// <summary>creates anew all objects from the scenario whose names contain the given substring.</summary>
        [ScriptImplementation(51)]
        void object_create_anew_containing(string value);

        /// <summary>creates an object, potentially resulting in multiple objects if it already exists.</summary>
        [ScriptImplementation(47)]
        void object_create_clone(IEntityIdentifier object_name);

        /// <summary>creates all objects from the scenario whose names contain the given substring.</summary>
        [ScriptImplementation(49)]
        void object_create_containing(string value);

        /// <summary>applies damage to a damage section, causing all manner of effects/constraint breakage to occur</summary>
        [ScriptImplementation(84)]
        void object_damage_damage_section(IGameObject entity, string string_id, float real);

        /// <summary>destroys an object.</summary>
        [ScriptImplementation(52)]
        void object_destroy(IGameObject entity);

        /// <summary>destroys all objects from the scenario whose names contain the given substring.</summary>
        [ScriptImplementation(53)]
        void object_destroy_containing(string value);

        /// <summary>destroys all objects matching the type mask</summary>
        [ScriptImplementation(55)]
        void object_destroy_type_mask(int value);

        /// <summary>disabled dynamic simulation for this object (makes it fixed)</summary>
        [ScriptImplementation(63)]
        void object_dynamic_simulation_disable(IGameObject entity, bool boolean);

        /// <summary>returns the ai attached to this object, if any</summary>
        [ScriptImplementation(336)]
        IAiActorDefinition object_get_ai(IGameObject entity);

        /// <summary>returns the health [0,1] of the object, returns -1 if the object does not exist</summary>
        [ScriptImplementation(69)]
        float object_get_health(IGameObject entity);

        /// <summary>returns the parent of the given object</summary>
        [ScriptImplementation(73)]
        IGameObject object_get_parent(IGameObject entity);

        /// <summary>returns the shield [0,1] of the object, returns -1 if the object does not exist</summary>
        [ScriptImplementation(70)]
        float object_get_shield(IGameObject entity);

        /// <summary>hides or shows the object passed in</summary>
        [ScriptImplementation(57)]
        void object_hide(IGameObject entity, bool boolean);

        /// <summary>returns TRUE if the specified model target is destroyed</summary>
        [ScriptImplementation(83)]
        short object_model_targets_destroyed(IGameObject entity, string target);

        /// <summary>when this object deactivates it will be deleted</summary>
        [ScriptImplementation(80)]
        void object_set_deleted_when_deactivated(IGameObject entity);

        /// <summary>sets funciton variable for sin-o-matic use</summary>
        [ScriptImplementation(60)]
        void object_set_function_variable(IGameObject entity, string string_id, float real, float real1);

        /// <summary>sets the desired region (use "" for all regions) to the permutation with the given name, e.g. (object_set_permutation flood "right arm" ~damaged)</summary>
        [ScriptImplementation(110)]
        void object_set_permutation(IGameObject entity, string string_id, string string_id1);

        /// <summary>sets phantom power to be latched at 1.0f or 0.0f</summary>
        [ScriptImplementation(64)]
        void object_set_phantom_power(IGameObject entity, bool boolean);

        /// <summary>sets the desired region (use "" for all regions) to the model state with the given name, e.g. (object_set_region_state marine head destroyed)</summary>
        [ScriptImplementation(111)]
        void object_set_region_state(IGameObject entity, string string_id, IDamageState model_state);

        /// <summary>sets the scale for a given object and interpolates over the given number of frames to achieve that scale</summary>
        [ScriptImplementation(77)]
        void object_set_scale(IGameObject entity, float real, short value);

        /// <summary>sets the shield vitality of the specified object (between 0 and 1).</summary>
        [ScriptImplementation(107)]
        void object_set_shield(IGameObject entity, float real);

        /// <summary>make this objects shield be stunned permanently</summary>
        [ScriptImplementation(109)]
        void object_set_shield_stun_infinite(IGameObject entity);

        /// <summary>Sets the (object-relative) forward velocity of the given object</summary>
        [ScriptImplementation(78)]
        void object_set_velocity(IGameObject entity, float real);

        /// <summary>Sets the (object-relative) forward velocity of the given object</summary>
        [ScriptImplementation(79)]
        void object_set_velocity(IGameObject entity, float real, float real1, float real12);

        /// <summary>moves the specified object to the specified flag.</summary>
        [ScriptImplementation(105)]
        void object_teleport(IGameObject entity, ILocationFlag cutscene_flag);

        /// <summary>loads textures necessary to draw an object that's about to come on-screen.</summary>
        [ScriptImplementation(104)]
        void object_type_predict(BaseTag entity);

        /// <summary>loads textures necessary to draw an object that's about to come on-screen.</summary>
        [ScriptImplementation(102)]
        void object_type_predict_high(BaseTag entity);

        /// <summary>makes an object use the cinematic directional and ambient lights instead of sampling the lightmap.</summary>
        [ScriptImplementation(95)]
        void object_uses_cinematic_lighting(IGameObject entity, bool boolean);

        /// <summary>turn off ground adhesion forces so you can play tricks with gravity</summary>
        [ScriptImplementation(129)]
        void physics_disable_character_ground_adhesion_forces(float real);

        /// <summary>set global gravity acceleration relative to halo standard gravity</summary>
        [ScriptImplementation(127)]
        void physics_set_gravity(float real);

        /// <summary>sets a local frame of motion for updating physics of things that wish to respect it</summary>
        [ScriptImplementation(128)]
        void physics_set_velocity_frame(float real, float real1, float real12);

        /// <summary>returns the first value pinned between the second two</summary>
        [ScriptImplementation(26)]
        float pin(float value, float min, float max);

        /// <summary>true if the first player is looking down</summary>
        [ScriptImplementation(499)]
        bool player0_looking_down();

        /// <summary>true if the first player is looking up</summary>
        [ScriptImplementation(498)]
        bool player0_looking_up();

        /// <summary>returns a list of the players</summary>
        [ScriptImplementation(28)]
        GameObjectList players();

        /// <summary>returns true if any player has hit accept since the last call to (player_action_test_reset).</summary>
        [ScriptImplementation(488)]
        bool player_action_test_accept();

        /// <summary>returns true if any player has hit cancel key since the last call to (player_action_test_reset).</summary>
        [ScriptImplementation(489)]
        bool player_action_test_cancel();

        /// <summary>returns true if any player has used grenade trigger since the last call to (player_action_test_reset).</summary>
        [ScriptImplementation(481)]
        bool player_action_test_grenade_trigger();

        /// <summary>true if the first player pushed backward on lookstick</summary>
        [ScriptImplementation(504)]
        bool player_action_test_lookstick_backward();

        /// <summary>true if the first player pushed forward on lookstick</summary>
        [ScriptImplementation(503)]
        bool player_action_test_lookstick_forward();

        /// <summary>sets down player look down test</summary>
        [ScriptImplementation(501)]
        void player_action_test_look_down_begin();

        /// <summary>ends the look pitch testing</summary>
        [ScriptImplementation(502)]
        void player_action_test_look_pitch_end();

        /// <summary>sets up player look up test</summary>
        [ScriptImplementation(500)]
        void player_action_test_look_up_begin();

        /// <summary>returns true if any player has hit the melee button since the last call to (player_action_test_reset).</summary>
        [ScriptImplementation(486)]
        bool player_action_test_melee();

        /// <summary>returns true if any player has used primary trigger since the last call to (player_action_test_reset).</summary>
        [ScriptImplementation(480)]
        bool player_action_test_primary_trigger();

        /// <summary>resets the player action test state so that all tests will return false.</summary>
        [ScriptImplementation(478)]
        void player_action_test_reset();

        /// <summary>returns true if any player has used vision trigger since the last call to (player_action_test_reset).</summary>
        [ScriptImplementation(482)]
        bool player_action_test_vision_trigger();

        /// <summary>enables/disables camera control globally</summary>
        [ScriptImplementation(477)]
        void player_camera_control(bool boolean);

        /// <summary>toggle player input. the look stick works, but nothing else.</summary>
        [ScriptImplementation(474)]
        void player_disable_movement(bool boolean);

        /// <summary><yaw> <pitch> <roll></summary>
        [ScriptImplementation(653)]
        void player_effect_set_max_rotation(float real, float real1, float real12);

        /// <summary><left> <right></summary>
        [ScriptImplementation(654)]
        void player_effect_set_max_vibration(float real, float real1);

        /// <summary><max_intensity> <attack time></summary>
        [ScriptImplementation(655)]
        void player_effect_start(float real, float real1);

        /// <summary><decay></summary>
        [ScriptImplementation(656)]
        void player_effect_stop(float real);

        /// <summary>toggle player input. the player can still free-look, but nothing else.</summary>
        [ScriptImplementation(473)]
        void player_enable_input(bool boolean);

        /// <summary>returns true if any player has a flashlight on</summary>
        [ScriptImplementation(475)]
        bool player_flashlight_on();

        /// <summary>guess</summary>
        [ScriptImplementation(634)]
        void player_training_activate_flashlight();

        /// <summary>guess</summary>
        [ScriptImplementation(636)]
        void player_training_activate_stealth();

        /// <summary>ur...</summary>
        [ScriptImplementation(781)]
        void play_credits();

        /// <summary>predict a geometry block.</summary>
        [ScriptImplementation(880)]
        void predict_model_section(RenderModelTag render_model, int value);

        /// <summary>predict a geometry block.</summary>
        [ScriptImplementation(881)]
        void predict_structure_section(IBsp structure_bsp, int value, bool boolean);

        /// <summary>prints a string to the console.</summary>
        [ScriptImplementation(27)]
        void print(string value);

        /// <summary>removes the special place that activates everything it sees.</summary>
        [ScriptImplementation(471)]
        void pvs_clear();

        /// <summary>sets the specified object as the special place that activates everything it sees.</summary>
        [ScriptImplementation(469)]
        void pvs_set_object(IGameObject entity);

        /// <summary>returns a random value in the range [lower bound, upper bound)</summary>
        [ScriptImplementation(124)]
        int random_range(int value, int value1);

        /// <summary>enable</summary>
        [ScriptImplementation(885)]
        void rasterizer_bloom_override(bool boolean);

        /// <summary>brightness</summary>
        [ScriptImplementation(889)]
        void rasterizer_bloom_override_brightness(float real);

        /// <summary>threshold</summary>
        [ScriptImplementation(888)]
        void rasterizer_bloom_override_threshold(float real);

        /// <summary>returns a random value in the range [lower bound, upper bound)</summary>
        [ScriptImplementation(125)]
        float real_random_range(float real, float real1);

        /// <summary>enable/disable the specified unit to receive cinematic shadows where the shadow is focused about a radius around a marker name</summary>
        [ScriptImplementation(146)]
        void render_lights_enable_cinematic_shadow(bool boolean, IGameObject entity, string string_id, float real);

        /// <summary>starts a custom looping animation playing on a piece of scenery</summary>
        [ScriptImplementation(185)]
        void scenery_animation_start_loop(IScenery scenery, AnimationGraphTag animation, string emotion);

        /// <summary>starts a custom animation playing on a piece of scenery relative to a parent object</summary>
        [ScriptImplementation(186)]
        void scenery_animation_start_relative(IScenery scenery, AnimationGraphTag animation, string emotion, IGameObject entity);

        /// <summary>returns the number of ticks remaining in a custom animation (or zero, if the animation is over).</summary>
        [ScriptImplementation(190)]
        short scenery_get_animation_time(IScenery scenery);

        /// <summary>this is your brain on drugs</summary>
        [ScriptImplementation(874)]
        void set_global_sound_environment(float real, float real1, float real12, float real123, int value, float real1234);

        /// <summary>pauses execution of this script (or, optionally, another script) for the specified number of ticks.</summary>
        Task sleep(int ticks);

        /// <summary>pauses execution of this script (or, optionally, another script) for the specified number of ticks.</summary>
        [ScriptImplementation(19)]
        Task sleep(short ticks, IScriptMethodReference script = null);

        /// <summary>pauses execution of this script (or, optionally, another script) forever.</summary>
        [ScriptImplementation(20)]
        void sleep_forever(IScriptMethodReference script = null);

        /// <summary>pauses execution of this script until the specified condition is true, checking once per second unless a different number of ticks is specified.</summary>
        Task sleep_until(Func<Task<bool>> condition, int ticks = 60, int timeout = -1);

        /// <summary>changes the gain on the specified sound class(es) to the specified gain over the specified number of ticks.</summary>
        [ScriptImplementation(614)]
        void sound_class_set_gain(string value, float gain, int ticks);

        /// <summary>returns the time remaining for the specified impulse sound. DO NOT CALL IN CUTSCENES.</summary>
        [ScriptImplementation(601)]
        int sound_impulse_language_time(SoundTag soundRef);

        /// <summary>your mom part 2.</summary>
        [ScriptImplementation(594)]
        void sound_impulse_predict(SoundTag soundRef);

        /// <summary>plays an impulse sound from the specified source object (or "none"), with the specified scale.</summary>
        [ScriptImplementation(596)]
        void sound_impulse_start(SoundTag sound, IGameObject entity, float floatValue);

        /// <summary>plays an impulse sound from the specified source object (or "none"), with the specified scale and effect.</summary>
        [ScriptImplementation(598)]
        void sound_impulse_start_effect(SoundTag sound, IGameObject entity, float floatValue, string effect);

        /// <summary>stops the specified impulse sound.</summary>
        [ScriptImplementation(602)]
        void sound_impulse_stop(SoundTag sound);

        /// <summary>plays an impulse sound from the specified source object (or "none"), with the specified scale.</summary>
        [ScriptImplementation(595)]
        void sound_impulse_trigger(SoundTag sound, IGameObject source, float floatValue, int intValue);

        /// <summary>enables or disables the alternate loop/alternate end for a looping sound.</summary>
        [ScriptImplementation(609)]
        void sound_looping_set_alternate(LoopingSoundTag looping_sound, bool boolean);

        /// <summary>plays a looping sound from the specified source object (or "none"), with the specified scale.</summary>
        [ScriptImplementation(605)]
        void sound_looping_start(LoopingSoundTag looping_sound, IGameObject entity, float real);

        /// <summary>stops the specified looping sound.</summary>
        [ScriptImplementation(606)]
        void sound_looping_stop(LoopingSoundTag looping_sound);

        /// <summary>call this when transitioning between two cinematics so ambience won't fade in between the skips</summary>
        [ScriptImplementation(898)]
        void sound_suppress_ambience_update_on_revert();

        /// <summary>returns the current structure bsp index</summary>
        [ScriptImplementation(509)]
        short structure_bsp_index();

        /// <summary>takes off your condom and changes to a different structure bsp</summary>
        [ScriptImplementation(507)]
        void switch_bsp(short value);

        /// <summary>leaves your condom on and changes to a different structure bsp by name</summary>
        [ScriptImplementation(508)]
        void switch_bsp_by_name(IBsp structure_bsp);

        /// <summary>don't make me kick your ass</summary>
        [ScriptImplementation(530)]
        void texture_cache_flush();

        /// <summary>turns off the render texture camera</summary>
        [ScriptImplementation(145)]
        void texture_camera_off();

        /// <summary>sets the render texture camera to a given object marker</summary>
        [ScriptImplementation(144)]
        void texture_camera_set_object_marker(IGameObject entity, string string_id, float real);

        /// <summary>resets the time code timer</summary>
        [ScriptImplementation(676)]
        void time_code_reset();

        /// <summary>shows the time code timer</summary>
        [ScriptImplementation(674)]
        void time_code_show(bool boolean);

        /// <summary>starts/stops the time code timer</summary>
        [ScriptImplementation(675)]
        void time_code_start(bool boolean);

        /// <summary>converts an object to a unit.</summary>
        [ScriptImplementation(24)]
        IUnit unit(IGameObject entity);

        /// <summary>sets a group of units' current body and shield vitality</summary>
        [ScriptImplementation(231)]
        void units_set_current_vitality(GameObjectList units, float body, float shield);

        /// <summary>sets a group of units' maximum body and shield vitality</summary>
        [ScriptImplementation(229)]
        void units_set_maximum_vitality(GameObjectList units, float body, float shield);

        /// <summary>adds/resets the unit's health, shield, and inventory (weapons and grenades) to the named profile. resets if third parameter is true, adds if false. weapons will be marked as garbage if fourth parameter is true (for respawning equipment).</summary>
        [ScriptImplementation(250)]
        void unit_add_equipment(IUnit unit, IStartingProfile starting_profile, bool reset, bool isGarbage);

        /// <summary>adds/resets the unit's health, shield, and inventory (weapons and grenades) to the named profile. resets if third parameter is true, adds if false. weapons will be marked as garbage if fourth parameter is true (for respawning equipment).</summary>
        //[ScriptImplementation(250)]
        //void unit_add_equipment(IUnit unit, IEquipment equipment, bool reset, bool isGarbage);

        /// <summary>prevents any of the given units from dropping weapons or grenades when they die</summary>
        [ScriptImplementation(247)]
        void unit_doesnt_drop_items(GameObjectList entities);

        /// <summary>makes a unit exit its vehicle</summary>
        [ScriptImplementation(227)]
        void unit_exit_vehicle(IUnit unit, short value);

        /// <summary>returns the number of ticks remaining in a unit's custom animation (or zero, if the animation is over).</summary>
        [ScriptImplementation(199)]
        short unit_get_custom_animation_time(IUnit unit);

        /// <summary>returns the health [0,1] of the unit, returns -1 if the unit does not exist</summary>
        [ScriptImplementation(239)]
        float unit_get_health(IUnit unit);

        /// <summary>returns the shield [0,1] of the unit, returns -1 if the unit does not exist</summary>
        [ScriptImplementation(240)]
        float unit_get_shield(IUnit unit);

        /// <summary>returns TRUE if the <unit> has <object> as a weapon, FALSE otherwise</summary>
        [ScriptImplementation(242)]
        bool unit_has_weapon(IUnit unit, BaseTag weapon);

        // TODO: Instead of overloads for single objects (rather than a list) there should be
        // a conversion of the single object to list

        /// <summary>prevents any of the given units from being knocked around or playing ping animations</summary>
        void unit_impervious(IGameObject unit, bool boolean);

        /// <summary>prevents any of the given units from being knocked around or playing ping animations</summary>
        void unit_impervious(IAiActorDefinition actor, bool boolean);

        /// <summary>prevents any of the given units from being knocked around or playing ping animations</summary>
        [ScriptImplementation(248)]
        void unit_impervious(GameObjectList object_list, bool boolean);

        /// <summary>returns true if the given unit is seated on a parent unit</summary>
        [ScriptImplementation(222)]
        bool unit_in_vehicle(IUnit unit);

        /// <summary>returns whether or not the given unit is current emitting an ai</summary>
        [ScriptImplementation(198)]
        bool unit_is_emitting(IUnit unit);

        /// <summary>kills a given unit, no saving throw</summary>
        [ScriptImplementation(196)]
        void unit_kill(IUnit unit);

        /// <summary>kills a given unit silently (doesn't make them play their normal death animation or sound)</summary>
        [ScriptImplementation(197)]
        void unit_kill_silent(IUnit unit);

        /// <summary>used for the tartarus boss fight</summary>
        [ScriptImplementation(215)]
        void unit_only_takes_damage_from_players_team(IUnit unit, bool boolean);

        /// <summary>enable or disable active camo for the given unit over the specified number of seconds</summary>
        [ScriptImplementation(193)]
        void unit_set_active_camo(IUnit unit, bool boolean, float real);

        /// <summary>sets a unit's current body and shield vitality</summary>
        [ScriptImplementation(230)]
        void unit_set_current_vitality(IUnit unit, float body, float shield);

        /// <summary>sets a unit's facial expression by name with weight and transition time</summary>
        [ScriptImplementation(220)]
        void unit_set_emotional_state(IUnit unit, string string_id, float weight, short transitionTime);

        /// <summary>can be used to prevent the player from entering a vehicle</summary>
        [ScriptImplementation(213)]
        void unit_set_enterable_by_player(IUnit unit, bool boolean);

        /// <summary>sets a unit's maximum body and shield vitality</summary>
        [ScriptImplementation(228)]
        void unit_set_maximum_vitality(IUnit unit, float real, float real1);

        /// <summary>stops the custom animation running on the given unit.</summary>
        [ScriptImplementation(200)]
        void unit_stop_custom_animation(IUnit unit);

        /// <summary>returns the driver of a vehicle</summary>
        [ScriptImplementation(237)]
        IGameObject vehicle_driver(IUnit unit);

        /// <summary>makes a list of units (named or by encounter) magically get into a vehicle, in the substring-specified seats (e.g. CD-passenger... empty string matches all seats)</summary>
        void vehicle_load_magic(IGameObject vehicle, string vehicleSeat, IGameObject unit);

        /// <summary>makes a list of units (named or by encounter) magically get into a vehicle, in the substring-specified seats (e.g. CD-passenger... empty string matches all seats)</summary>
        [ScriptImplementation(232)]
        void vehicle_load_magic(IGameObject vehicle, string vehicleSeat, GameObjectList units);

        /// <summary>tests whether the named seat has a specified unit in it (use "" to test all seats for this unit)</summary>
        [ScriptImplementation(224)]
        bool vehicle_test_seat(IVehicle vehicle, string seat, IUnit unit);

        /// <summary>tests whether the named seat has an object in the object list (use "" to test all seats for any unit in the list)</summary>
        bool vehicle_test_seat_list(IVehicle vehicle, string seat, IGameObject subject);

        /// <summary>tests whether the named seat has an object in the object list (use "" to test all seats for any unit in the list)</summary>
        [ScriptImplementation(223)]
        bool vehicle_test_seat_list(IVehicle vehicle, string seat, GameObjectList subjects);

        /// <summary>makes units get out of an object from the substring-specified seats (e.g. CD-passenger... empty string matches all seats)</summary>
        [ScriptImplementation(233)]
        void vehicle_unload(IGameObject entity, string unit_seat_mapping);

        /// <summary>returns list of objects in volume or (max 128).</summary>
        [ScriptImplementation(35)]
        GameObjectList volume_return_objects(ITriggerVolume trigger_volume);

        /// <summary>returns list of objects in volume or (max 128).</summary>
        [ScriptImplementation(36)]
        GameObjectList volume_return_objects_by_type(ITriggerVolume trigger_volume, int value);

        /// <summary>moves all players outside a specified trigger volume to a specified flag.</summary>
        [ScriptImplementation(31)]
        void volume_teleport_players_not_inside(ITriggerVolume trigger_volume, ILocationFlag cutscene_flag);

        /// <summary>returns true if the specified object is within the specified volume.</summary>
        [ScriptImplementation(32)]
        bool volume_test_object(ITriggerVolume trigger_volume, IGameObject entity);

        // TODO: Instead of overloads for single objects (rather than a list) there should be
        // a conversion of the single object to list

        /// <summary>returns true if any of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        bool volume_test_objects(ITriggerVolume trigger, IGameObject entity);

        /// <summary>returns true if any of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        bool volume_test_objects(ITriggerVolume trigger, IAiActorDefinition actor);

        /// <summary>returns true if any of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        [ScriptImplementation(33)]
        bool volume_test_objects(ITriggerVolume trigger_volume, GameObjectList object_list);

        /// <summary>returns true if any (rb: all?) of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        bool volume_test_objects_all(ITriggerVolume trigger, IGameObject entity);

        /// <summary>returns true if any (rb: all?) of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        bool volume_test_objects_all(ITriggerVolume trigger, IAiActorDefinition actor);

        /// <summary>returns true if any (rb: all?) of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        [ScriptImplementation(34)]
        bool volume_test_objects_all(ITriggerVolume trigger, GameObjectList object_list);

        /// <summary>wakes a sleeping script in the next update.</summary>
        [ScriptImplementation(22)]
        void wake(IScriptMethodReference script_name);

        /// <summary>turns the trigger for a weapon  on/off</summary>
        [ScriptImplementation(252)]
        void weapon_enable_warthog_chaingun_light(bool boolean);

        /// <summary>turns the trigger for a weapon  on/off</summary>
        [ScriptImplementation(251)]
        void weapon_hold_trigger(IWeaponReference weapon, int triggerIndex, bool boolean);

        /// <summary><time> <intensity></summary>
        [ScriptImplementation(867)]
        void weather_change_intensity(float time, float intensity);

        /// <summary><time></summary>
        [ScriptImplementation(865)]
        void weather_start(float time);

        /// <summary><time></summary>
        [ScriptImplementation(866)]
        void weather_stop(float time);
    }
}

