using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;

namespace OpenH2.Engine.Scripting
{
    public partial class ScriptEngine : IScriptEngine
    {

        /// <summary>animate the overlay over time</summary>
        public void device_animate_overlay(Device device, float real, float real1, float real12, float real123)
        {
        }

        /// <summary>animate the position over time</summary>
        public void device_animate_position(Device device, float real, float real1, float real12, float real123, bool boolean)
        {
        }

        /// <summary>animate the position over time</summary>
        public void device_animate_position(Device device, float floatValue, float floatValue0, float floatValue1, bool boolean)
        {
        }

        /// <summary>TRUE makes the given device close automatically after it has opened, FALSE makes it not</summary>
        public void device_closes_automatically_set(Device device, bool boolean)
        {
        }

        /// <summary>gets the current position of the given device (used for devices without explicit device groups)</summary>
        public float device_get_position(Device device)
        {
            return default(float);
        }

        /// <summary>TRUE allows a device to change states only once</summary>
        public void device_group_change_only_once_more_set(ScenarioTag.DeviceGroupDefinition device_group, bool boolean)
        {
        }

        /// <summary>returns the desired value of the specified device group.</summary>
        public float device_group_get(ScenarioTag.DeviceGroupDefinition device_group)
        {
            return default(float);
        }

        /// <summary>changes the desired value of the specified device group.</summary>
        public void device_group_set(Device device, ScenarioTag.DeviceGroupDefinition device_group, float real)
        {
        }

        /// <summary>instantaneously changes the value of the specified device group.</summary>
        public void device_group_set_immediate(ScenarioTag.DeviceGroupDefinition device_group, float real)
        {
        }

        /// <summary>TRUE makes the given device one-sided (only able to be opened from one direction), FALSE makes it two-sided</summary>
        public void device_one_sided_set(Device device, bool boolean)
        {
        }

        /// <summary>TRUE makes the given device open automatically when any biped is nearby, FALSE makes it not</summary>
        public void device_operates_automatically_set(Device device, bool boolean)
        {
        }

        /// <summary>changes a machine's never_appears_locked flag, but only if paul is a bastard</summary>
        public void device_set_never_appears_locked(Device device, bool boolean)
        {
        }

        /// <summary>set the desired overlay animation to use</summary>
        public void device_set_overlay_track(Device device, string /*id*/ string_id)
        {
        }

        /// <summary>set the desired position of the given device (used for devices without explicit device groups)</summary>
        public void device_set_position(Device device, float real)
        {
        }

        /// <summary>instantaneously changes the position of the given device (used for devices without explicit device groups</summary>
        public void device_set_position_immediate(Device device, float real)
        {
        }

        /// <summary>set the desired position track animation to use (optional interpolation time onto track)</summary>
        public void device_set_position_track(Device device, string /*id*/ string_id, float real)
        {
        }

        /// <summary>immediately sets the power of a named device to the given value</summary>
        public void device_set_power(Device device, float real)
        {
        }
    }
}
