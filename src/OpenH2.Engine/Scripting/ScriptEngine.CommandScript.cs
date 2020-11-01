using OpenH2.Core.GameObjects;
using OpenH2.Core.Scripting;

namespace OpenH2.Engine.Scripting
{
    public partial class ScriptEngine : IScriptEngine
    {

        /// <summary>Command script ends prematurely when actor's combat status raises to 'alert' or higher</summary>
        public void cs_abort_on_alert(bool boolean)
        {
        }

        /// <summary>Command script ends prematurely when actor's combat status rises to given level</summary>
        public void cs_abort_on_combat_status(short value)
        {
        }

        /// <summary>Command script ends prematurely when actor is damaged</summary>
        public void cs_abort_on_damage(bool boolean)
        {
        }

        /// <summary>Actor aims at the point for the remainder of the cs, or until overridden (overrides look)</summary>
        public void cs_aim(bool boolean, ISpatialPoint point)
        {
        }

        /// <summary>Actor aims at the object for the duration of the cs, or until overridden (overrides look)</summary>
        public void cs_aim_object(bool boolean, IGameObject entity)
        {
        }

        /// <summary>Actor aims at the object for the duration of the cs, or until overridden (overrides look)</summary>
        public void cs_aim_object(bool boolean)
        {
        }

        /// <summary>Actor aims at nearest player for the duration of the cs, or until overridden (overrides look)</summary>
        public void cs_aim_player(bool boolean)
        {
        }

        /// <summary></summary>
        public void cs_approach(IGameObject entity, float real, float real1, float real12)
        {
        }

        /// <summary></summary>
        public void cs_approach(float floatValue, float floatValue1, float floatValue2)
        {
        }

        /// <summary></summary>
        public void cs_approach_player(float real, float real1, float real12)
        {
        }

        /// <summary>Actor stops approaching</summary>
        public void cs_approach_stop()
        {
        }

        /// <summary>Returns true if the command script is in the ai's cs queue</summary>
        public bool cs_command_script_queued(IAiActorDefinition ai, AIScript ai_command_script)
        {
            return default(bool);
        }

        /// <summary>Returns true if the ai is running the command script in question</summary>
        public bool cs_command_script_running(IAiActorDefinition ai, AIScript ai_command_script)
        {
            return default(bool);
        }

        /// <summary>Actor crouches for the remainder of the command script, or until overridden</summary>
        public void cs_crouch(bool boolean)
        {
        }

        /// <summary>starts a custom animation playing on the unit (interpolates into animation if last parameter is TRUE)</summary>
        public void cs_custom_animation(IAnimation animation, string /*id*/ emotion, float floatValue, bool interpolate)
        {
        }

        /// <summary>Deploy a turret at the given script point</summary>
        public void cs_deploy_turret(ISpatialPoint point)
        {
        }

        /// <summary>Actor combat dialogue enabled/disabled.</summary>
        public void cs_enable_dialogue(bool boolean)
        {
        }

        /// <summary>Actor autonomous looking enabled/disabled.</summary>
        public void cs_enable_looking(bool boolean)
        {
        }

        /// <summary>Actor autonomous moving enabled/disabled.</summary>
        public void cs_enable_moving(bool boolean)
        {
        }

        /// <summary>Actor blocks until pathfinding calls succeed</summary>
        public void cs_enable_pathfinding_failsafe(bool boolean)
        {
        }

        /// <summary>Actor autonomous target selection enabled/disabled.</summary>
        public void cs_enable_targeting(bool boolean)
        {
        }

        /// <summary>Actor faces exactly the point for the remainder of the cs, or until overridden (overrides aim, look)</summary>
        public void cs_face(bool boolean, ISpatialPoint point = null)
        {
        }

        /// <summary>Actor faces exactly the given object for the duration of the cs, or until overridden (overrides aim, look)</summary>
        public void cs_face_object(bool boolean, IGameObject entity)
        {
        }

        /// <summary>Actor faces exactly the nearest player for the duration of the cs, or until overridden (overrides aim, look)</summary>
        public void cs_face_player(bool boolean)
        {
        }

        /// <summary>Flies the actor through the given point</summary>
        public void cs_fly_by()
        {
        }

        /// <summary>Flies the actor through the given point</summary>
        public void cs_fly_by(ISpatialPoint point, float tolerance = 0f)
        {
        }

        /// <summary>Flies the actor to the given point (within the given tolerance)</summary>
        public void cs_fly_to(ISpatialPoint point, float tolerance = 0f)
        {
        }

        /// <summary>Flies the actor to the given point and orients him in the appropriate direction (within the given tolerance)</summary>
        public void cs_fly_to_and_face(ISpatialPoint point, ISpatialPoint face, float tolerance = 0f)
        {
        }

        /// <summary>Force the actor's combat status (0= no override, 1= asleep, 2=idle, 3= alert, 4= active)</summary>
        public void cs_force_combat_status(short value)
        {
        }

        /// <summary>Actor moves toward the point, and considers it hit when it breaks the indicated plane</summary>
        public void cs_go_by(ISpatialPoint point, ISpatialPoint planeP, float planeD = 0f)
        {
        }

        /// <summary>Moves the actor to a specified point</summary>
        public void cs_go_to(ISpatialPoint point, float tolerance = 1f)
        {
        }

        /// <summary>Moves the actor to a specified point and has him face the second point</summary>
        public void cs_go_to_and_face(ISpatialPoint point, ISpatialPoint faceTowards)
        {
        }

        /// <summary>Given a point set, AI goes toward the nearest point</summary>
        public void cs_go_to_nearest(ISpatialPoint destination)
        {
        }

        /// <summary>Actor gets in the appropriate vehicle</summary>
        public void cs_go_to_vehicle()
        {
        }

        /// <summary>Actor gets in the appropriate vehicle</summary>
        public void cs_go_to_vehicle(IVehicle vehicle)
        {
        }

        /// <summary>Actor throws a grenade, either by tossing (arg2=0), lobbing (1) or bouncing (2)</summary>
        public void cs_grenade(ISpatialPoint point, int action)
        {
        }

        /// <summary>Actor does not avoid obstacles when true</summary>
        public void cs_ignore_obstacles(bool boolean)
        {
        }

        /// <summary>Actor jumps in direction of angle at the given velocity (angle, velocity)</summary>
        public void cs_jump(float real, float real1)
        {
        }

        /// <summary>Actor jumps with given horizontal and vertical velocity</summary>
        public void cs_jump_to_point(float real, float real1)
        {
        }

        /// <summary>Actor looks at the point for the remainder of the cs, or until overridden</summary>
        public void cs_look(bool boolean, ISpatialPoint point = null)
        {
        }

        /// <summary>Actor looks at the object for the duration of the cs, or until overridden</summary>
        public void cs_look_object(bool boolean)
        {
        }

        /// <summary>Actor looks at the object for the duration of the cs, or until overridden</summary>
        public void cs_look_object(bool boolean, IGameObject entity)
        {
        }

        /// <summary>Actor looks at nearest player for the duration of the cs, or until overridden</summary>
        public void cs_look_player(bool boolean)
        {
        }

        /// <summary>Actor moves at given angle, for the given distance, optionally with the given facing (angle, distance, facing)</summary>
        public void cs_move_in_direction(float real, float real1, float real12)
        {
        }

        /// <summary>Actor switches to given animation mode</summary>
        public void cs_movement_mode(short value)
        {
        }

        /// <summary>Returns TRUE if the actor is currently following a path</summary>
        public bool cs_moving()
        {
            return default(bool);
        }

        /// <summary>The actor does nothing for the given number of seconds</summary>
        public void cs_pause(float real)
        {
        }

        /// <summary>Play the named line in the current scene</summary>
        public void cs_play_line(string /*id*/ string_id)
        {
        }

        /// <summary>Add a command script onto the end of an actor's command script queue</summary>
        public void cs_queue_command_script(IAiActorDefinition ai, AIScript ai_command_script)
        {
        }

        /// <summary>Causes the specified actor(s) to start executing a command script immediately (discarding any other command scripts in the queue)</summary>
        public void cs_run_command_script(IAiActorDefinition ai, AIScript ai_command_script)
        {
        }

        /// <summary>Actor performs the indicated behavior</summary>
        public void cs_set_behavior(IAIBehavior ai_behavior)
        {
        }

        /// <summary>Actor is allowed to shoot at its target or not</summary>
        public void cs_shoot(bool boolean)
        {
        }

        /// <summary>Actor is allowed to shoot at its target or not</summary>
        public void cs_shoot(bool boolean, IGameObject entity)
        {
        }

        /// <summary>Actor shoots at given point</summary>
        public void cs_shoot_point(bool boolean, ISpatialPoint point)
        {
        }

        /// <summary>Push a command script to the top of the actor's command script queue</summary>
        public void cs_stack_command_script(IAiActorDefinition ai, AIScript ai_command_script)
        {
        }

        /// <summary></summary>
        public void cs_start_approach(IGameObject entity, float real, float real1, float real12)
        {
        }

        /// <summary></summary>
        public void cs_start_approach_player(float real, float real1, float real12)
        {
        }

        /// <summary>Moves the actor to a specified point. DOES NOT BLOCK SCRIPT EXECUTION.</summary>
        public void cs_start_to(ISpatialPoint destination)
        {
        }

        /// <summary>Stop running a custom animation</summary>
        public void cs_stop_custom_animation()
        {
        }

        /// <summary>Combat dialogue is suppressed for the remainder of the command script</summary>
        public void cs_suppress_dialogue_global(bool boolean)
        {
        }

        /// <summary>Switch control of the joint command script to the given member</summary>
        public void cs_switch(string /*id*/ string_id)
        {

        }
        /// <summary>Actor teleports to point1 facing point2</summary>
        public void cs_teleport(ISpatialPoint destination, ISpatialPoint facing)
        {
        }

        /// <summary>Set the sharpness of a vehicle turn (values 0 -> 1). Only applicable to nondirectional flying vehicles (e.g. dropships)</summary>
        public void cs_turn_sharpness(bool boolean, float real)
        {
        }

        /// <summary>Enables or disables boost</summary>
        public void cs_vehicle_boost(bool boolean)
        {
        }

        /// <summary>Set the speed at which the actor will drive a vehicle, expressed as a multiplier 0-1</summary>
        public void cs_vehicle_speed(float real)
        {
        }

    }
}
