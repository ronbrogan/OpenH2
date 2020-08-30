﻿using OpenH2.Core.Scripting;
using System;
using System.Threading.Tasks;

namespace OpenH2.Core.Scripting
{
    public interface IScriptEngine
    {
        void activate_team_nav_point_flag(NavigationPoint navpoint, Team team, LocationFlag cutscene_flag, float real);
        void activate_team_nav_point_object(NavigationPoint navpoint, Team team, Entity entity, float real);
        ObjectList ai_actors(AI ai);
        void ai_allegiance(Team team, Team team1);
        void ai_attach_units(ObjectList units, AI ai);
        void ai_attach_units(Unit unit, AI ai);
        void ai_berserk(AI ai, bool boolean);
        void ai_braindead(AI ai, bool boolean);
        void ai_cannot_die(AI ai, bool boolean);
        short ai_combat_status(AI ai);
        void ai_dialogue_enable(bool boolean);
        void ai_disposable(AI ai, bool boolean);
        void ai_disregard(Entity unit, bool boolean);
        void ai_disregard(ObjectList object_list, bool boolean);
        void ai_enter_squad_vehicles(AI ai);
        void ai_erase(AI ai);
        void ai_erase_all();
        short ai_fighting_count(AI ai);
        Entity ai_get_object(AI ai);
        Unit ai_get_unit(AI ai);
        void ai_kill(AI ai);
        void ai_kill_silent(AI ai);
        short ai_living_count(AI ai);
        void ai_magically_see(AI ai, AI ai1);
        void ai_magically_see_object(AI ai, Entity value);
        void ai_migrate(AI ai);
        void ai_migrate(AI ai, AI ai1);
        short ai_nonswarm_count(AI ai);
        void ai_overcomes_oversteer(AI ai, bool boolean);
        void ai_place(AI ai);
        void ai_place(AI ai, float value);
        void ai_place(AI ai, short value);
        void ai_place_in_vehicle(AI ai, AI ai1);
        short ai_play_line(AI ai, string string_id);
        short ai_play_line_at_player(AI ai, string string_id);
        short ai_play_line_at_player(string emotion);
        short ai_play_line_on_object(Entity entity, string string_id);
        void ai_prefer_target(ObjectList units, bool boolean);
        void ai_renew(AI ai);
        bool ai_scene(string emotion, AIScript aiScript);
        bool ai_scene(string string_id, AIScript ai_command_script, AI ai);
        bool ai_scene(string string_id, AIScript ai_command_script, AI ai, AI ai1);
        void ai_set_active_camo(AI ai, bool boolean);
        void ai_set_blind(AI ai, bool boolean);
        void ai_set_deaf(AI ai, bool boolean);
        void ai_set_orders(AI ai, AIOrders ai_orders);
        short ai_spawn_count(AI ai);
        float ai_strength(AI ai);
        void ai_suppress_combat(AI ai, bool boolean);
        short ai_swarm_count(AI ai);
        void ai_teleport_to_starting_location_if_outside_bsp(AI ai);
        bool ai_trigger_test(string value, AI ai);
        void ai_vehicle_enter(AI ai);
        void ai_vehicle_enter(AI ai, string unit);
        void ai_vehicle_enter(AI ai, Unit unit, string unit_seat_mapping = null);
        void ai_vehicle_enter_immediate(AI ai, Unit unit, string seat = null);
        void ai_vehicle_exit(AI ai);
        Vehicle ai_vehicle_get(AI ai);
        Vehicle ai_vehicle_get_from_starting_location(AI ai);
        void ai_vehicle_reserve(Vehicle vehicle, bool boolean);
        void ai_vehicle_reserve_seat(string emotion, bool boolean);
        void ai_vehicle_reserve_seat(Vehicle vehicle, string string_id, bool boolean);
        bool ai_vitality_pinned(AI ai);
        void begin_random(params Action[] expressions);
        void begin_random(params Func<Task>[] expressions);
        bool bink_done();
        void biped_ragdoll(Unit unit);
        Task cache_block_for_one_frame();
        void camera_control(bool boolean);
        void camera_predict_resources_at_frame(Animation animation, string emotion, Unit unit, LocationFlag locationFlag, int intValue);
        void camera_predict_resources_at_point(CameraPathTarget cutscene_camera_point);
        void camera_set(CameraPathTarget cutscene_camera_point, short value);
        void camera_set_animation_relative(Animation animation, string id, Unit unit, LocationFlag locationFlag);
        void camera_set_field_of_view(float real, short value);
        short camera_time();
        void cheat_active_camouflage_by_player(short value, bool boolean);
        void cinematic_clone_players_weapon(Entity entity, string string_id, string string_id1);
        void cinematic_enable_ambience_details(bool boolean);
        void cinematic_lighting_set_ambient_light(float real, float real1, float real12);
        void cinematic_lighting_set_primary_light(float real, float real1, float real12, float real123, float real1234);
        void cinematic_lighting_set_secondary_light(float real, float real1, float real12, float real123, float real1234);
        void cinematic_lightmap_shadow_disable();
        void cinematic_lightmap_shadow_enable();
        void cinematic_outro_start();
        void cinematic_screen_effect_set_crossfade(float real);
        void cinematic_screen_effect_set_depth_of_field(float real, float real1, float real12, float real123, float real1234, float real12345, float real123456);
        void cinematic_screen_effect_start(bool boolean);
        void cinematic_screen_effect_stop();
        void cinematic_set_far_clip_distance(float real);
        void cinematic_set_near_clip_distance(float real);
        void cinematic_set_title(CinematicTitle cutscene_title);
        void cinematic_show_letterbox(bool boolean);
        void cinematic_show_letterbox_immediate(bool boolean);
        void cinematic_skip_start_internal();
        void cinematic_skip_stop_internal();
        void cinematic_start();
        void cinematic_stop();
        void cinematic_subtitle(string string_id, float real);
        bool controller_get_look_invert();
        void controller_set_look_invert();
        void controller_set_look_invert(bool boolean);
        void cs_abort_on_alert(bool boolean);
        void cs_abort_on_combat_status(short value);
        void cs_abort_on_damage(bool boolean);
        void cs_aim(bool boolean, SpatialPoint point);
        void cs_aim_object(bool boolean);
        void cs_aim_object(bool boolean, Entity entity);
        void cs_aim_player(bool boolean);
        void cs_approach(Entity entity, float real, float real1, float real12);
        void cs_approach(float floatValue, float floatValue1, float floatValue2);
        void cs_approach_player(float real, float real1, float real12);
        void cs_approach_stop();
        bool cs_command_script_queued(AI ai, AIScript ai_command_script);
        bool cs_command_script_running(AI ai, AIScript ai_command_script);
        void cs_crouch(bool boolean);
        void cs_custom_animation(Animation animation, string emotion, float floatValue, bool interpolate);
        void cs_deploy_turret(SpatialPoint point);
        void cs_enable_dialogue(bool boolean);
        void cs_enable_looking(bool boolean);
        void cs_enable_moving(bool boolean);
        void cs_enable_pathfinding_failsafe(bool boolean);
        void cs_enable_targeting(bool boolean);
        void cs_face(bool boolean, SpatialPoint point = null);
        void cs_face_object(bool boolean, Entity entity);
        void cs_face_player(bool boolean);
        void cs_fly_by();
        void cs_fly_by(SpatialPoint point, float tolerance = 0);
        void cs_fly_to(SpatialPoint point, float tolerance = 0);
        void cs_fly_to_and_face(SpatialPoint point, SpatialPoint face, float tolerance = 0);
        void cs_force_combat_status(short value);
        void cs_go_by(SpatialPoint point, SpatialPoint planeP, float planeD = 0);
        void cs_go_to(SpatialPoint point, float tolerance = 1);
        void cs_go_to_and_face(SpatialPoint point, SpatialPoint faceTowards);
        void cs_go_to_nearest(SpatialPoint destination);
        void cs_go_to_vehicle();
        void cs_go_to_vehicle(Vehicle vehicle);
        void cs_grenade(SpatialPoint point, int action);
        void cs_ignore_obstacles(bool boolean);
        void cs_jump(float real, float real1);
        void cs_jump_to_point(float real, float real1);
        void cs_look(bool boolean, SpatialPoint point = null);
        void cs_look_object(bool boolean);
        void cs_look_object(bool boolean, Entity entity);
        void cs_look_player(bool boolean);
        void cs_movement_mode(short value);
        void cs_move_in_direction(float real, float real1, float real12);
        bool cs_moving();
        void cs_pause(float real);
        void cs_play_line(string string_id);
        void cs_queue_command_script(AI ai, AIScript ai_command_script);
        void cs_run_command_script(AI ai, AIScript ai_command_script);
        void cs_set_behavior(AIBehavior ai_behavior);
        void cs_shoot(bool boolean);
        void cs_shoot(bool boolean, Entity entity);
        void cs_shoot_point(bool boolean, SpatialPoint point);
        void cs_stack_command_script(AI ai, AIScript ai_command_script);
        void cs_start_approach(Entity entity, float real, float real1, float real12);
        void cs_start_approach_player(float real, float real1, float real12);
        void cs_start_to(SpatialPoint destination);
        void cs_stop_custom_animation();
        void cs_suppress_dialogue_global(bool boolean);
        void cs_switch(string string_id);
        void cs_teleport(SpatialPoint destination, SpatialPoint facing);
        void cs_turn_sharpness(bool boolean, float real);
        void cs_vehicle_boost(bool boolean);
        void cs_vehicle_speed(float real);
        void custom_animation(Unit unit, Animation animation, string stringid, bool interpolate);
        void custom_animation(Unit unit, string emotion, bool interpolate);
        void custom_animation_loop(string emotion, bool interpolate);
        void custom_animation_loop(Unit unit, Animation animation1, string emotion, bool interpolate);
        void custom_animation_relative(Unit entity, Animation animation, string emotion, bool boolean, Entity other);
        void custom_animation_relative(Unit unit, string emotion, bool interpolate, Entity entity);
        void custom_animation_relative_loop(Unit unit, Animation animation2, string emotion, bool boolean, Entity entity);
        void damage_new(Damage damage, LocationFlag cutscene_flag);
        void damage_object(Damage damage, Entity entity);
        void damage_players(Damage damage);
        void data_mine_set_mission_segment(string value);
        void deactivate_team_nav_point_flag(Team team, LocationFlag cutscene_flag);
        void deactivate_team_nav_point_object(Team team);
        void deactivate_team_nav_point_object(Team team, Entity entity);
        void device_animate_overlay(Device device, float real, float real1, float real12, float real123);
        void device_animate_position(Device device, float floatValue, float floatValue0, float floatValue1, bool boolean);
        void device_animate_position(Device device, float real, float real1, float real12, float real123, bool boolean);
        void device_closes_automatically_set(Device device, bool boolean);
        float device_get_position(Device device);
        void device_group_change_only_once_more_set(DeviceGroup device_group, bool boolean);
        float device_group_get(DeviceGroup device_group);
        void device_group_set(Device device, DeviceGroup device_group, float real);
        void device_group_set_immediate(DeviceGroup device_group, float real);
        void device_one_sided_set(Device device, bool boolean);
        void device_operates_automatically_set(Device device, bool boolean);
        void device_set_never_appears_locked(Device device, bool boolean);
        void device_set_overlay_track(Device device, string string_id);
        void device_set_position(Device device, float real);
        void device_set_position_immediate(Device device, float real);
        void device_set_position_track(Device device, string string_id, float real);
        void device_set_power(Device device, float real);
        void disable_render_light_suppressor();
        void drop(string value);
        void effect_new(Effect effect, LocationFlag cutscene_flag);
        void effect_new_on_object_marker(Effect effect, Entity entity, string string_id);
        void effect_new_on_object_marker(Effect effect, string emotion);
        void enable_render_light_suppressor();
        Entity entity_at_marker(Entity entity, string string_id);
        Entity entity_get_parent();
        Entity entity_get_parent(Entity entity);
        void fade_in(float real, float real1, float real12, short value);
        void fade_out(float real, float real1, float real12, short value);
        void flock_start(string string_id);
        void flock_stop(string string_id);
        void game_can_use_flashlights(bool boolean);
        string game_difficulty_get();
        string game_difficulty_get_real();
        bool game_is_cooperative();
        bool game_is_playtest();
        void game_revert();
        bool game_reverted();
        bool game_safe_to_save();
        void game_save();
        void game_save_cancel();
        void game_save_cinematic_skip();
        void game_save_immediate();
        void game_save_no_timeout();
        bool game_saving();
        void game_won();
        void garbage_collect_now();
        void garbage_collect_unsafe();
        void geometry_cache_flush();
        T GetReference<T>(string reference);
        void hud_cinematic_fade(float real, float real1);
        void hud_enable_training(bool boolean);
        void hud_set_training_text(string string_id);
        void hud_show_training_text(bool boolean);
        bool ice_cream_flavor_available(int value);
        void ice_cream_flavor_stock(int value);
        void interpolator_start(string string_id, float real, float real1);
        void kill_volume_disable(Trigger trigger_volume);
        void kill_volume_enable(Trigger trigger_volume);
        short list_count(Entity e);
        short list_count(ObjectList object_list);
        short list_count_not_dead(ObjectList objects);
        Entity list_get(ObjectList object_list, int index);
        void loading_screen_fade_to_white();
        void map_reset();
        float max(float a, float b);
        short max(short a, short b);
        float min(float a, float b);
        short min(short a, short b);
        void objectives_clear();
        void objectives_finish_up_to(int value);
        void objectives_show_up_to(int value);
        void objects_attach(Entity entity, string string_id, Entity entity1, string string_id1);
        void objects_attach(Entity entity, string emotion0, string emotion1);
        void objects_attach(string emotion0, Entity entity, string emotion1);
        void objects_attach(string emotion0, string emotion1);
        bool objects_can_see_flag(Entity entity, LocationFlag locationFlag, float floatValue);
        bool objects_can_see_flag(ObjectList list, LocationFlag locationFlag, float floatValue);
        bool objects_can_see_object(Entity entity, EntityIdentifier obj, float degrees);
        bool objects_can_see_object(float floatValue);
        bool objects_can_see_object(ObjectList list, EntityIdentifier obj, float degrees);
        void objects_detach();
        void objects_detach(Entity entity);
        void objects_detach(Entity entity, Entity entity1);
        float objects_distance_to_flag(Entity entity, LocationFlag locationFlag);
        float objects_distance_to_flag(ObjectList list, LocationFlag locationFlag);
        float objects_distance_to_object(ObjectList list, Entity entity);
        void objects_predict(Entity entity);
        void objects_predict(ObjectList object_list);
        void objects_predict_high(Entity entity);
        Entity object_at_marker(Entity entity, string stringId);
        void object_cannot_die(bool boolean);
        void object_cannot_die(Entity entity, bool boolean);
        void object_cannot_take_damage(Entity entity);
        void object_cannot_take_damage(ObjectList object_list);
        void object_can_take_damage(Entity entity);
        void object_can_take_damage(ObjectList object_list);
        void object_cinematic_lod(bool boolean);
        void object_cinematic_lod(Entity entity, bool boolean);
        void object_cinematic_visibility(Entity entity, bool boolean);
        void object_clear_all_function_variables(Entity entity);
        void object_clear_function_variable(Entity entity, string string_id);
        void object_create(Entity object_name);
        void object_create(EntityIdentifier object_name);
        void object_create_anew(Entity entity);
        void object_create_anew(EntityIdentifier object_name);
        void object_create_anew_containing(string value);
        void object_create_clone(EntityIdentifier object_name);
        void object_create_containing(string value);
        void object_damage_damage_section(Entity entity, string string_id, float real);
        void object_damage_damage_section(string emotion, float floatValue);
        void object_destroy();
        void object_destroy(Entity entity);
        void object_destroy_containing(string value);
        void object_destroy_type_mask(int value);
        void object_dynamic_simulation_disable(bool boolean);
        void object_dynamic_simulation_disable(Entity entity, bool boolean);
        AI object_get_ai();
        AI object_get_ai(Entity entity);
        float object_get_health();
        float object_get_health(Entity entity);
        Entity object_get_parent(Entity entity);
        float object_get_shield();
        float object_get_shield(Entity entity);
        void object_hide(Entity entity, bool boolean);
        short object_model_targets_destroyed(Entity entity, string target);
        void object_set_deleted_when_deactivated(Entity entity);
        void object_set_function_variable(Entity entity, string string_id, float real, float real1);
        void object_set_function_variable(string emotion, float floatValue0, float floatValue1);
        void object_set_permutation(Entity entity, string string_id, string string_id1);
        void object_set_permutation(string emotion, string emotion1);
        void object_set_phantom_power(bool boolean);
        void object_set_phantom_power(Entity entity, bool boolean);
        void object_set_region_state(Entity entity, string string_id, DamageState model_state);
        void object_set_scale(Entity entity, float real, short value);
        void object_set_scale(float floatValue, short valueValue);
        void object_set_shield();
        void object_set_shield(Entity entity, float real);
        void object_set_shield_stun_infinite(Entity entity);
        void object_set_velocity(Entity entity, float real);
        void object_set_velocity(Entity entity, float real, float real1, float real12);
        void object_teleport(Entity entity, LocationFlag cutscene_flag);
        void object_type_predict(Entity entity);
        void object_type_predict_high(Entity entity);
        void object_uses_cinematic_lighting(bool boolean);
        void object_uses_cinematic_lighting(Entity entity, bool boolean);
        void physics_disable_character_ground_adhesion_forces(float real);
        void physics_set_gravity(float real);
        void physics_set_velocity_frame(float real, float real1, float real12);
        float pin(float value, float min, float max);
        short pin(short value, short min, short max);
        bool player0_looking_down();
        bool player0_looking_up();
        ObjectList players();
        bool player_action_test_accept();
        bool player_action_test_cancel();
        bool player_action_test_grenade_trigger();
        bool player_action_test_lookstick_backward();
        bool player_action_test_lookstick_forward();
        void player_action_test_look_down_begin();
        void player_action_test_look_pitch_end();
        void player_action_test_look_up_begin();
        bool player_action_test_melee();
        bool player_action_test_primary_trigger();
        void player_action_test_reset();
        bool player_action_test_vision_trigger();
        void player_camera_control(bool boolean);
        void player_disable_movement(bool boolean);
        void player_effect_set_max_rotation(float real, float real1, float real12);
        void player_effect_set_max_vibration(float real, float real1);
        void player_effect_start();
        void player_effect_start(float real, float real1);
        void player_effect_stop();
        void player_effect_stop(float real);
        void player_enable_input(bool boolean);
        bool player_flashlight_on();
        void player_training_activate_flashlight();
        void player_training_activate_stealth();
        void play_credits();
        void predict_model_section(Model render_model, int value);
        void predict_structure_section(Bsp structure_bsp, int value, bool boolean);
        void print(string value);
        void pvs_clear();
        void pvs_set_object(Entity entity);
        float random_range(float value, float value1);
        int random_range(int value, int value1);
        void rasterizer_bloom_override(bool boolean);
        void rasterizer_bloom_override_brightness(float real);
        void rasterizer_bloom_override_threshold(float real);
        float real_random_range(float real, float real1);
        void render_lights_enable_cinematic_shadow(bool boolean, Entity entity, string string_id, float real);
        void scenery_animation_start_loop(Scenery scenery, Animation animation, string emotion);
        void scenery_animation_start_relative(Scenery scenery, Animation animation, string emotion, Entity entity);
        void scenery_animation_start_relative(Scenery scenery, string emotion, Entity entity);
        short scenery_get_animation_time(Scenery scenery);
        void set_global_sound_environment(float real, float real1, float real12, float real123, int value, float real1234);
        Task sleep(int ticks);
        Task sleep(short ticks);
        void sleep_forever();
        void sleep_forever(ScriptReference script = null);
        Task sleep_until(Func<Task<bool>> condition, int ticks = 60, int timeout = -1);
        void sound_class_set_gain(string value, float gain, int ticks);
        int sound_impulse_language_time(ReferenceGet soundRef);
        void sound_impulse_predict(ReferenceGet soundRef);
        void sound_impulse_start(object sound, Entity entity, float floatValue);
        void sound_impulse_start_effect(ReferenceGet sound, Entity entity, float floatValue, string effect);
        void sound_impulse_stop(ReferenceGet sound);
        void sound_impulse_trigger(Entity sound, Entity source, float floatValue, int intValue);
        void sound_impulse_trigger(Entity sound, float floatValue, int intValue);
        void sound_looping_set_alternate(LoopingSound looping_sound, bool boolean);
        void sound_looping_start(LoopingSound looping_sound, Entity entity, float real);
        void sound_looping_start(LoopingSound loopingSound, float floatValue);
        void sound_looping_stop(LoopingSound looping_sound);
        void sound_suppress_ambience_update_on_revert();
        short structure_bsp_index();
        void switch_bsp(short value);
        void switch_bsp_by_name(Bsp structure_bsp);
        void texture_cache_flush();
        void texture_camera_off();
        void texture_camera_set_object_marker(Entity entity, string string_id, float real);
        void time_code_reset();
        void time_code_show(bool boolean);
        void time_code_start(bool boolean);
        Unit unit(Entity entity);
        void units_set_current_vitality(ObjectList units, float body, float shield);
        void units_set_maximum_vitality(ObjectList units, float body, float shield);
        void unit_add_equipment(Unit unit, Equipment starting_profile, bool reset, bool isGarbage);
        void unit_doesnt_drop_items(ObjectList entities);
        void unit_exit_vehicle(Unit unit, short value);
        short unit_get_custom_animation_time(Unit unit);
        float unit_get_health(Unit unit);
        float unit_get_shield();
        float unit_get_shield(Unit unit);
        bool unit_has_weapon(Unit unit, Weapon weapon);
        void unit_impervious(Entity unit, bool boolean);
        void unit_impervious(ObjectList object_list, bool boolean);
        bool unit_in_vehicle();
        bool unit_in_vehicle(Unit unit);
        bool unit_is_emitting(Unit unit);
        void unit_kill();
        void unit_kill(Unit unit);
        void unit_kill_silent(Unit unit);
        void unit_only_takes_damage_from_players_team(Unit unit, bool boolean);
        void unit_set_active_camo(Unit unit, bool boolean, float real);
        void unit_set_current_vitality(float body, float shield);
        void unit_set_current_vitality(Unit unit, float real, float real1);
        void unit_set_emotional_state(string emotion, float floatValue, short valueValue);
        void unit_set_emotional_state(Unit unit, string string_id, float real, short value);
        void unit_set_enterable_by_player(Unit unit, bool boolean);
        void unit_set_maximum_vitality(float body, float shield);
        void unit_set_maximum_vitality(Unit unit, float real, float real1);
        void unit_stop_custom_animation(Unit unit);
        Entity vehicle_driver(Unit unit);
        void vehicle_load_magic(Entity vehicle, string vehicleSeat, Entity unit);
        void vehicle_load_magic(Entity vehicle, string vehicleSeat, ObjectList units);
        bool vehicle_test_seat(Vehicle vehicle, string seat, Unit unit);
        bool vehicle_test_seat_list(Vehicle vehicle, string seat, Entity subject);
        bool vehicle_test_seat_list(Vehicle vehicle, string seat, ObjectList subjects);
        void vehicle_unload(Entity entity, string unit_seat_mapping);
        ObjectList volume_return_objects(Trigger trigger_volume);
        ObjectList volume_return_objects_by_type(Trigger trigger_volume, int value);
        void volume_teleport_players_not_inside(Trigger trigger_volume, LocationFlag cutscene_flag);
        bool volume_test_object(Trigger trigger);
        bool volume_test_object(Trigger trigger_volume, Entity entity);
        bool volume_test_objects(Trigger trigger, Entity entity);
        bool volume_test_objects(Trigger trigger_volume, ObjectList object_list);
        bool volume_test_objects_all(Trigger trigger, Entity entity);
        bool volume_test_objects_all(Trigger trigger, ObjectList object_list);
        void wake(ScriptReference script_name);
        void weapon_enable_warthog_chaingun_light(bool boolean);
        void weapon_hold_trigger(WeaponReference weapon, int triggerIndex, bool boolean);
        void weather_change_intensity(float floatValue);
        void weather_change_intensity(float real, float real1);
        void weather_start();
        void weather_start(float real);
        void weather_stop();
        void weather_stop(float real);
    }
}