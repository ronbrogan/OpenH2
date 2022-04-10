using System;

namespace OpenH2.Core.Tags.Common
{
    [Flags]
    public enum GameObjectFlags : ushort
    {
        DoesNotCastShadow = 1,
        SearchCardinalDirectionLightmapsOnFail = 2,
        Unused = 4,
        NotAPathfindingObstacle = 8,
        ExtensionOfParent = 16,
        DoesNotCauseCollisionDamage = 32,
        EarlyMover = 64,
        EarlyMoverLocalizedPhysics = 128,
        UseStaticMassiveLightmapSample = 256,
        ObjectScalesAttachments = 512,
        InheritsPlayersAppearance = 1024,
        DeadBipedsCantLocalize = 2048,
        AttachToClustersByDynamicSphere = 4096,
        EffectsCreatedDoNotSpawnObjectInMulti = 8192,
        ProphetIsNotDisplayedInPegasusBuilds = 16384,
    }
}
