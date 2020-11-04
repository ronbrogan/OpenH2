using OpenH2.Core.GameObjects;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenH2.Engine.Scripting
{
    public partial class ScriptEngine : IScriptEngine
    {
        /// <summary>toggles script control of the camera.</summary>
        public void camera_control(bool boolean)
        {
        }

        /// <summary>moves the camera to the specified camera point over the specified number of ticks.</summary>
        public void camera_set(ICameraPathTarget cutscene_camera_point, short value)
        {
            this.cameraSystem.PerformCameraMove(value);
        }

        /// <summary>begins a prerecorded camera animation synchronized to unit relative to cutscene flag.</summary>
        public void camera_set_animation_relative(AnimationGraphTag animation, string trackName, IUnit unit, ILocationFlag locationFlag)
        {
            var track = animation.Tracks.FirstOrDefault(t => t.Description == trackName);

            if(track != null)
            {
                this.cameraSystem.PerformCameraMove(track.Values[8]);
            }

        }

        /// <summary>sets the field of view</summary>
        public void camera_set_field_of_view(float real, short value)
        {
        }

        /// <summary>returns the number of ticks remaining in the current camera interpolation.</summary>
        public short camera_time()
        {
            return (short)this.cameraSystem.GetCameraMoveRemaining();
        }

        /// <summary>clone the first player's most reasonable weapon and attach it to the specified object's marker</summary>
        public void cinematic_clone_players_weapon(IGameObject entity, string /*id*/ string_id, string /*id*/ string_id1)
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
        public void cinematic_set_title(ICinematicTitle cutscene_title)
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


        /// <summary>ur...</summary>
        public void play_credits()
        {
        }
    }
}
