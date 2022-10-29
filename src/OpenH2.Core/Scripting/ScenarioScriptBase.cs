using System;
using System.Threading.Tasks;
using OpenH2.Core.Architecture;
using OpenH2.Core.GameObjects;
using OpenH2.Core.Scripting.Execution;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Scenario;

namespace OpenH2.Core.Scripting
{
    public abstract class ScenarioScriptBase : IScriptEngine
    {
        public ScenarioTag Scenario { get; protected set; }

        public short TicksPerSecond => Engine.TicksPerSecond;

        protected IScriptEngine Engine;
        public ITeam player;
        public ITeam human;
        public ITeam prophet;
        public ITeam covenant;
        public ITeam sentinel;
        public ITeam heretic;
        public short cinematic_letterbox_style;
        public IAiActorDefinition ai_current_actor;
        public IAiActorDefinition ai_current_squad;
        public IAIBehavior guard;
        public short ai_combat_status_active;
        public short ai_combat_status_alert;
        public short ai_combat_status_idle;
        public short ai_combat_status_definite;
        public short ai_combat_status_certain;
        public short ai_combat_status_visible;
        public short ai_combat_status_clear_los;
        public short ai_combat_status_uninspected;
        public short ai_combat_status_dangerous;
        public short ai_movement_combat;
        public short ai_movement_patrol;
        public short ai_movement_flee;
        public IDamageState destroyed;
        public INavigationPoint _default;
        public INavigationPoint default_red;

        public abstract void InitializeData(ScenarioTag scenario, Scene scene);

        public void activate_team_nav_point_flag(INavigationPoint navpoint, ITeam team, ILocationFlag cutscene_flag, float real) => Engine.activate_team_nav_point_flag(navpoint, team, cutscene_flag, real);

        public void activate_team_nav_point_object(INavigationPoint navpoint, ITeam team, IGameObject entity, float real) => Engine.activate_team_nav_point_object(navpoint, team, entity, real);

        public GameObjectList ai_actors(IAiActorDefinition ai) => Engine.ai_actors(ai);

        public void ai_allegiance(ITeam team, ITeam team1) => Engine.ai_allegiance(team, team1);

        public void ai_attach_units(GameObjectList units, IAiActorDefinition ai) => Engine.ai_attach_units(units, ai);

        public void ai_attach_units(IUnit unit, IAiActorDefinition ai) => Engine.ai_attach_units(unit, ai);

        public void ai_berserk(IAiActorDefinition ai, bool boolean) => Engine.ai_berserk(ai, boolean);

        public void ai_braindead(IAiActorDefinition ai, bool boolean) => Engine.ai_braindead(ai, boolean);

        public void ai_cannot_die(IAiActorDefinition ai, bool boolean) => Engine.ai_cannot_die(ai, boolean);

        public short ai_combat_status(IAiActorDefinition ai) => Engine.ai_combat_status(ai);

        public void ai_dialogue_enable(bool boolean) => Engine.ai_dialogue_enable(boolean);

        public void ai_disposable(IAiActorDefinition ai, bool boolean) => Engine.ai_disposable(ai, boolean);

        public void ai_disregard(IGameObject unit, bool boolean) => Engine.ai_disregard(unit, boolean);

        public void ai_disregard(IAiActorDefinition actor, bool boolean) => Engine.ai_disregard(actor, boolean);

        public void ai_disregard(GameObjectList object_list, bool boolean) => Engine.ai_disregard(object_list, boolean);

        public void ai_enter_squad_vehicles(IAiActorDefinition ai) => Engine.ai_enter_squad_vehicles(ai);

        public void ai_erase(IAiActorDefinition ai) => Engine.ai_erase(ai);

        public void ai_erase_all() => Engine.ai_erase_all();

        public short ai_fighting_count(IAiActorDefinition ai) => Engine.ai_fighting_count(ai);

        public IGameObject ai_get_object(IAiActorDefinition ai) => Engine.ai_get_object(ai);

        public IUnit ai_get_unit(IAiActorDefinition ai) => Engine.ai_get_unit(ai);

        public void ai_kill(IAiActorDefinition ai) => Engine.ai_kill(ai);

        public void ai_kill_silent(IAiActorDefinition ai) => Engine.ai_kill_silent(ai);

        public short ai_living_count(IAiActorDefinition ai) => Engine.ai_living_count(ai);

        public void ai_magically_see(IAiActorDefinition ai, IAiActorDefinition ai1) => Engine.ai_magically_see(ai, ai1);

        public void ai_magically_see_object(IAiActorDefinition ai, IGameObject value) => Engine.ai_magically_see_object(ai, value);

        public void ai_migrate(IAiActorDefinition ai, IAiActorDefinition ai1) => Engine.ai_migrate(ai, ai1);

        public short ai_nonswarm_count(IAiActorDefinition ai) => Engine.ai_nonswarm_count(ai);

        public void ai_overcomes_oversteer(IAiActorDefinition ai, bool boolean) => Engine.ai_overcomes_oversteer(ai, boolean);

        public void ai_place(IAiActorDefinition ai) => Engine.ai_place(ai);

        public void ai_place(IAiActorDefinition ai, int value) => Engine.ai_place(ai, value);

        public void ai_place_in_vehicle(IAiActorDefinition ai, IAiActorDefinition ai1) => Engine.ai_place_in_vehicle(ai, ai1);

        public short ai_play_line(IAiActorDefinition ai, string string_id) => Engine.ai_play_line(ai, string_id);

        public short ai_play_line_at_player(IAiActorDefinition ai, string string_id) => Engine.ai_play_line_at_player(ai, string_id);

        public short ai_play_line_on_object(IGameObject entity, string string_id) => Engine.ai_play_line_on_object(entity, string_id);

        public void ai_prefer_target(GameObjectList units, bool boolean) => Engine.ai_prefer_target(units, boolean);

        public void ai_renew(IAiActorDefinition ai) => Engine.ai_renew(ai);

        public bool ai_scene(string scene_name, IScriptMethodReference ai_command_script, IAiActorDefinition ai) => Engine.ai_scene(scene_name, ai_command_script, ai);

        public bool ai_scene(string scene_name, IScriptMethodReference ai_command_script, IAiActorDefinition ai, IAiActorDefinition ai1) => Engine.ai_scene(scene_name, ai_command_script, ai, ai1);

        public void ai_set_active_camo(IAiActorDefinition ai, bool boolean) => Engine.ai_set_active_camo(ai, boolean);

        public void ai_set_blind(IAiActorDefinition ai, bool boolean) => Engine.ai_set_blind(ai, boolean);

        public void ai_set_deaf(IAiActorDefinition ai, bool boolean) => Engine.ai_set_deaf(ai, boolean);

        public void ai_set_orders(IAiActorDefinition ai, IAiOrders ai_orders) => Engine.ai_set_orders(ai, ai_orders);

        public short ai_spawn_count(IAiActorDefinition ai) => Engine.ai_spawn_count(ai);

        public float ai_strength(IAiActorDefinition ai) => Engine.ai_strength(ai);

        public void ai_suppress_combat(IAiActorDefinition ai, bool boolean) => Engine.ai_suppress_combat(ai, boolean);

        public short ai_swarm_count(IAiActorDefinition ai) => Engine.ai_swarm_count(ai);

        public void ai_teleport_to_starting_location_if_outside_bsp(IAiActorDefinition ai) => Engine.ai_teleport_to_starting_location_if_outside_bsp(ai);

        public bool ai_trigger_test(string value, IAiActorDefinition ai) => Engine.ai_trigger_test(value, ai);

        public void ai_vehicle_enter(IAiActorDefinition ai, string unit) => Engine.ai_vehicle_enter(ai, unit);

        public void ai_vehicle_enter(IAiActorDefinition ai, IUnit unit, string unit_seat_mapping = null) => Engine.ai_vehicle_enter(ai, unit, unit_seat_mapping);

        public void ai_vehicle_enter_immediate(IAiActorDefinition ai, IUnit unit, string seat = null) => Engine.ai_vehicle_enter_immediate(ai, unit, seat);

        public void ai_vehicle_exit(IAiActorDefinition ai) => Engine.ai_vehicle_exit(ai);

        public IVehicle ai_vehicle_get(IAiActorDefinition ai) => Engine.ai_vehicle_get(ai);

        public IVehicle ai_vehicle_get_from_starting_location(IAiActorDefinition ai) => Engine.ai_vehicle_get_from_starting_location(ai);

        public void ai_vehicle_reserve(IVehicle vehicle, bool boolean) => Engine.ai_vehicle_reserve(vehicle, boolean);

        public void ai_vehicle_reserve_seat(IVehicle vehicle, string string_id, bool boolean) => Engine.ai_vehicle_reserve_seat(vehicle, string_id, boolean);

        public bool ai_vitality_pinned(IAiActorDefinition ai) => Engine.ai_vitality_pinned(ai);

        public void begin_random(params Func<Task>[] expressions) => Engine.begin_random(expressions);

        public bool bink_done() => Engine.bink_done();

        public void biped_ragdoll(IUnit unit) => Engine.biped_ragdoll(unit);

        public Task cache_block_for_one_frame() => Engine.cache_block_for_one_frame();

        public void camera_control(bool boolean) => Engine.camera_control(boolean);

        public void camera_predict_resources_at_frame(AnimationGraphTag animation, string emotion, IUnit unit, ILocationFlag locationFlag, int intValue) => Engine.camera_predict_resources_at_frame(animation, emotion, unit, locationFlag, intValue);

        public void camera_predict_resources_at_point(ICameraPathTarget cutscene_camera_point) => Engine.camera_predict_resources_at_point(cutscene_camera_point);

        public void camera_set(ICameraPathTarget cutscene_camera_point, short value) => Engine.camera_set(cutscene_camera_point, value);

        public void camera_set_animation_relative(AnimationGraphTag animation, string id, IUnit unit, ILocationFlag locationFlag) => Engine.camera_set_animation_relative(animation, id, unit, locationFlag);

        public void camera_set_field_of_view(float degrees, short ticks) => Engine.camera_set_field_of_view(degrees, ticks);

        public short camera_time() => Engine.camera_time();

        public void cheat_active_camouflage_by_player(short value, bool boolean) => Engine.cheat_active_camouflage_by_player(value, boolean);

        public void cinematic_clone_players_weapon(IGameObject entity, string string_id, string string_id1) => Engine.cinematic_clone_players_weapon(entity, string_id, string_id1);

        public void cinematic_enable_ambience_details(bool boolean) => Engine.cinematic_enable_ambience_details(boolean);

        public void cinematic_lighting_set_ambient_light(float real, float real1, float real12) => Engine.cinematic_lighting_set_ambient_light(real, real1, real12);

        public void cinematic_lighting_set_primary_light(float real, float real1, float real12, float real123, float real1234) => Engine.cinematic_lighting_set_primary_light(real, real1, real12, real123, real1234);

        public void cinematic_lighting_set_secondary_light(float real, float real1, float real12, float real123, float real1234) => Engine.cinematic_lighting_set_secondary_light(real, real1, real12, real123, real1234);

        public void cinematic_lightmap_shadow_disable() => Engine.cinematic_lightmap_shadow_disable();

        public void cinematic_lightmap_shadow_enable() => Engine.cinematic_lightmap_shadow_enable();

        public void cinematic_outro_start() => Engine.cinematic_outro_start();

        public void cinematic_screen_effect_set_crossfade(float real) => Engine.cinematic_screen_effect_set_crossfade(real);

        public void cinematic_screen_effect_set_depth_of_field(float real, float real1, float real12, float real123, float real1234, float real12345, float real123456) => Engine.cinematic_screen_effect_set_depth_of_field(real, real1, real12, real123, real1234, real12345, real123456);

        public void cinematic_screen_effect_start(bool boolean) => Engine.cinematic_screen_effect_start(boolean);

        public void cinematic_screen_effect_stop() => Engine.cinematic_screen_effect_stop();

        public void cinematic_set_far_clip_distance(float real) => Engine.cinematic_set_far_clip_distance(real);

        public void cinematic_set_near_clip_distance(float real) => Engine.cinematic_set_near_clip_distance(real);

        public void cinematic_set_title(ICinematicTitle cutscene_title) => Engine.cinematic_set_title(cutscene_title);

        public void cinematic_show_letterbox(bool boolean) => Engine.cinematic_show_letterbox(boolean);

        public void cinematic_show_letterbox_immediate(bool boolean) => Engine.cinematic_show_letterbox_immediate(boolean);

        public void cinematic_skip_start_internal() => Engine.cinematic_skip_start_internal();

        public void cinematic_skip_stop_internal() => Engine.cinematic_skip_stop_internal();

        public void cinematic_start() => Engine.cinematic_start();

        public void cinematic_stop() => Engine.cinematic_stop();

        public void cinematic_subtitle(string string_id, float real) => Engine.cinematic_subtitle(string_id, real);

        public void cinematic_start_movie(string name) => Engine.cinematic_start_movie(name);

        public bool controller_get_look_invert() => Engine.controller_get_look_invert();

        public void controller_set_look_invert(bool boolean) => Engine.controller_set_look_invert(boolean);

        public void cs_abort_on_alert(bool boolean) => Engine.cs_abort_on_alert(boolean);

        public void cs_abort_on_combat_status(short value) => Engine.cs_abort_on_combat_status(value);

        public void cs_abort_on_damage(bool boolean) => Engine.cs_abort_on_damage(boolean);

        public void cs_aim(bool boolean, ISpatialPoint point) => Engine.cs_aim(boolean, point);

        public void cs_aim_object(bool boolean, IGameObject entity) => Engine.cs_aim_object(boolean, entity);

        public void cs_aim_player(bool boolean) => Engine.cs_aim_player(boolean);

        public void cs_approach(IGameObject entity, float real, float real1, float real12) => Engine.cs_approach(entity, real, real1, real12);

        public void cs_approach_player(float real, float real1, float real12) => Engine.cs_approach_player(real, real1, real12);

        public void cs_approach_stop() => Engine.cs_approach_stop();

        public bool cs_command_script_queued(IAiActorDefinition ai, IScriptMethodReference ai_command_script) => Engine.cs_command_script_queued(ai, ai_command_script);

        public bool cs_command_script_running(IAiActorDefinition ai, IScriptMethodReference ai_command_script) => Engine.cs_command_script_running(ai, ai_command_script);

        public void cs_crouch(bool boolean) => Engine.cs_crouch(boolean);

        public void cs_custom_animation(AnimationGraphTag animation, string emotion, float floatValue, bool interpolate) => Engine.cs_custom_animation(animation, emotion, floatValue, interpolate);

        public void cs_deploy_turret(ISpatialPoint point) => Engine.cs_deploy_turret(point);

        public void cs_enable_dialogue(bool boolean) => Engine.cs_enable_dialogue(boolean);

        public void cs_enable_looking(bool boolean) => Engine.cs_enable_looking(boolean);

        public void cs_enable_moving(bool boolean) => Engine.cs_enable_moving(boolean);

        public void cs_enable_pathfinding_failsafe(bool boolean) => Engine.cs_enable_pathfinding_failsafe(boolean);

        public void cs_enable_targeting(bool boolean) => Engine.cs_enable_targeting(boolean);

        public void cs_face(bool boolean, ISpatialPoint point = null) => Engine.cs_face(boolean, point);

        public void cs_face_object(bool boolean, IGameObject entity) => Engine.cs_face_object(boolean, entity);

        public void cs_face_player(bool boolean) => Engine.cs_face_player(boolean);

        public void cs_fly_by(ISpatialPoint point) => Engine.cs_fly_by(point);

        public void cs_fly_by(ISpatialPoint point, float tolerance) => Engine.cs_fly_by(point, tolerance);

        public void cs_fly_to(ISpatialPoint point) => Engine.cs_fly_to(point);

        public void cs_fly_to(ISpatialPoint point, float tolerance) => Engine.cs_fly_to(point, tolerance);

        public void cs_fly_to_and_face(ISpatialPoint point, ISpatialPoint face) => Engine.cs_fly_to_and_face(point, face);

        public void cs_fly_to_and_face(ISpatialPoint point, ISpatialPoint face, float tolerance) => Engine.cs_fly_to_and_face(point, face, tolerance);

        public void cs_force_combat_status(short value) => Engine.cs_force_combat_status(value);

        public void cs_go_by(ISpatialPoint point, ISpatialPoint planeP, float planeD = 0) => Engine.cs_go_by(point, planeP, planeD);

        public void cs_go_to(ISpatialPoint point) => Engine.cs_go_to(point);

        public void cs_go_to(ISpatialPoint point, float tolerance) => Engine.cs_go_to(point, tolerance);

        public void cs_go_to_and_face(ISpatialPoint point, ISpatialPoint faceTowards) => Engine.cs_go_to_and_face(point, faceTowards);

        public void cs_go_to_nearest(ISpatialPoint destination) => Engine.cs_go_to_nearest(destination);

        public void cs_go_to_vehicle(IVehicle vehicle) => Engine.cs_go_to_vehicle(vehicle);

        public void cs_grenade(ISpatialPoint point, int action) => Engine.cs_grenade(point, action);

        public void cs_ignore_obstacles(bool boolean) => Engine.cs_ignore_obstacles(boolean);

        public void cs_jump(float real, float real1) => Engine.cs_jump(real, real1);

        public void cs_jump_to_point(float real, float real1) => Engine.cs_jump_to_point(real, real1);

        public void cs_look(bool boolean, ISpatialPoint point = null) => Engine.cs_look(boolean, point);

        public void cs_look_object(bool boolean, IGameObject entity) => Engine.cs_look_object(boolean, entity);

        public void cs_look_player(bool boolean) => Engine.cs_look_player(boolean);

        public void cs_movement_mode(short value) => Engine.cs_movement_mode(value);

        public void cs_move_in_direction(float real, float real1, float real12) => Engine.cs_move_in_direction(real, real1, real12);

        public bool cs_moving() => Engine.cs_moving();

        public void cs_pause(float real) => Engine.cs_pause(real);

        public void cs_play_line(string string_id) => Engine.cs_play_line(string_id);

        public void cs_queue_command_script(IAiActorDefinition ai, IScriptMethodReference ai_command_script) => Engine.cs_queue_command_script(ai, ai_command_script);

        public void cs_run_command_script(IAiActorDefinition ai, IScriptMethodReference ai_command_script) => Engine.cs_run_command_script(ai, ai_command_script);

        public void cs_set_behavior(IAIBehavior ai_behavior) => Engine.cs_set_behavior(ai_behavior);

        public void cs_shoot(bool boolean) => Engine.cs_shoot(boolean);

        public void cs_shoot(bool boolean, IGameObject entity) => Engine.cs_shoot(boolean, entity);

        public void cs_shoot_point(bool boolean, ISpatialPoint point) => Engine.cs_shoot_point(boolean, point);

        public void cs_stack_command_script(IAiActorDefinition ai, IScriptMethodReference ai_command_script) => Engine.cs_stack_command_script(ai, ai_command_script);

        public void cs_start_approach(IGameObject entity, float real, float real1, float real12) => Engine.cs_start_approach(entity, real, real1, real12);

        public void cs_start_approach_player(float real, float real1, float real12) => Engine.cs_start_approach_player(real, real1, real12);

        public void cs_start_to(ISpatialPoint destination) => Engine.cs_start_to(destination);

        public void cs_stop_custom_animation() => Engine.cs_stop_custom_animation();

        public void cs_suppress_dialogue_global(bool boolean) => Engine.cs_suppress_dialogue_global(boolean);

        public void cs_switch(string string_id) => Engine.cs_switch(string_id);

        public void cs_teleport(ISpatialPoint destination, ISpatialPoint facing) => Engine.cs_teleport(destination, facing);

        public void cs_turn_sharpness(bool boolean, float real) => Engine.cs_turn_sharpness(boolean, real);

        public void cs_vehicle_boost(bool boolean) => Engine.cs_vehicle_boost(boolean);

        public void cs_vehicle_speed(float real) => Engine.cs_vehicle_speed(real);

        public void custom_animation(IUnit unit, AnimationGraphTag animation, string animationName, bool interpolate) => Engine.custom_animation(unit, animation, animationName, interpolate);

        public void custom_animation_loop(IUnit unit, AnimationGraphTag animation, string animationName, bool interpolate) => Engine.custom_animation_loop(unit, animation, animationName, interpolate);

        public void custom_animation_relative(IUnit entity, AnimationGraphTag animation, string animationName, bool boolean, IGameObject other) => Engine.custom_animation_relative(entity, animation, animationName, boolean, other);

        public void custom_animation_relative_loop(IUnit unit, AnimationGraphTag animation, string animationName, bool boolean, IGameObject entity) => Engine.custom_animation_relative_loop(unit, animation, animationName, boolean, entity);

        public void damage_new(DamageEffectTag damage, ILocationFlag cutscene_flag) => Engine.damage_new(damage, cutscene_flag);

        public void damage_object(DamageEffectTag damage, IGameObject entity) => Engine.damage_object(damage, entity);

        public void damage_players(DamageEffectTag damage) => Engine.damage_players(damage);

        public void data_mine_set_mission_segment(string value) => Engine.data_mine_set_mission_segment(value);

        public void deactivate_team_nav_point_flag(ITeam team, ILocationFlag cutscene_flag) => Engine.deactivate_team_nav_point_flag(team, cutscene_flag);

        public void deactivate_team_nav_point_object(ITeam team, IGameObject entity) => Engine.deactivate_team_nav_point_object(team, entity);

        public void device_animate_overlay(IDevice device, float real, float real1, float real12, float real123) => Engine.device_animate_overlay(device, real, real1, real12, real123);

        public void device_animate_position(IDevice device, float desiredPosition, float seconds, float real12, float real123, bool boolean) => Engine.device_animate_position(device, desiredPosition, seconds, real12, real123, boolean);

        public void device_closes_automatically_set(IDevice device, bool boolean) => Engine.device_closes_automatically_set(device, boolean);

        public float device_get_position(IDevice device) => Engine.device_get_position(device);

        public void device_group_change_only_once_more_set(IDeviceGroup device_group, bool boolean) => Engine.device_group_change_only_once_more_set(device_group, boolean);

        public float device_group_get(IDeviceGroup device_group) => Engine.device_group_get(device_group);

        public void device_group_set(IDevice device, IDeviceGroup device_group, float real) => Engine.device_group_set(device, device_group, real);

        public void device_group_set_immediate(IDeviceGroup device_group, float real) => Engine.device_group_set_immediate(device_group, real);

        public void device_one_sided_set(IDevice device, bool boolean) => Engine.device_one_sided_set(device, boolean);

        public void device_operates_automatically_set(IDevice device, bool boolean) => Engine.device_operates_automatically_set(device, boolean);

        public void device_set_never_appears_locked(IDevice device, bool boolean) => Engine.device_set_never_appears_locked(device, boolean);

        public void device_set_overlay_track(IDevice device, string string_id) => Engine.device_set_overlay_track(device, string_id);

        public void device_set_position(IDevice device, float real) => Engine.device_set_position(device, real);

        public void device_set_position_immediate(IDevice device, float real) => Engine.device_set_position_immediate(device, real);

        public void device_set_position_track(IDevice device, string string_id, float real) => Engine.device_set_position_track(device, string_id, real);

        public void device_set_power(IDevice device, float real) => Engine.device_set_power(device, real);

        public void disable_render_light_suppressor() => Engine.disable_render_light_suppressor();

        public void drop(string value) => Engine.drop(value);

        public void effect_new(EffectTag effect, ILocationFlag cutscene_flag) => Engine.effect_new(effect, cutscene_flag);

        public void effect_new_on_object_marker(EffectTag effect, IGameObject entity, string string_id) => Engine.effect_new_on_object_marker(effect, entity, string_id);

        public void enable_render_light_suppressor() => Engine.enable_render_light_suppressor();

        public void fade_in(float real, float real1, float real12, short value) => Engine.fade_in(real, real1, real12, value);

        public void fade_out(float real, float real1, float real12, short value) => Engine.fade_out(real, real1, real12, value);

        public void flock_start(string string_id) => Engine.flock_start(string_id);

        public void flock_stop(string string_id) => Engine.flock_stop(string_id);

        public void game_can_use_flashlights(bool boolean) => Engine.game_can_use_flashlights(boolean);

        public IGameDifficulty game_difficulty_get() => Engine.game_difficulty_get();

        public IGameDifficulty game_difficulty_get_real() => Engine.game_difficulty_get_real();

        public bool game_is_cooperative() => Engine.game_is_cooperative();

        public bool game_is_playtest() => Engine.game_is_playtest();

        public void game_revert() => Engine.game_revert();

        public bool game_reverted() => Engine.game_reverted();

        public bool game_safe_to_save() => Engine.game_safe_to_save();

        public void game_save() => Engine.game_save();

        public void game_save_cancel() => Engine.game_save_cancel();

        public void game_save_cinematic_skip() => Engine.game_save_cinematic_skip();

        public void game_save_immediate() => Engine.game_save_immediate();

        public void game_save_no_timeout() => Engine.game_save_no_timeout();

        public bool game_saving() => Engine.game_saving();

        public void game_won() => Engine.game_won();

        public void garbage_collect_now() => Engine.garbage_collect_now();

        public void garbage_collect_unsafe() => Engine.garbage_collect_unsafe();

        public void geometry_cache_flush() => Engine.geometry_cache_flush();

        public T GetReference<T>(string reference) => Engine.GetReference<T>(reference);

        public T GetTag<T>(string? name, uint id) where T : BaseTag => Engine.GetTag<T>(name, id);

        public void hud_cinematic_fade(float real, float real1) => Engine.hud_cinematic_fade(real, real1);

        public void hud_enable_training(bool boolean) => Engine.hud_enable_training(boolean);

        public void hud_set_training_text(string string_id) => Engine.hud_set_training_text(string_id);

        public void hud_show_training_text(bool boolean) => Engine.hud_show_training_text(boolean);

        public bool ice_cream_flavor_available(int value) => Engine.ice_cream_flavor_available(value);

        public void ice_cream_flavor_stock(int value) => Engine.ice_cream_flavor_stock(value);

        public void interpolator_start(string string_id, float real, float real1) => Engine.interpolator_start(string_id, real, real1);

        public void kill_volume_disable(ITriggerVolume trigger_volume) => Engine.kill_volume_disable(trigger_volume);

        public void kill_volume_enable(ITriggerVolume trigger_volume) => Engine.kill_volume_enable(trigger_volume);

        public short list_count(IGameObject e) => Engine.list_count(e);

        public short list_count(GameObjectList object_list) => Engine.list_count(object_list);

        public short list_count_not_dead(GameObjectList objects) => Engine.list_count_not_dead(objects);

        public IGameObject list_get(GameObjectList object_list, int index) => Engine.list_get(object_list, index);

        public void loading_screen_fade_to_white() => Engine.loading_screen_fade_to_white();

        public void map_reset() => Engine.map_reset();

        public float max(float a, float b) => Engine.max(a, b);

        public float min(float a, float b) => Engine.min(a, b);

        public void objectives_clear() => Engine.objectives_clear();

        public void objectives_finish_up_to(int value) => Engine.objectives_finish_up_to(value);

        public void objectives_show_up_to(int value) => Engine.objectives_show_up_to(value);

        public void objects_attach(IGameObject entity, string string_id, IGameObject entity1, string string_id1) => Engine.objects_attach(entity, string_id, entity1, string_id1);

        public bool objects_can_see_flag(GameObjectList list, ILocationFlag locationFlag, float floatValue) => Engine.objects_can_see_flag(list, locationFlag, floatValue);

        public bool objects_can_see_object(IGameObject entity, IGameObject target, float degrees) => Engine.objects_can_see_object(entity, target, degrees);

        public bool objects_can_see_object(GameObjectList list, IGameObject target, float degrees) => Engine.objects_can_see_object(list, target, degrees);

        public void objects_detach(IGameObject entity, IGameObject entity1) => Engine.objects_detach(entity, entity1);

        public float objects_distance_to_flag(IGameObject entity, ILocationFlag locationFlag) => Engine.objects_distance_to_flag(entity, locationFlag);

        public float objects_distance_to_flag(GameObjectList list, ILocationFlag locationFlag) => Engine.objects_distance_to_flag(list, locationFlag);

        public float objects_distance_to_object(GameObjectList list, IGameObject entity) => Engine.objects_distance_to_object(list, entity);

        public void objects_predict(IGameObject entity) => Engine.objects_predict(entity);

        public void objects_predict(GameObjectList object_list) => Engine.objects_predict(object_list);

        public void objects_predict_high(GameObjectList entity) => Engine.objects_predict_high(entity);

        public void objects_predict_high(IGameObject entity) => Engine.objects_predict_high(entity);

        public IGameObject object_at_marker(IGameObject entity, string stringId) => Engine.object_at_marker(entity, stringId);

        public void object_cannot_die(IGameObject entity, bool boolean) => Engine.object_cannot_die(entity, boolean);

        public void object_cannot_take_damage(IGameObject entity) => Engine.object_cannot_take_damage(entity);

        public void object_cannot_take_damage(GameObjectList object_list) => Engine.object_cannot_take_damage(object_list);

        public void object_can_take_damage(IGameObject entity) => Engine.object_can_take_damage(entity);

        public void object_can_take_damage(GameObjectList object_list) => Engine.object_can_take_damage(object_list);

        public void object_cinematic_lod(IGameObject entity, bool boolean) => Engine.object_cinematic_lod(entity, boolean);

        public void object_cinematic_visibility(IGameObject entity, bool boolean) => Engine.object_cinematic_visibility(entity, boolean);

        public void object_clear_all_function_variables(IGameObject entity) => Engine.object_clear_all_function_variables(entity);

        public void object_clear_function_variable(IGameObject entity, string string_id) => Engine.object_clear_function_variable(entity, string_id);

        public void object_create(IEntityIdentifier object_name) => Engine.object_create(object_name);

        public void object_create_anew(IEntityIdentifier object_name) => Engine.object_create_anew(object_name);

        public void object_create_anew_containing(string value) => Engine.object_create_anew_containing(value);

        public void object_create_clone(IEntityIdentifier object_name) => Engine.object_create_clone(object_name);

        public void object_create_containing(string value) => Engine.object_create_containing(value);

        public void object_damage_damage_section(IGameObject entity, string string_id, float real) => Engine.object_damage_damage_section(entity, string_id, real);

        public void object_destroy(IGameObject entity) => Engine.object_destroy(entity);

        public void object_destroy_containing(string value) => Engine.object_destroy_containing(value);

        public void object_destroy_type_mask(int value) => Engine.object_destroy_type_mask(value);

        public void object_dynamic_simulation_disable(IGameObject entity, bool boolean) => Engine.object_dynamic_simulation_disable(entity, boolean);

        public IAiActorDefinition object_get_ai(IGameObject entity) => Engine.object_get_ai(entity);

        public float object_get_health(IGameObject entity) => Engine.object_get_health(entity);

        public IGameObject object_get_parent(IGameObject entity) => Engine.object_get_parent(entity);

        public float object_get_shield(IGameObject entity) => Engine.object_get_shield(entity);

        public void object_hide(IGameObject entity, bool boolean) => Engine.object_hide(entity, boolean);

        public short object_model_targets_destroyed(IGameObject entity, string target) => Engine.object_model_targets_destroyed(entity, target);

        public void object_set_deleted_when_deactivated(IGameObject entity) => Engine.object_set_deleted_when_deactivated(entity);

        public void object_set_function_variable(IGameObject entity, string string_id, float real, float real1) => Engine.object_set_function_variable(entity, string_id, real, real1);

        public void object_set_permutation(IGameObject entity, string string_id, string string_id1) => Engine.object_set_permutation(entity, string_id, string_id1);

        public void object_set_phantom_power(IGameObject entity, bool boolean) => Engine.object_set_phantom_power(entity, boolean);

        public void object_set_region_state(IGameObject entity, string string_id, IDamageState model_state) => Engine.object_set_region_state(entity, string_id, model_state);

        public void object_set_scale(IGameObject entity, float real, short value) => Engine.object_set_scale(entity, real, value);

        public void object_set_shield(IGameObject entity, float real) => Engine.object_set_shield(entity, real);

        public void object_set_shield_stun_infinite(IGameObject entity) => Engine.object_set_shield_stun_infinite(entity);

        public void object_set_velocity(IGameObject entity, float real) => Engine.object_set_velocity(entity, real);

        public void object_set_velocity(IGameObject entity, float real, float real1, float real12) => Engine.object_set_velocity(entity, real, real1, real12);

        public void object_teleport(IGameObject entity, ILocationFlag cutscene_flag) => Engine.object_teleport(entity, cutscene_flag);

        public void object_type_predict(BaseTag entity) => Engine.object_type_predict(entity);

        public void object_type_predict_high(BaseTag entity) => Engine.object_type_predict_high(entity);

        public void object_uses_cinematic_lighting(IGameObject entity, bool boolean) => Engine.object_uses_cinematic_lighting(entity, boolean);

        public void physics_disable_character_ground_adhesion_forces(float real) => Engine.physics_disable_character_ground_adhesion_forces(real);

        public void physics_set_gravity(float real) => Engine.physics_set_gravity(real);

        public void physics_set_velocity_frame(float real, float real1, float real12) => Engine.physics_set_velocity_frame(real, real1, real12);

        public float pin(float value, float min, float max) => Engine.pin(value, min, max);

        public bool player0_looking_down() => Engine.player0_looking_down();

        public bool player0_looking_up() => Engine.player0_looking_up();

        public GameObjectList players() => Engine.players();

        public bool player_action_test_accept() => Engine.player_action_test_accept();

        public bool player_action_test_cancel() => Engine.player_action_test_cancel();

        public bool player_action_test_grenade_trigger() => Engine.player_action_test_grenade_trigger();

        public bool player_action_test_lookstick_backward() => Engine.player_action_test_lookstick_backward();

        public bool player_action_test_lookstick_forward() => Engine.player_action_test_lookstick_forward();

        public void player_action_test_look_down_begin() => Engine.player_action_test_look_down_begin();

        public void player_action_test_look_pitch_end() => Engine.player_action_test_look_pitch_end();

        public void player_action_test_look_up_begin() => Engine.player_action_test_look_up_begin();

        public bool player_action_test_melee() => Engine.player_action_test_melee();

        public bool player_action_test_primary_trigger() => Engine.player_action_test_primary_trigger();

        public void player_action_test_reset() => Engine.player_action_test_reset();

        public bool player_action_test_vision_trigger() => Engine.player_action_test_vision_trigger();

        public void player_camera_control(bool boolean) => Engine.player_camera_control(boolean);

        public void player_disable_movement(bool boolean) => Engine.player_disable_movement(boolean);

        public void player_effect_set_max_rotation(float real, float real1, float real12) => Engine.player_effect_set_max_rotation(real, real1, real12);

        public void player_effect_set_max_vibration(float real, float real1) => Engine.player_effect_set_max_vibration(real, real1);

        public void player_effect_start(float real, float real1) => Engine.player_effect_start(real, real1);

        public void player_effect_stop(float real) => Engine.player_effect_stop(real);

        public void player_enable_input(bool boolean) => Engine.player_enable_input(boolean);

        public bool player_flashlight_on() => Engine.player_flashlight_on();

        public void player_training_activate_flashlight() => Engine.player_training_activate_flashlight();

        public void player_training_activate_stealth() => Engine.player_training_activate_stealth();

        public void play_credits() => Engine.play_credits();

        public void predict_model_section(RenderModelTag render_model, int value) => Engine.predict_model_section(render_model, value);

        public void predict_structure_section(IBsp structure_bsp, int value, bool boolean) => Engine.predict_structure_section(structure_bsp, value, boolean);

        public void print(string value) => Engine.print(value);

        public void pvs_clear() => Engine.pvs_clear();

        public void pvs_set_object(IGameObject entity) => Engine.pvs_set_object(entity);

        public int random_range(int value, int value1) => Engine.random_range(value, value1);

        public void rasterizer_bloom_override(bool boolean) => Engine.rasterizer_bloom_override(boolean);

        public void rasterizer_bloom_override_brightness(float real) => Engine.rasterizer_bloom_override_brightness(real);

        public void rasterizer_bloom_override_threshold(float real) => Engine.rasterizer_bloom_override_threshold(real);

        public float real_random_range(float real, float real1) => Engine.real_random_range(real, real1);

        public void render_lights_enable_cinematic_shadow(bool boolean, IGameObject entity, string string_id, float real) => Engine.render_lights_enable_cinematic_shadow(boolean, entity, string_id, real);

        public void scenery_animation_start_loop(IScenery scenery, AnimationGraphTag animation, string emotion) => Engine.scenery_animation_start_loop(scenery, animation, emotion);

        public void scenery_animation_start_relative(IScenery scenery, AnimationGraphTag animation, string emotion, IGameObject entity) => Engine.scenery_animation_start_relative(scenery, animation, emotion, entity);

        public short scenery_get_animation_time(IScenery scenery) => Engine.scenery_get_animation_time(scenery);

        public void set_global_sound_environment(float real, float real1, float real12, float real123, int value, float real1234) => Engine.set_global_sound_environment(real, real1, real12, real123, value, real1234);

        public Task sleep(int ticks) => Engine.sleep(ticks);

        public Task sleep(short ticks, IScriptMethodReference script = null) => Engine.sleep(ticks, script);

        public void sleep_forever(IScriptMethodReference script = null) => Engine.sleep_forever(script);

        public Task sleep_until(Func<Task<bool>> condition, int ticks = 60, int timeout = -1) => Engine.sleep_until(condition, ticks, timeout);

        public void sound_class_set_gain(string value, float gain, int ticks) => Engine.sound_class_set_gain(value, gain, ticks);

        public int sound_impulse_language_time(SoundTag soundRef) => Engine.sound_impulse_language_time(soundRef);

        public void sound_impulse_predict(SoundTag soundRef) => Engine.sound_impulse_predict(soundRef);

        public void sound_impulse_start(SoundTag sound, IGameObject entity, float floatValue) => Engine.sound_impulse_start(sound, entity, floatValue);

        public void sound_impulse_start_effect(SoundTag sound, IGameObject entity, float floatValue, string effect) => Engine.sound_impulse_start_effect(sound, entity, floatValue, effect);

        public void sound_impulse_stop(SoundTag sound) => Engine.sound_impulse_stop(sound);

        public void sound_impulse_trigger(SoundTag sound, IGameObject source, float floatValue, int intValue) => Engine.sound_impulse_trigger(sound, source, floatValue, intValue);

        public void sound_looping_set_alternate(LoopingSoundTag looping_sound, bool boolean) => Engine.sound_looping_set_alternate(looping_sound, boolean);

        public void sound_looping_start(LoopingSoundTag looping_sound, IGameObject entity, float real) => Engine.sound_looping_start(looping_sound, entity, real);

        public void sound_looping_stop(LoopingSoundTag looping_sound) => Engine.sound_looping_stop(looping_sound);

        public void sound_suppress_ambience_update_on_revert() => Engine.sound_suppress_ambience_update_on_revert();

        public short structure_bsp_index() => Engine.structure_bsp_index();

        public void switch_bsp(short value) => Engine.switch_bsp(value);

        public void switch_bsp_by_name(IBsp structure_bsp) => Engine.switch_bsp_by_name(structure_bsp);

        public void texture_cache_flush() => Engine.texture_cache_flush();

        public void texture_camera_off() => Engine.texture_camera_off();

        public void texture_camera_set_object_marker(IGameObject entity, string string_id, float real) => Engine.texture_camera_set_object_marker(entity, string_id, real);

        public void time_code_reset() => Engine.time_code_reset();

        public void time_code_show(bool boolean) => Engine.time_code_show(boolean);

        public void time_code_start(bool boolean) => Engine.time_code_start(boolean);

        public IUnit unit(IGameObject entity) => Engine.unit(entity);

        public void units_set_current_vitality(GameObjectList units, float body, float shield) => Engine.units_set_current_vitality(units, body, shield);

        public void units_set_maximum_vitality(GameObjectList units, float body, float shield) => Engine.units_set_maximum_vitality(units, body, shield);

        public void unit_add_equipment(IUnit unit, IStartingProfile starting_profile, bool reset, bool isGarbage) => Engine.unit_add_equipment(unit, starting_profile, reset, isGarbage);

        public void unit_doesnt_drop_items(GameObjectList entities) => Engine.unit_doesnt_drop_items(entities);

        public void unit_exit_vehicle(IUnit unit, short value) => Engine.unit_exit_vehicle(unit, value);

        public short unit_get_custom_animation_time(IUnit unit) => Engine.unit_get_custom_animation_time(unit);

        public float unit_get_health(IUnit unit) => Engine.unit_get_health(unit);

        public float unit_get_shield(IUnit unit) => Engine.unit_get_shield(unit);

        public bool unit_has_weapon(IUnit unit, BaseTag weapon) => Engine.unit_has_weapon(unit, weapon);

        public void unit_impervious(IGameObject unit, bool boolean) => Engine.unit_impervious(unit, boolean);

        public void unit_impervious(IAiActorDefinition actor, bool boolean) => Engine.unit_impervious(actor, boolean);

        public void unit_impervious(GameObjectList object_list, bool boolean) => Engine.unit_impervious(object_list, boolean);

        public bool unit_in_vehicle(IUnit unit) => Engine.unit_in_vehicle(unit);

        public bool unit_is_emitting(IUnit unit) => Engine.unit_is_emitting(unit);

        public void unit_kill(IUnit unit) => Engine.unit_kill(unit);

        public void unit_kill_silent(IUnit unit) => Engine.unit_kill_silent(unit);

        public void unit_only_takes_damage_from_players_team(IUnit unit, bool boolean) => Engine.unit_only_takes_damage_from_players_team(unit, boolean);

        public void unit_set_active_camo(IUnit unit, bool boolean, float real) => Engine.unit_set_active_camo(unit, boolean, real);

        public void unit_set_current_vitality(IUnit unit, float body, float shield) => Engine.unit_set_current_vitality(unit, body, shield);

        public void unit_set_emotional_state(IUnit unit, string string_id, float weight, short transitionTime) => Engine.unit_set_emotional_state(unit, string_id, weight, transitionTime);

        public void unit_set_enterable_by_player(IUnit unit, bool boolean) => Engine.unit_set_enterable_by_player(unit, boolean);

        public void unit_set_maximum_vitality(IUnit unit, float real, float real1) => Engine.unit_set_maximum_vitality(unit, real, real1);

        public void unit_stop_custom_animation(IUnit unit) => Engine.unit_stop_custom_animation(unit);

        public IGameObject vehicle_driver(IUnit unit) => Engine.vehicle_driver(unit);

        public void vehicle_load_magic(IGameObject vehicle, string vehicleSeat, IGameObject unit) => Engine.vehicle_load_magic(vehicle, vehicleSeat, unit);

        public void vehicle_load_magic(IGameObject vehicle, string vehicleSeat, GameObjectList units) => Engine.vehicle_load_magic(vehicle, vehicleSeat, units);

        public bool vehicle_test_seat(IVehicle vehicle, string seat, IUnit unit) => Engine.vehicle_test_seat(vehicle, seat, unit);

        public bool vehicle_test_seat_list(IVehicle vehicle, string seat, IGameObject subject) => Engine.vehicle_test_seat_list(vehicle, seat, subject);

        public bool vehicle_test_seat_list(IVehicle vehicle, string seat, GameObjectList subjects) => Engine.vehicle_test_seat_list(vehicle, seat, subjects);

        public void vehicle_unload(IGameObject entity, string unit_seat_mapping) => Engine.vehicle_unload(entity, unit_seat_mapping);

        public GameObjectList volume_return_objects(ITriggerVolume trigger_volume) => Engine.volume_return_objects(trigger_volume);

        public GameObjectList volume_return_objects_by_type(ITriggerVolume trigger_volume, int value) => Engine.volume_return_objects_by_type(trigger_volume, value);

        public void volume_teleport_players_not_inside(ITriggerVolume trigger_volume, ILocationFlag cutscene_flag) => Engine.volume_teleport_players_not_inside(trigger_volume, cutscene_flag);

        public bool volume_test_object(ITriggerVolume trigger_volume, IGameObject entity) => Engine.volume_test_object(trigger_volume, entity);

        public bool volume_test_objects(ITriggerVolume trigger, IGameObject entity) => Engine.volume_test_objects(trigger, entity);

        public bool volume_test_objects(ITriggerVolume trigger, IAiActorDefinition actor) => Engine.volume_test_objects(trigger, actor);

        public bool volume_test_objects(ITriggerVolume trigger_volume, GameObjectList object_list) => Engine.volume_test_objects(trigger_volume, object_list);

        public bool volume_test_objects_all(ITriggerVolume trigger, IGameObject entity) => Engine.volume_test_objects_all(trigger, entity);

        public bool volume_test_objects_all(ITriggerVolume trigger, IAiActorDefinition actor) => Engine.volume_test_objects_all(trigger, actor);

        public bool volume_test_objects_all(ITriggerVolume trigger, GameObjectList object_list) => Engine.volume_test_objects_all(trigger, object_list);

        public void wake(IScriptMethodReference script_name) => Engine.wake(script_name);

        public void weapon_enable_warthog_chaingun_light(bool boolean) => Engine.weapon_enable_warthog_chaingun_light(boolean);

        public void weapon_hold_trigger(IWeaponReference weapon, int triggerIndex, bool boolean) => Engine.weapon_hold_trigger(weapon, triggerIndex, boolean);

        public void weather_change_intensity(float time, float intensity) => Engine.weather_change_intensity(time, intensity);

        public void weather_start(float time) => Engine.weather_start(time);

        public void weather_stop(float time) => Engine.weather_stop(time);
    }
}
