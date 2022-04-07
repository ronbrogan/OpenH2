﻿using OpenH2.Core.GameObjects;
using OpenH2.Core.Scripting;

namespace OpenH2.Engine.Scripting
{
    public partial class ScriptEngine : IScriptEngine
    {

        /// <summary>animate the overlay over time</summary>
        public void device_animate_overlay(IDevice device, float real, float real1, float real12, float real123)
        {
        }

        /// <summary>animate the position over time</summary>
        public void device_animate_position(IDevice device, float real, float real1, float real12, float real123, bool boolean)
        {
        }

        /// <summary>animate the position over time</summary>
        public void device_animate_position(IDevice device, float floatValue, float floatValue0, float floatValue1, bool boolean)
        {
        }

        /// <summary>TRUE makes the given device close automatically after it has opened, FALSE makes it not</summary>
        public void device_closes_automatically_set(IDevice device, bool boolean)
        {
        }

        /// <summary>gets the current position of the given device (used for devices without explicit device groups)</summary>
        public float device_get_position(IDevice device)
        {
            return default(float);
        }

        /// <summary>TRUE allows a device to change states only once</summary>
        public void device_group_change_only_once_more_set(IDeviceGroup device_group, bool boolean)
        {
        }

        /// <summary>returns the desired value of the specified device group.</summary>
        public float device_group_get(IDeviceGroup device_group)
        {
            return default(float);
        }

        /// <summary>changes the desired value of the specified device group.</summary>
        public void device_group_set(IDevice device, IDeviceGroup device_group, float real)
        {
        }

        /// <summary>instantaneously changes the value of the specified device group.</summary>
        public void device_group_set_immediate(IDeviceGroup device_group, float real)
        {
        }

        /// <summary>TRUE makes the given device one-sided (only able to be opened from one direction), FALSE makes it two-sided</summary>
        public void device_one_sided_set(IDevice device, bool boolean)
        {
        }

        /// <summary>TRUE makes the given device open automatically when any biped is nearby, FALSE makes it not</summary>
        public void device_operates_automatically_set(IDevice device, bool boolean)
        {
        }

        /// <summary>changes a machine's never_appears_locked flag, but only if paul is a bastard</summary>
        public void device_set_never_appears_locked(IDevice device, bool boolean)
        {
        }

        /// <summary>set the desired overlay animation to use</summary>
        public void device_set_overlay_track(IDevice device, string /*id*/ string_id)
        {
        }

        /// <summary>set the desired position of the given device (used for devices without explicit device groups)</summary>
        public void device_set_position(IDevice device, float real)
        {
        }

        /// <summary>instantaneously changes the position of the given device (used for devices without explicit device groups</summary>
        public void device_set_position_immediate(IDevice device, float real)
        {
        }

        /// <summary>set the desired position track animation to use (optional interpolation time onto track)</summary>
        public void device_set_position_track(IDevice device, string /*id*/ string_id, float real)
        {
        }

        /// <summary>immediately sets the power of a named device to the given value</summary>
        public void device_set_power(IDevice device, float real)
        {
        }
    }
}
