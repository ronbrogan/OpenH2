using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Generators.Scripting
{
    internal static class TypeMapping
    {
        public static Dictionary<string, string> ImplementationToScriptDataType = new()
        {
            ["IInternedStringId"] = "StringId",
            ["IVehicleSeat"] = "VehicleSeat",
            ["ITriggerVolume"] = "Trigger",
            ["ILocationFlag"] = "LocationFlag",
            ["ICameraPathTarget"] = "CameraPathTarget",
            ["ICinematicTitle"] = "CinematicTitle",
            ["IDeviceGroup"] = "DeviceGroup",
            ["IAiActor"] = "AI",
            ["IAiScript"] = "AIScript",
            ["IAIBehavior"] = "AIBehavior",
            ["IAiOrders"] = "AIOrders",
            ["IStartingProfile"] = "StartingProfile",
            ["IBsp"] = "Bsp",
            ["INavigationPoint"] = "NavigationPoint",
            ["ISpatialPoint"] = "SpatialPoint",
            ["GameObjectList"] = "List",
            ["ISound"] = "Sound",
            ["IEffect"] = "Effect",
            ["IDamage"] = "DamageEffect",
            ["ILoopingSound"] = "LoopingSound",
            ["IAnimation"] = "Animation",
            ["IModel"] = "Model",
            ["IGameDifficulty"] = "GameDifficulty",
            ["ITeam"] = "Team",
            ["IDamageState"] = "DamageState",
            ["IGameObject"] = "Entity",
            ["IUnit"] = "Unit",
            ["IVehicle"] = "Vehicle",
            ["IWeaponReference"] = "WeaponReference",
            ["IDevice"] = "Device",
            ["IScenery"] = "Scenery",
            ["IEntityIdentifier"] = "EntityIdentifier",
        };
    }
}
