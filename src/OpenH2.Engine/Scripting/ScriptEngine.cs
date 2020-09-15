namespace OpenH2.Engine.Scripting
{
    using OpenH2.Core.Scripting;
    using OpenH2.Core.Scripting.Execution;
    using OpenH2.Core.Tags.Scenario;
    using OpenH2.Foundation.Logging;
    using System;
    using System.Threading.Tasks;

    public partial class ScriptEngine : IScriptEngine
    {
        public const short TicksPerSecond = 60;
        private readonly IScriptExecutor executionOrchestrator;

        public ScriptEngine(IScriptExecutor executionOrchestrator)
        {
            this.executionOrchestrator = executionOrchestrator;
        }

        public T GetReference<T>(string reference)
        {
            return default(T);
        }

        /// <summary>activates a nav point type <string> attached to a team anchored to a flag with a vertical offset <real>. If the player is not local to the machine, this will fail</summary>
        public void activate_team_nav_point_flag(NavigationPoint navpoint, Team team, ScenarioTag.LocationFlagDefinition cutscene_flag, float real)
        {
        }

        /// <summary>activates a nav point type <string> attached to a team anchored to an object with a vertical offset <real>. If the player is not local to the machine, this will fail</summary>
        public void activate_team_nav_point_object(NavigationPoint navpoint, Team team, Entity entity, float real)
        {
        }

        /// <summary>evaluates the sequence of expressions in random order and returns the last value evaluated.</summary>
        public void begin_random(params Func<Task>[] expressions)
        {
        }

        /// <summary>returns true if the movie is done playing</summary>
        public bool bink_done()
        {
            return default(bool);
        }

        /// <summary>given a dead biped, turns on ragdoll</summary>
        public void biped_ragdoll(Unit unit)
        {
        }

        /// <summary>toggles script control of the camera.</summary>
        public void camera_control(bool boolean)
        {
        }

        /// <summary>moves the camera to the specified camera point over the specified number of ticks.</summary>
        public void camera_set(ScenarioTag.CameraPathTarget cutscene_camera_point, short value)
        {
        }

        /// <summary>begins a prerecorded camera animation synchronized to unit relative to cutscene flag.</summary>
        public void camera_set_animation_relative(Animation animation, string /*id*/ id, Unit unit, ScenarioTag.LocationFlagDefinition locationFlag)
        {
        }

        /// <summary>sets the field of view</summary>
        public void camera_set_field_of_view(float real, short value)
        {
        }

        /// <summary>returns the number of ticks remaining in the current camera interpolation.</summary>
        public short camera_time()
        {
            return default(short);
        }

        /// <summary>gives a specific player active camouflage</summary>
        public void cheat_active_camouflage_by_player(short value, bool boolean)
        {
        }

        /// <summary>clone the first player's most reasonable weapon and attach it to the specified object's marker</summary>
        public void cinematic_clone_players_weapon(Entity entity, string /*id*/ string_id, string /*id*/ string_id1)
        {
        }

        /// <summary>enable/disable ambience details in cinematics</summary>
        public void cinematic_enable_ambience_details(bool boolean)
        {
        }

        /// <summary>sets the color (red, green, blue) of the cinematic ambient light.</summary>
        public void cinematic_lighting_set_ambient_light(float real, float real1, float real12)
        {
        }

        /// <summary>sets the pitch, yaw, and color (red, green, blue) of the cinematic shadowing diffuse and specular directional light.</summary>
        public void cinematic_lighting_set_primary_light(float real, float real1, float real12, float real123, float real1234)
        {
        }

        /// <summary>sets the pitch, yaw, and color (red, green, blue) of the cinematic non-shadowing diffuse directional light.</summary>
        public void cinematic_lighting_set_secondary_light(float real, float real1, float real12, float real123, float real1234)
        {
        }

        /// <summary>turn off lightmap shadow in cinematics</summary>
        public void cinematic_lightmap_shadow_disable()
        {
        }

        /// <summary>turn on lightmap shadow in cinematics</summary>
        public void cinematic_lightmap_shadow_enable()
        {
        }

        /// <summary>flag this cutscene as an outro cutscene</summary>
        public void cinematic_outro_start()
        {
        }

        /// <summary>transition-time</summary>
        public void cinematic_screen_effect_set_crossfade(float real)
        {
        }

        /// <summary>sets dof: <seperation dist>, <near blur lower bound> <upper bound> <time> <far blur lower bound> <upper bound> <time></summary>
        public void cinematic_screen_effect_set_depth_of_field(float real, float real1, float real12, float real123, float real1234, float real12345, float real123456)
        {
        }

        /// <summary>starts screen effect pass TRUE to clear</summary>
        public void cinematic_screen_effect_start(bool boolean)
        {
        }

        /// <summary>returns control of the screen effects to the rest of the game</summary>
        public void cinematic_screen_effect_stop()
        {
        }

        /// <summary></summary>
        public void cinematic_set_far_clip_distance(float real)
        {
        }

        /// <summary></summary>
        public void cinematic_set_near_clip_distance(float real)
        {
        }

        /// <summary>activates the chapter title</summary>
        public void cinematic_set_title(ScenarioTag.CinematicTitleDefinition cutscene_title)
        {
        }

        /// <summary>sets or removes the letterbox bars</summary>
        public void cinematic_show_letterbox(bool boolean)
        {
        }

        /// <summary>sets or removes the letterbox bars</summary>
        public void cinematic_show_letterbox_immediate(bool boolean)
        {
        }

        /// <summary></summary>
        public void cinematic_skip_start_internal()
        {
        }

        /// <summary></summary>
        public void cinematic_skip_stop_internal()
        {
        }

        /// <summary>initializes game to start a cinematic (interruptive) cutscene</summary>
        public void cinematic_start()
        {
        }

        /// <summary>initializes the game to end a cinematic (interruptive) cutscene</summary>
        public void cinematic_stop()
        {
        }

        /// <summary>displays the named subtitle for <real> seconds</summary>
        public void cinematic_subtitle(string /*id*/ string_id, float real)
        {
        }

        /// <summary>returns TRUE if player0's look pitch is inverted</summary>
        public bool controller_get_look_invert()
        {
            return default(bool);
        }

        /// <summary>invert player0's look</summary>
        public void controller_set_look_invert()
        {
        }

        /// <summary>invert player0's look</summary>
        public void controller_set_look_invert(bool boolean)
        {
        }

        /// <summary>starts a custom animation playing on a unit (interpolates into animation if last parameter is TRUE)</summary>
        public void custom_animation(Unit unit, Animation animation, string /*id*/ stringid, bool interpolate)
        {
        }

        /// <summary>starts a custom animation playing on a unit (interpolates into animation if last parameter is TRUE)</summary>
        public void custom_animation(Unit unit, string /*id*/ emotion, bool interpolate)
        {
        }

        /// <summary>starts a custom animation playing on a unit (interpolates into animation if last parameter is TRUE)</summary>
        public void custom_animation_loop(Unit unit, Animation animation1, string /*id*/ emotion, bool interpolate)
        {
        }

        /// <summary>starts a custom animation playing on a unit (interpolates into animation if last parameter is TRUE)</summary>
        public void custom_animation_loop(string /*id*/ emotion, bool interpolate)
        {
        }

        /// <summary>starts a custom animation relative to some other object (interpolates into animation if last parameter is TRUE)</summary>
        public void custom_animation_relative(Unit unit, string /*id*/ emotion, bool interpolate, Entity entity)
        {
        }

        /// <summary>starts a custom animation relative to some other object (interpolates into animation if last parameter is TRUE)</summary>
        public void custom_animation_relative(Unit entity, Animation animation, string /*id*/ emotion, bool boolean, Entity other)
        {
        }

        /// <summary>starts a custom animation relative to some other object (interpolates into animation if last parameter is TRUE)</summary>
        public void custom_animation_relative_loop(Unit unit, Animation animation2, string /*id*/ emotion, bool boolean, Entity entity)
        {
        }

        /// <summary>causes the specified damage at the specified flag.</summary>
        public void damage_new(Damage damage, ScenarioTag.LocationFlagDefinition cutscene_flag)
        {
        }

        /// <summary>causes the specified damage at the specified object.</summary>
        public void damage_object(Damage damage, Entity entity)
        {
        }

        /// <summary>damages all players with the given damage effect</summary>
        public void damage_players(Damage damage)
        {
        }

        /// <summary>deactivates a nav point type attached to a team anchored to a flag</summary>
        public void deactivate_team_nav_point_flag(Team team, ScenarioTag.LocationFlagDefinition cutscene_flag)
        {
        }

        /// <summary>deactivates a nav point type attached to a team anchored to an object</summary>
        public void deactivate_team_nav_point_object(Team team)
        {
        }

        /// <summary>deactivates a nav point type attached to a team anchored to an object</summary>
        public void deactivate_team_nav_point_object(Team team, Entity entity)
        {
        }

        /// <summary>drops the named tag e.g. objects\vehicles\banshee\banshee.vehicle</summary>
        public void drop(string value)
        {
        }

        /// <summary>starts the specified effect at the specified flag.</summary>
        public void effect_new(Effect effect, ScenarioTag.LocationFlagDefinition cutscene_flag)
        {
        }

        /// <summary>starts the specified effect on the specified object at the specified marker.</summary>
        public void effect_new_on_object_marker(Effect effect, Entity entity, string /*id*/ string_id)
        {
        }

        /// <summary>starts the specified effect on the specified object at the specified marker.</summary>
        public void effect_new_on_object_marker(Effect effect, string /*id*/ emotion)
        {
        }

        /// <summary>enables the code that constrains the max # active lights</summary>
        public void enable_render_light_suppressor()
        {
        }

        /// <summary>does a screen fade in from a particular color</summary>
        public void fade_in(float real, float real1, float real12, short value)
        {
        }

        /// <summary>does a screen fade out to a particular color</summary>
        public void fade_out(float real, float real1, float real12, short value)
        {
        }

        /// <summary>The flock starts producing boids</summary>
        public void flock_start(string /*id*/ string_id)
        {
        }

        /// <summary>The flock stops producing boids</summary>
        public void flock_stop(string /*id*/ string_id)
        {
        }

        /// <summary>allows or disallows the user of player flashlights</summary>
        public void game_can_use_flashlights(bool boolean)
        {
        }

        /// <summary>returns the current difficulty setting, but lies to you and will never return easy, instead returning normal</summary>
        public string game_difficulty_get()
        {
            return "";
        }

        /// <summary>returns the actual current difficulty setting without lying</summary>
        public string game_difficulty_get_real()
        {
            return default(string);
        }

        /// <summary>returns TRUE if the game is cooperative</summary>
        public bool game_is_cooperative()
        {
            return default(bool);
        }

        /// <summary>returns the hs global boolean 'global_playtest_mode' which can be set in your init.txt</summary>
        public bool game_is_playtest()
        {
            return default(bool);
        }

        /// <summary>causes the player to revert to his previous saved game (for testing, the first bastard that does this to me gets it in the head)</summary>
        public void game_revert()
        {
        }

        /// <summary>don't use this for anything, you black-hearted bastards.</summary>
        public bool game_reverted()
        {
            return default(bool);
        }

        /// <summary>returns FALSE if it would be a bad idea to save the player's game right now</summary>
        public bool game_safe_to_save()
        {
            return default(bool);
        }

        /// <summary>checks to see if it is safe to save game, then saves (gives up after 8 seconds)</summary>
        public void game_save()
        {
        }

        /// <summary>cancels any pending game_save, timeout or not</summary>
        public void game_save_cancel()
        {
        }

        /// <summary>don't use this, except in one place.</summary>
        public void game_save_cinematic_skip()
        {
        }

        /// <summary>disregards player's current situation and saves (BE VERY CAREFUL!)</summary>
        public void game_save_immediate()
        {
        }

        /// <summary>checks to see if it is safe to save game, then saves (this version never gives up)</summary>
        public void game_save_no_timeout()
        {
        }

        /// <summary>checks to see if the game is trying to save the map.</summary>
        public bool game_saving()
        {
            return default(bool);
        }

        /// <summary>causes the player to successfully finish the current level and move to the next</summary>
        public void game_won()
        {
        }

        /// <summary>parameter 1 is how, parameter 2 is when</summary>
        public void hud_cinematic_fade(float real, float real1)
        {
        }

        /// <summary>true turns training on, false turns it off.</summary>
        public void hud_enable_training(bool boolean)
        {
        }

        /// <summary>sets the string id fo the scripted training text</summary>
        public void hud_set_training_text(string /*id*/ string_id)
        {
        }

        /// <summary>true turns on scripted training text</summary>
        public void hud_show_training_text(bool boolean)
        {
        }

        /// <summary></summary>
        public bool ice_cream_flavor_available(int value)
        {
            return default(bool);
        }

        /// <summary></summary>
        public void ice_cream_flavor_stock(int value)
        {
        }

        /// <summary><name> <final value> <time></summary>
        public void interpolator_start(string /*id*/ name, float finalValue, float time)
        {
        }

        /// <summary>disables a kill volume</summary>
        public void kill_volume_disable(ScenarioTag.TriggerVolume trigger_volume)
        {
        }

        /// <summary>enables a kill volume</summary>
        public void kill_volume_enable(ScenarioTag.TriggerVolume trigger_volume)
        {
        }

        /// <summary>returns the number of objects in a list</summary>
        public short list_count(Entity e)
        {
            return (short)(e == null ? 0 : 1);
        }

        /// <summary>returns the number of objects in a list</summary>
        public short list_count(EntityList object_list)
        {
            return (short)object_list.Objects.Length;
        }

        /// <summary>returns the number of objects in a list that aren't dead</summary>
        public short list_count_not_dead(EntityList objects)
        {
            return default(short);
        }

        /// <summary>returns an item in an object list.</summary>
        public Entity list_get(EntityList object_list, int index)
        {
            return object_list.Objects[index];
        }

        /// <summary>sets the next loading screen to just fade to white</summary>
        public void loading_screen_fade_to_white()
        {
        }

        /// <summary>starts the map from the beginning.</summary>
        public void map_reset()
        {
        }

        /// <summary>returns the maximum of all specified expressions.</summary>
        public float max(float a, float b)
        {
            return default(float);
        }

        /// <summary>returns the minimum of all specified expressions.</summary>
        public float min(float a, float b)
        {
            return default(float);
        }

        /// <summary>turn off ground adhesion forces so you can play tricks with gravity</summary>
        public void physics_disable_character_ground_adhesion_forces(float real)
        {
        }

        /// <summary>set global gravity acceleration relative to halo standard gravity</summary>
        public void physics_set_gravity(float real)
        {
        }

        /// <summary>sets a local frame of motion for updating physics of things that wish to respect it</summary>
        public void physics_set_velocity_frame(float real, float real1, float real12)
        {
        }

        /// <summary>returns the first value pinned between the second two</summary>
        public float pin(float value, float min, float max)
        {
            return default(float);
        }

        /// <summary>ur...</summary>
        public void play_credits()
        {
        }

        /// <summary>prints a string to the console.</summary>
        public void print(string value)
        {
            Logger.Log(value, Logger.Color.Magenta);
        }

        /// <summary>returns a random value in the range [lower bound, upper bound)</summary>
        public int random_range(int value, int value1)
        {
            return default(int);
        }

        /// <summary>returns a random value in the range [lower bound, upper bound)</summary>
        public float real_random_range(float real, float real1)
        {
            return default(float);
        }

        /// <summary>starts a custom looping animation playing on a piece of scenery</summary>
        public void scenery_animation_start_loop(ScenarioTag.SceneryInstance scenery, Animation animation, string /*id*/ emotion)
        {
        }

        /// <summary>starts a custom animation playing on a piece of scenery relative to a parent object</summary>
        public void scenery_animation_start_relative(ScenarioTag.SceneryInstance scenery, string /*id*/ emotion, Entity entity)
        {
        }

        /// <summary>starts a custom animation playing on a piece of scenery relative to a parent object</summary>
        public void scenery_animation_start_relative(ScenarioTag.SceneryInstance scenery, Animation animation, string /*id*/ emotion, Entity entity)
        {
        }

        /// <summary>returns the number of ticks remaining in a custom animation (or zero, if the animation is over).</summary>
        public short scenery_get_animation_time(ScenarioTag.SceneryInstance scenery)
        {
            return default(short);
        }

        /// <summary>this is your brain on drugs</summary>
        public void set_global_sound_environment(float real, float real1, float real12, float real123, int value, float real1234)
        {
        }

        /// <summary>pauses execution of this script (or, optionally, another script) for the specified number of ticks.</summary>
        public async Task sleep(int ticks)
        {
            if (ticks <= 0) ticks = 1;
            var sleepTime = TimeSpan.FromSeconds(ticks / (double)TicksPerSecond);
            await Task.Delay(sleepTime);
        }

        /// <summary>pauses execution of this script (or, optionally, another script) for the specified number of ticks.</summary>
        public async Task sleep(short ticks)
        {
            if (ticks <= 0) ticks = 1;
            var sleepTime = TimeSpan.FromSeconds(ticks / (double)TicksPerSecond);
            await Task.Delay(sleepTime);
        }

        /// <summary>pauses execution of this script (or, optionally, another script) forever.</summary>
        public void sleep_forever()
        {
            throw new Exception("Aborting, this doesn't work if we expect to be able to resume here...");
        }

        /// <summary>pauses execution of this script (or, optionally, another script) forever.</summary>
        public void sleep_forever(ScriptReference script)
        {
            this.executionOrchestrator.SetStatus(script.Method.Name, ScriptStatus.Sleeping);
        }

        /// <summary>pauses execution of this script until the specified condition is true, checking once per second unless a different number of ticks is specified.</summary>
        public async Task sleep_until(Func<Task<bool>> condition, int ticks = TicksPerSecond, int timeout = -1)
        {
            var start = DateTimeOffset.Now;
            var timeoutOffset = DateTimeOffset.MaxValue;

            if (timeout >= 0)
            {
                timeoutOffset = start.AddSeconds(timeout / (double)TicksPerSecond);
            }

            while (await condition() == false && timeoutOffset > DateTimeOffset.Now)
            {
                await Task.Delay(TimeSpan.FromSeconds(ticks / (double)TicksPerSecond));
            }
        }

        /// <summary>returns the current structure bsp index</summary>
        public short structure_bsp_index()
        {
            return default(short);
        }

        /// <summary>takes off your condom and changes to a different structure bsp</summary>
        public void switch_bsp(short value)
        {
        }

        /// <summary>leaves your condom on and changes to a different structure bsp by name</summary>
        public void switch_bsp_by_name(Bsp structure_bsp)
        {
        }

        /// <summary>turns off the render texture camera</summary>
        public void texture_camera_off()
        {
        }

        /// <summary>sets the render texture camera to a given object marker</summary>
        public void texture_camera_set_object_marker(Entity entity, string /*id*/ string_id, float real)
        {
        }

        /// <summary>returns the driver of a vehicle</summary>
        public Entity vehicle_driver(Unit unit)
        {
            return default(Entity);
        }

        /// <summary>makes a list of units (named or by encounter) magically get into a vehicle, in the substring-specified seats (e.g. CD-passenger... empty string matches all seats)</summary>
        public void vehicle_load_magic(Entity vehicle, /*VehicleSeat*/ string vehicleSeat, EntityList units)
        {
        }

        /// <summary>makes a list of units (named or by encounter) magically get into a vehicle, in the substring-specified seats (e.g. CD-passenger... empty string matches all seats)</summary>
        public void vehicle_load_magic(Entity vehicle, /*VehicleSeat*/ string vehicleSeat, Entity unit)
        {
        }

        /// <summary>tests whether the named seat has a specified unit in it (use "" to test all seats for this unit)</summary>
        public bool vehicle_test_seat(Vehicle vehicle, string seat, Unit unit)
        {
            return default(bool);
        }

        /// <summary>tests whether the named seat has an object in the object list (use "" to test all seats for any unit in the list)</summary>
        public bool vehicle_test_seat_list(Vehicle vehicle, string /*id*/ seat, EntityList subjects)
        {
            return default(bool);
        }

        /// <summary>tests whether the named seat has an object in the object list (use "" to test all seats for any unit in the list)</summary>
        public bool vehicle_test_seat_list(Vehicle vehicle, string /*id*/ seat, Entity subject)
        {
            return default(bool);
        }

        /// <summary>makes units get out of an object from the substring-specified seats (e.g. CD-passenger... empty string matches all seats)</summary>
        public void vehicle_unload(Entity entity, /*VehicleSeat*/ string unit_seat_mapping)
        {
        }

        /// <summary>returns list of objects in volume or (max 128).</summary>
        public EntityList volume_return_objects(ScenarioTag.TriggerVolume trigger_volume)
        {
            return default(EntityList);
        }

        /// <summary>returns list of objects in volume or (max 128).</summary>
        public EntityList volume_return_objects_by_type(ScenarioTag.TriggerVolume trigger_volume, int value)
        {
            return default(EntityList);
        }

        /// <summary>moves all players outside a specified trigger volume to a specified flag.</summary>
        public void volume_teleport_players_not_inside(ScenarioTag.TriggerVolume trigger_volume, ScenarioTag.LocationFlagDefinition cutscene_flag)
        {
        }

        /// <summary>returns true if the specified object is within the specified volume.</summary>
        public bool volume_test_object(ScenarioTag.TriggerVolume trigger)
        {
            return default(bool);
        }

        /// <summary>returns true if the specified object is within the specified volume.</summary>
        public bool volume_test_object(ScenarioTag.TriggerVolume trigger_volume, Entity entity)
        {
            return default(bool);
        }

        /// <summary>returns true if any of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        public bool volume_test_objects(ScenarioTag.TriggerVolume trigger, Entity entity)
        {
            return default(bool);
        }

        /// <summary>returns true if any of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        public bool volume_test_objects(ScenarioTag.TriggerVolume trigger_volume, EntityList object_list)
        {
            return default(bool);
        }

        /// <summary>returns true if any (rb: all?) of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        public bool volume_test_objects_all(ScenarioTag.TriggerVolume trigger, EntityList object_list)
        {
            return default(bool);
        }

        /// <summary>returns true if any (rb: all?) of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        public bool volume_test_objects_all(ScenarioTag.TriggerVolume trigger, Entity entity)
        {
            return default(bool);
        }

        /// <summary>wakes a sleeping script in the next update.</summary>
        public void wake(ScriptReference script_name)
        {
            this.executionOrchestrator.SetStatus(script_name.Method.Name, ScriptStatus.RunOnce);
        }

        /// <summary>turns the trigger for a weapon  on/off</summary>
        public void weapon_enable_warthog_chaingun_light(bool boolean)
        {
        }

        /// <summary>turns the trigger for a weapon  on/off</summary>
        public void weapon_hold_trigger(WeaponReference weapon, int triggerIndex, bool boolean)
        {
        }

        /// <summary><time> <intensity></summary>
        public void weather_change_intensity(float real, float real1)
        {
        }

        /// <summary><time> <intensity></summary>
        public void weather_change_intensity(float floatValue)
        {
        }

        /// <summary><time></summary>
        public void weather_start(float real)
        {
        }

        /// <summary><time></summary>
        public void weather_start()
        {
        }

        /// <summary><time></summary>
        public void weather_stop(float real)
        {
        }

        /// <summary><time></summary>
        public void weather_stop()
        {
        }
    }
}