using OpenH2.Core.Scripting;

namespace OpenH2.Engine.Scripting
{
    public partial class ScriptEngine : IScriptEngine
    {
        /// <summary>returns true if any player has hit accept since the last call to (player_action_test_reset).</summary>
        public bool player_action_test_accept()
        {
            return default(bool);
        }

        /// <summary>returns true if any player has hit cancel key since the last call to (player_action_test_reset).</summary>
        public bool player_action_test_cancel()
        {
            return default(bool);
        }

        /// <summary>returns true if any player has used grenade trigger since the last call to (player_action_test_reset).</summary>
        public bool player_action_test_grenade_trigger()
        {
            return default(bool);
        }

        /// <summary>sets down player look down test</summary>
        public void player_action_test_look_down_begin()
        {
        }

        /// <summary>ends the look pitch testing</summary>
        public void player_action_test_look_pitch_end()
        {
        }

        /// <summary>sets up player look up test</summary>
        public void player_action_test_look_up_begin()
        {
        }

        /// <summary>true if the first player pushed backward on lookstick</summary>
        public bool player_action_test_lookstick_backward()
        {
            return default(bool);
        }

        /// <summary>true if the first player pushed forward on lookstick</summary>
        public bool player_action_test_lookstick_forward()
        {
            return default(bool);
        }

        /// <summary>returns true if any player has hit the melee button since the last call to (player_action_test_reset).</summary>
        public bool player_action_test_melee()
        {
            return default(bool);
        }

        /// <summary>returns true if any player has used primary trigger since the last call to (player_action_test_reset).</summary>
        public bool player_action_test_primary_trigger()
        {
            return default(bool);
        }

        /// <summary>resets the player action test state so that all tests will return false.</summary>
        public void player_action_test_reset()
        {
        }

        /// <summary>returns true if any player has used vision trigger since the last call to (player_action_test_reset).</summary>
        public bool player_action_test_vision_trigger()
        {
            return default(bool);
        }

        /// <summary>enables/disables camera control globally</summary>
        public void player_camera_control(bool boolean)
        {
        }

        /// <summary>toggle player input. the look stick works, but nothing else.</summary>
        public void player_disable_movement(bool boolean)
        {
        }

        /// <summary><yaw> <pitch> <roll></summary>
        public void player_effect_set_max_rotation(float real, float real1, float real12)
        {
        }

        /// <summary><left> <right></summary>
        public void player_effect_set_max_vibration(float real, float real1)
        {
        }

        /// <summary><max_intensity> <attack time></summary>
        public void player_effect_start(float real, float real1)
        {
        }

        /// <summary><decay></summary>
        public void player_effect_stop(float real)
        {
        }

        /// <summary>toggle player input. the player can still free-look, but nothing else.</summary>
        public void player_enable_input(bool boolean)
        {
        }

        /// <summary>returns true if any player has a flashlight on</summary>
        public bool player_flashlight_on()
        {
            return default(bool);
        }

        /// <summary>guess</summary>
        public void player_training_activate_flashlight()
        {
        }

        /// <summary>guess</summary>
        public void player_training_activate_stealth()
        {
        }

        /// <summary>true if the first player is looking all the way down</summary>
        public bool player0_looking_down()
        {
            return default(bool);
        }

        /// <summary>true if the first player is looking all the way up</summary>
        public bool player0_looking_up()
        {
            return default(bool);
        }

        /// <summary>returns a list of the players</summary>
        public GameObjectList players()
        {
            return new GameObjectList(this.actorSystem.Players);
        }
    }
}
