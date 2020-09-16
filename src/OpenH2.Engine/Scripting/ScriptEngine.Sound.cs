using OpenH2.Core.GameObjects;
using OpenH2.Core.Scripting;

namespace OpenH2.Engine.Scripting
{
    public partial class ScriptEngine : IScriptEngine
    {
        /// <summary>changes the gain on the specified sound class(es) to the specified gain over the specified number of ticks.</summary>
        public void sound_class_set_gain(string value, float gain, int ticks)
        {
        }

        /// <summary>returns the time remaining for the specified impulse sound. DO NOT CALL IN CUTSCENES.</summary>
        public int sound_impulse_language_time(ReferenceGet soundRef)
        {
            return TicksPerSecond;
        }

        /// <summary>plays an impulse sound from the specified source object (or "none"), with the specified scale.</summary>
        public void sound_impulse_start(object sound, IGameObject entity, float floatValue)
        {
        }

        /// <summary>plays an impulse sound from the specified source object (or "none"), with the specified scale and effect.</summary>
        public void sound_impulse_start_effect(ReferenceGet sound, IGameObject entity, float floatValue, string /*id*/ effect)
        {
        }

        /// <summary>stops the specified impulse sound.</summary>
        public void sound_impulse_stop(ReferenceGet sound)
        {
        }

        /// <summary>plays an impulse sound from the specified source object (or "none"), with the specified scale.</summary>
        public void sound_impulse_trigger(IGameObject sound, float floatValue, int intValue)
        {
        }

        /// <summary>plays an impulse sound from the specified source object (or "none"), with the specified scale.</summary>
        public void sound_impulse_trigger(IGameObject sound, IGameObject source, float floatValue, int intValue)
        {
        }

        /// <summary>enables or disables the alternate loop/alternate end for a looping sound.</summary>
        public void sound_looping_set_alternate(LoopingSound looping_sound, bool boolean)
        {
        }

        /// <summary>plays a looping sound from the specified source object (or "none"), with the specified scale.</summary>
        public void sound_looping_start(LoopingSound looping_sound, IGameObject entity, float real)
        {
        }

        /// <summary>plays a looping sound from the specified source object (or "none"), with the specified scale.</summary>
        public void sound_looping_start(LoopingSound loopingSound, float floatValue)
        {
        }

        /// <summary>stops the specified looping sound.</summary>
        public void sound_looping_stop(LoopingSound looping_sound)
        {
        }

        /// <summary>call this when transitioning between two cinematics so ambience won't fade in between the skips</summary>
        public void sound_suppress_ambience_update_on_revert()
        {
        }
    }
}
