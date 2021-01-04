namespace OpenH2.Engine.Scripting
{
    using OpenH2.Core.Architecture;
    using OpenH2.Core.GameObjects;
    using OpenH2.Core.Scripting;
    using OpenH2.Core.Scripting.Execution;
    using OpenH2.Core.Tags;
    using OpenH2.Engine.Systems;
    using OpenH2.Foundation.Extensions;
    using OpenH2.Foundation.Logging;
    using System;
    using System.Threading.Tasks;

    public partial class ScriptEngine : IScriptEngine
    {
        private const short ticksPerSecond = 30;
        public short TicksPerSecond => ticksPerSecond;
        private readonly Scene scene;
        private readonly IScriptExecutor executionOrchestrator;
        private readonly AudioSystem audioSystem;
        private readonly CameraSystem cameraSystem;
        private readonly Random rng;

        public ScriptEngine(Scene scene, 
            IScriptExecutor executionOrchestrator,
            AudioSystem audioSystem,
            CameraSystem cameraSystem)
        {
            this.scene = scene;
            this.executionOrchestrator = executionOrchestrator;
            this.audioSystem = audioSystem;
            this.cameraSystem = cameraSystem;
            this.rng = new Random(42);
        }

        public T GetReference<T>(string reference)
        {
            return default(T);
        }

        public T GetTag<T>(string? name, uint id) where T: BaseTag
        {
            return scene.Map.GetTag<T>(id);
        }

        /// <summary>activates a nav point type <string> attached to a team anchored to a flag with a vertical offset <real>. If the player is not local to the machine, this will fail</summary>
        public void activate_team_nav_point_flag(INavigationPoint navpoint, ITeam team, ILocationFlag cutscene_flag, float real)
        {
        }

        /// <summary>activates a nav point type <string> attached to a team anchored to an object with a vertical offset <real>. If the player is not local to the machine, this will fail</summary>
        public void activate_team_nav_point_object(INavigationPoint navpoint, ITeam team, IGameObject entity, float real)
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
        public void biped_ragdoll(IUnit unit)
        {
        }

        /// <summary>gives a specific player active camouflage</summary>
        public void cheat_active_camouflage_by_player(short value, bool boolean)
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
        public void custom_animation(IUnit unit, AnimationGraphTag animation, string /*id*/ stringid, bool interpolate)
        {
        }

        /// <summary>starts a custom animation playing on a unit (interpolates into animation if last parameter is TRUE)</summary>
        public void custom_animation(IUnit unit, string /*id*/ emotion, bool interpolate)
        {
        }

        /// <summary>starts a custom animation playing on a unit (interpolates into animation if last parameter is TRUE)</summary>
        public void custom_animation_loop(IUnit unit, AnimationGraphTag animation1, string /*id*/ emotion, bool interpolate)
        {
        }

        /// <summary>starts a custom animation playing on a unit (interpolates into animation if last parameter is TRUE)</summary>
        public void custom_animation_loop(string /*id*/ emotion, bool interpolate)
        {
        }

        /// <summary>starts a custom animation relative to some other object (interpolates into animation if last parameter is TRUE)</summary>
        public void custom_animation_relative(IUnit unit, string /*id*/ emotion, bool interpolate, IGameObject entity)
        {
        }

        /// <summary>starts a custom animation relative to some other object (interpolates into animation if last parameter is TRUE)</summary>
        public void custom_animation_relative(IUnit entity, AnimationGraphTag animation, string /*id*/ emotion, bool boolean, IGameObject other)
        {
        }

        /// <summary>starts a custom animation relative to some other object (interpolates into animation if last parameter is TRUE)</summary>
        public void custom_animation_relative_loop(IUnit unit, AnimationGraphTag animation2, string /*id*/ emotion, bool boolean, IGameObject entity)
        {
        }

        /// <summary>causes the specified damage at the specified flag.</summary>
        public void damage_new(DamageEffectTag damage, ILocationFlag cutscene_flag)
        {
        }

        /// <summary>causes the specified damage at the specified object.</summary>
        public void damage_object(DamageEffectTag damage, IGameObject entity)
        {
        }

        /// <summary>damages all players with the given damage effect</summary>
        public void damage_players(DamageEffectTag damage)
        {
        }

        /// <summary>deactivates a nav point type attached to a team anchored to a flag</summary>
        public void deactivate_team_nav_point_flag(ITeam team, ILocationFlag cutscene_flag)
        {
        }

        /// <summary>deactivates a nav point type attached to a team anchored to an object</summary>
        public void deactivate_team_nav_point_object(ITeam team)
        {
        }

        /// <summary>deactivates a nav point type attached to a team anchored to an object</summary>
        public void deactivate_team_nav_point_object(ITeam team, IGameObject entity)
        {
        }

        /// <summary>drops the named tag e.g. objects\vehicles\banshee\banshee.vehicle</summary>
        public void drop(string value)
        {
        }

        /// <summary>starts the specified effect at the specified flag.</summary>
        public void effect_new(EffectTag effect, ILocationFlag cutscene_flag)
        {
        }

        /// <summary>starts the specified effect on the specified object at the specified marker.</summary>
        public void effect_new_on_object_marker(EffectTag effect, IGameObject entity, string /*id*/ string_id)
        {
        }

        /// <summary>starts the specified effect on the specified object at the specified marker.</summary>
        public void effect_new_on_object_marker(EffectTag effect, string /*id*/ emotion)
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
        public IGameDifficulty game_difficulty_get()
        {
            return GameDifficulty.Normal();
        }

        /// <summary>returns the actual current difficulty setting without lying</summary>
        public IGameDifficulty game_difficulty_get_real()
        {
            return GameDifficulty.Easy();
        }

        /// <summary>returns TRUE if the game is cooperative</summary>
        public bool game_is_cooperative()
        {
            return false;
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
        public void kill_volume_disable(ITriggerVolume trigger_volume)
        {
            trigger_volume.KillOnEnter(false);
        }

        /// <summary>enables a kill volume</summary>
        public void kill_volume_enable(ITriggerVolume trigger_volume)
        {
            trigger_volume.KillOnEnter(true);
        }

        /// <summary>returns the number of objects in a list</summary>
        public short list_count(IGameObject e)
        {
            return (short)(e == null ? 0 : 1);
        }

        /// <summary>returns the number of objects in a list</summary>
        public short list_count(GameObjectList object_list)
        {
            return (short)object_list.Objects.Length;
        }

        /// <summary>returns the number of objects in a list that aren't dead</summary>
        public short list_count_not_dead(GameObjectList objects)
        {
            short live = 0;

            foreach (var o in objects?.Objects)
            {
                if (o.IsAlive) live++;
            }

            return live;
        }

        /// <summary>returns an item in an object list.</summary>
        public IGameObject list_get(GameObjectList object_list, int index)
        {
            if(object_list.Objects.Length > index)
                return object_list.Objects[index];

            return null;
        }

        /// <summary>starts the map from the beginning.</summary>
        public void map_reset()
        {
        }

        /// <summary>returns the maximum of all specified expressions.</summary>
        public float max(float a, float b)
        {
            return MathF.Max(a, b);
        }

        /// <summary>returns the minimum of all specified expressions.</summary>
        public float min(float a, float b)
        {
            return MathF.Min(a, b);
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
            return MathExt.Clamp(value, min, max);
        }

        /// <summary>prints a string to the console.</summary>
        public void print(string value)
        {
            Logger.Log(value, Logger.Color.Magenta);
        }

        /// <summary>returns a random value in the range [lower bound, upper bound)</summary>
        public int random_range(int value, int value1)
        {
            return this.rng.Next(value, value1);
        }

        /// <summary>returns a random value in the range [lower bound, upper bound)</summary>
        public float real_random_range(float min, float max)
        {
            return (float)(this.rng.NextDouble() * (max - min) + min);
        }

        /// <summary>starts a custom looping animation playing on a piece of scenery</summary>
        public void scenery_animation_start_loop(IScenery scenery, AnimationGraphTag animation, string /*id*/ emotion)
        {
        }

        /// <summary>starts a custom animation playing on a piece of scenery relative to a parent object</summary>
        public void scenery_animation_start_relative(IScenery scenery, string /*id*/ emotion, IGameObject entity)
        {
        }

        /// <summary>starts a custom animation playing on a piece of scenery relative to a parent object</summary>
        public void scenery_animation_start_relative(IScenery scenery, AnimationGraphTag animation, string /*id*/ emotion, IGameObject entity)
        {
        }

        /// <summary>returns the number of ticks remaining in a custom animation (or zero, if the animation is over).</summary>
        public short scenery_get_animation_time(IScenery scenery)
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
            await this.executionOrchestrator.Delay(ticks);
        }

        /// <summary>pauses execution of this script (or, optionally, another script) for the specified number of ticks.</summary>
        public async Task sleep(short ticks)
        {
            if (ticks <= 0) ticks = 1;
            await this.executionOrchestrator.Delay(ticks);
        }

        /// <summary>pauses execution of this script (or, optionally, another script) forever.</summary>
        public void sleep_forever()
        {
            this.executionOrchestrator.SetStatus(ScriptStatus.Sleeping);
        }

        /// <summary>pauses execution of this script (or, optionally, another script) forever.</summary>
        public void sleep_forever(IScriptMethodReference script)
        {
            this.executionOrchestrator.SetStatus(script.GetId(), ScriptStatus.Sleeping);
        }

        /// <summary>pauses execution of this script until the specified condition is true, checking once per second unless a different number of ticks is specified.</summary>
        public async Task sleep_until(Func<Task<bool>> condition, int ticks = ticksPerSecond, int timeout = -1)
        {
            var start = DateTimeOffset.Now;
            var timeoutOffset = DateTimeOffset.MaxValue;

            if (timeout >= 0)
            {
                timeoutOffset = start.AddSeconds(timeout / (double)TicksPerSecond);
            }

            while (await condition() == false && timeoutOffset > DateTimeOffset.Now)
            { 
                await this.executionOrchestrator.Delay(ticks);
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
        public void switch_bsp_by_name(IBsp structure_bsp)
        {
        }

        /// <summary>turns off the render texture camera</summary>
        public void texture_camera_off()
        {
        }

        /// <summary>sets the render texture camera to a given object marker</summary>
        public void texture_camera_set_object_marker(IGameObject entity, string /*id*/ string_id, float real)
        {
        }

        /// <summary>returns the driver of a vehicle</summary>
        public IGameObject vehicle_driver(IUnit unit)
        {
            return unit.Driver;
        }

        /// <summary>makes a list of units (named or by encounter) magically get into a vehicle, in the substring-specified seats (e.g. CD-passenger... empty string matches all seats)</summary>
        public void vehicle_load_magic(IGameObject vehicle, /*VehicleSeat*/ string vehicleSeat, GameObjectList units)
        {
        }

        /// <summary>makes a list of units (named or by encounter) magically get into a vehicle, in the substring-specified seats (e.g. CD-passenger... empty string matches all seats)</summary>
        public void vehicle_load_magic(IGameObject vehicle, /*VehicleSeat*/ string vehicleSeat, IGameObject unit)
        {
        }

        /// <summary>tests whether the named seat has a specified unit in it (use "" to test all seats for this unit)</summary>
        public bool vehicle_test_seat(IVehicle vehicle, string seat, IUnit unit)
        {
            return default(bool);
        }

        /// <summary>tests whether the named seat has an object in the object list (use "" to test all seats for any unit in the list)</summary>
        public bool vehicle_test_seat_list(IVehicle vehicle, string /*id*/ seat, GameObjectList subjects)
        {
            return default(bool);
        }

        /// <summary>tests whether the named seat has an object in the object list (use "" to test all seats for any unit in the list)</summary>
        public bool vehicle_test_seat_list(IVehicle vehicle, string /*id*/ seat, IGameObject subject)
        {
            return default(bool);
        }

        /// <summary>makes units get out of an object from the substring-specified seats (e.g. CD-passenger... empty string matches all seats)</summary>
        public void vehicle_unload(IGameObject entity, /*VehicleSeat*/ string unit_seat_mapping)
        {
        }

        /// <summary>returns list of objects in volume or (max 128).</summary>
        public GameObjectList volume_return_objects(ITriggerVolume trigger_volume)
        {
            var items = trigger_volume.GetObjects();

            return new GameObjectList(items);
        }

        /// <summary>returns list of objects in volume or (max 128).</summary>
        public GameObjectList volume_return_objects_by_type(ITriggerVolume trigger_volume, int value)
        {
            var items = trigger_volume.GetObjects((TypeFlags)value);

            return new GameObjectList(items);
        }

        /// <summary>moves all players outside a specified trigger volume to a specified flag.</summary>
        public void volume_teleport_players_not_inside(ITriggerVolume trigger_volume, ILocationFlag cutscene_flag)
        {
        }

        /// <summary>returns true if the specified object is within the specified volume.</summary>
        public bool volume_test_object(ITriggerVolume trigger_volume, IGameObject entity)
        {
            return trigger_volume.Contains(entity);
        }

        /// <summary>returns true if any of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        public bool volume_test_objects(ITriggerVolume trigger_volume, IGameObject entity)
        {
            return trigger_volume.Contains(entity);
        }

        /// <summary>returns true if any of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        public bool volume_test_objects(ITriggerVolume trigger_volume, IAiActorDefinition actor)
        {
            return trigger_volume.Contains(actor.Actor);
        }

        /// <summary>returns true if any of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        public bool volume_test_objects(ITriggerVolume trigger_volume, GameObjectList object_list)
        {
            foreach(var o in object_list)
            {
                if(trigger_volume.Contains(o))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>returns true if all of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        public bool volume_test_objects_all(ITriggerVolume trigger_volume, GameObjectList object_list)
        {
            var allIn = true;

            foreach (var o in object_list)
            {
                if (trigger_volume.Contains(o) == false)
                {
                    allIn = false;
                }
            }

            return allIn;
        }

        /// <summary>returns true if all of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        public bool volume_test_objects_all(ITriggerVolume trigger, IGameObject entity)
        {
            return trigger.Contains(entity);
        }


        /// <summary>returns true if all of the specified objects are within the specified volume. trigger volume must have been postprocessed</summary>
        public bool volume_test_objects_all(ITriggerVolume trigger, IAiActorDefinition actor)
        {
            return trigger.Contains(actor.Actor);
        }

        /// <summary>wakes a sleeping script in the next update.</summary>
        public void wake(IScriptMethodReference script_name)
        {
            this.executionOrchestrator.SetStatus(script_name.GetId(), ScriptStatus.RunOnce);
        }

        /// <summary>turns the trigger for a weapon  on/off</summary>
        public void weapon_enable_warthog_chaingun_light(bool boolean)
        {
        }

        /// <summary>turns the trigger for a weapon  on/off</summary>
        public void weapon_hold_trigger(IWeaponReference weapon, int triggerIndex, bool boolean)
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