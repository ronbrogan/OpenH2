using OpenH2.Core.GameObjects;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags;

namespace OpenH2.Engine.Scripting
{
    public partial class ScriptEngine : IScriptEngine
    {
        /// <summary>changes the gain on the specified sound class(es) to the specified gain over the specified number of ticks.</summary>
        public void sound_class_set_gain(string value, float gain, int ticks)
        {
        }

        /// <summary>returns the time remaining for the specified impulse sound. DO NOT CALL IN CUTSCENES.</summary>
        public int sound_impulse_language_time(SoundTag soundRef)
        {
            return (int)(this.audioSystem.SecondsRemaining(soundRef) * TicksPerSecond);
        }

        /// <summary>plays an impulse sound from the specified source object (or "none"), with the specified scale.</summary>
        public void sound_impulse_start(SoundTag sound, IGameObject entity, float floatValue)
        {
            this.audioSystem.Start(sound, entity);
        }

        /// <summary>plays an impulse sound from the specified source object (or "none"), with the specified scale and effect.</summary>
        public void sound_impulse_start_effect(SoundTag sound, IGameObject entity, float floatValue, string /*id*/ effect)
        {
            this.audioSystem.Start(sound, entity);
        }

        /// <summary>stops the specified impulse sound.</summary>
        public void sound_impulse_stop(SoundTag sound)
        {
        }

        /// <summary>plays an impulse sound from the specified source object (or "none"), with the specified scale.</summary>
        public void sound_impulse_trigger(SoundTag sound, IGameObject source, float floatValue, int intValue)
        {
            this.audioSystem.Start(sound, source);
        }

        /// <summary>enables or disables the alternate loop/alternate end for a looping sound.</summary>
        public void sound_looping_set_alternate(LoopingSoundTag looping_sound, bool boolean)
        {
        }

        /// <summary>plays a looping sound from the specified source object (or "none"), with the specified scale.</summary>
        public void sound_looping_start(LoopingSoundTag looping_sound, IGameObject entity, float real)
        {
        }

        /// <summary>stops the specified looping sound.</summary>
        public void sound_looping_stop(LoopingSoundTag looping_sound)
        {
        }

        /// <summary>call this when transitioning between two cinematics so ambience won't fade in between the skips</summary>
        public void sound_suppress_ambience_update_on_revert()
        {
        }
    }
}
