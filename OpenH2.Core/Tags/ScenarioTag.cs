using OpenH2.Core.Tags.Layout;
using System.Numerics;

namespace OpenH2.Core.Tags
{
    [TagLabel("scnr")]
    public class ScenarioTag : BaseTag
    {
        public override string Name { get; set; }
        public ScenarioTag(uint id) : base(id)
        {
        }

        [InternalReferenceValue(8)]
        public SkyboxReference[] SkyboxReferences { get; set; }


        [InternalReferenceValue(72)] public Obj72[] Obj72s { get; set; }

        [InternalReferenceValue(80)] public SceneryInstance[] SceneryInstances { get; set; }
        [InternalReferenceValue(88)] public SceneryDefinition[] SceneryReferences { get; set; }

        [InternalReferenceValue(96)] public Obj96[] Obj96s { get; set; }
        [InternalReferenceValue(104)] public BipedDefinition[] BipedDefinitions { get; set; }
        [InternalReferenceValue(144)] public AmmunitionInstance[] AmmoPickupInstances { get; set; }
        [InternalReferenceValue(152)] public WeaponDefinition[] WeaponDefinitions { get; set; }
        [InternalReferenceValue(160)] public Obj160[] Obj160s { get; set; }
        [InternalReferenceValue(168)] public MachineryInstance[] MachineryInstances { get; set; }
        [InternalReferenceValue(176)] public MachineryDefinition[] MachineryDefinitions { get; set; }
        [InternalReferenceValue(184)] public ControllerInstance[] ControllerInstances { get; set; }
        [InternalReferenceValue(192)] public ControllerDefinition[] ControllerDefinitions { get; set; }

        

        [InternalReferenceValue(216)] 
        public SoundSceneryInstance[] SoundSceneryInstances { get; set; }

        [InternalReferenceValue(224)] 
        public SoundSceneryDefinition[] SoundSceneryDefinitions { get; set; }

        [InternalReferenceValue(232)] 
        public LightInstance[] LightInstances { get; set; }

        [InternalReferenceValue(240)] public Obj240[] Obj240s { get; set; }

        [InternalReferenceValue(256)] 
        public PlayerSpawnMarker[] PlayerSpawnMarkers { get; set; }

        [InternalReferenceValue(264)] public Obj264[] Obj264s { get; set; }

        [InternalReferenceValue(280)] 
        public GameModeMarker[] GameModeMarkers { get; set; }

        [InternalReferenceValue(288)] 
        public ItemCollectionPlacement[] ItemCollectionPlacements { get; set; }

        [InternalReferenceValue(296)] public Obj296[] Obj296s { get; set; }

        [InternalReferenceValue(312)] 
        public DecalInstance[] DecalInstances { get; set; }



        [InternalReferenceValue(320)] public Obj320[] Obj320s { get; set; }

        [InternalReferenceValue(432)] public Obj432[] Obj432s { get; set; }


        [InternalReferenceValue(472)] public Obj472[] Obj472s { get; set; }

        [InternalReferenceValue(480)] public Obj480[] Obj480s { get; set; }
        [InternalReferenceValue(488)] public Obj488[] Obj488s { get; set; }

        [InternalReferenceValue(528)]
        public Terrain[] Terrains { get; set; }

        [InternalReferenceValue(536)] public Obj536[] Obj536s { get; set; }

        [InternalReferenceValue(560)] public Obj560[] Obj560s { get; set; }

        [InternalReferenceValue(568)] public Obj568[] Obj568s { get; set; }

        [InternalReferenceValue(592)] public Obj592[] Obj592s { get; set; }

        [InternalReferenceValue(600)] public Obj600[] Obj600s { get; set; }

        [InternalReferenceValue(656)] public Obj656[] Obj656s { get; set; }

        [InternalReferenceValue(792)] public Obj792[] Obj792s { get; set; }

        [InternalReferenceValue(808)] 
        public BlocInstance[] BlocInstances { get; set; }

        [InternalReferenceValue(816)] 
        public BlocDefinition[] BlocDefinitions { get; set; }

        [InternalReferenceValue(840)] public Obj840[] Obj840s { get; set; }

        [InternalReferenceValue(904)] public Obj904[] Obj904s { get; set; }

        [InternalReferenceValue(920)] public Obj920[] Obj920s { get; set; }


        [FixedLength(8)]
        public class SkyboxReference
        {
            [PrimitiveValue(4)]
            public uint SkyboxId { get; set; }
        }

        [FixedLength(68)]
        public class Terrain
        {
            [PrimitiveValue(20)]
            public uint BspId { get; set; }

            public BspTag Bsp { get; set; }

            // TODO implement lightmap tag
            [PrimitiveValue(28)]
            public uint LightmapId { get; set; }

            public BaseTag Lightmap { get; set; }
        }

        [FixedLength(36)] public class Obj72 { }

        [FixedLength(92)]
        public class SceneryInstance
        {
            [PrimitiveValue(0)]
            public ushort SceneryDefinitionIndex { get; set; }

            [PrimitiveValue(8)]
            public Vector3 Position { get; set; }

            [PrimitiveValue(20)]
            public Vector3 Orientation { get; set; }
        }

        [FixedLength(40)]
        public class SceneryDefinition
        {
            [PrimitiveValue(4)]
            public uint SceneryId { get; set; }
        }

        [FixedLength(84)]
        public class Obj96
        {
            [PrimitiveValue(0)]
            public ushort Index { get; set; }

            [PrimitiveValue(8)]
            public Vector3 Position { get; set; }

            [PrimitiveValue(20)]
            public Vector3 Orientation { get; set; }
        }

        [FixedLength(40)]
        public class BipedDefinition 
        {
            [PrimitiveValue(4)]
            public uint BipedId { get; set; }
        }


        [FixedLength(84)]
        public class AmmunitionInstance
        {
            [PrimitiveValue(0)]
            public ushort Index { get; set; }

            [PrimitiveValue(8)]
            public Vector3 Position { get; set; }

            [PrimitiveValue(20)]
            public Vector3 Orientation { get; set; }
        }

        [FixedLength(40)] public class Obj160 { }

        [FixedLength(68)]
        public class ControllerInstance
        {
            [PrimitiveValue(0)]
            public ushort Index { get; set; }

            [PrimitiveValue(8)]
            public Vector3 Position { get; set; }

            [PrimitiveValue(20)]
            public Vector3 Orientation { get; set; }
        }

        [FixedLength(40)] 
        public class ControllerDefinition 
        {
            [PrimitiveValue(4)]
            public uint ControllerId { get; set; }
        }


        [FixedLength(40)] public class WeaponDefinition 
        {
            [PrimitiveValue(4)]
            public uint WeaponId { get; set; }
        }

        [FixedLength(72)]
        public class MachineryInstance
        {
            [PrimitiveValue(0)]
            public ushort MachineryDefinitionIndex { get; set; }

            [PrimitiveValue(8)]
            public Vector3 Position { get; set; }

            [PrimitiveValue(20)]
            public Vector3 Orientation { get; set; }
        }

        [FixedLength(40)]
        public class MachineryDefinition
        {
            [PrimitiveValue(4)]
            public uint MachineryId { get; set; }
        }

        [FixedLength(80)]
        public class SoundSceneryInstance
        {
            [PrimitiveValue(0)]
            public ushort SoundSceneryDefinitionIndex { get; set; }

            [PrimitiveValue(8)]
            public Vector3 Position { get; set; }

            [PrimitiveValue(20)]
            public Vector3 Orientation { get; set; }
        }

        [FixedLength(40)] public class SoundSceneryDefinition 
        {
            [PrimitiveValue(4)]
            public uint SoundSceneryId { get; set; }
        }

        [FixedLength(108)]
        public class LightInstance
        {
            [PrimitiveValue(0)]
            public ushort Index { get; set; }

            [PrimitiveValue(8)]
            public Vector3 Position { get; set; }

            [PrimitiveValue(20)]
            public Vector3 Orientation { get; set; }
        }

        [FixedLength(40)] public class Obj240 { }

        [FixedLength(52)]
        public class PlayerSpawnMarker
        {
            [PrimitiveValue(0)]
            public Vector3 Position { get; set; }
        }

        [FixedLength(68)]
        public class Obj264
        {
            [PrimitiveValue(12)]
            public Vector3 Position { get; set; }
        }

        [FixedLength(32)]
        public class GameModeMarker
        {
            [PrimitiveValue(0)]
            public Vector3 Position { get; set; }
        }

        [FixedLength(144)]
        public class ItemCollectionPlacement
        {
            [PrimitiveValue(64)]
            public Vector3 Position { get; set; }

            [PrimitiveValue(76)]
            public Vector3 Orientation { get; set; }

            [PrimitiveValue(92)]
            public uint ItemCollectionId { get; set; }
        }

        [FixedLength(156)] public class Obj296 { }
        [FixedLength(16)]
        public class DecalInstance
        {
            [PrimitiveValue(0)]
            public ushort Index { get; set; }

            [PrimitiveValue(4)]
            public Vector3 Position { get; set; }
        }
        [FixedLength(8)] public class Obj320 { }
        [FixedLength(1)] public class Obj432 { }

        [FixedLength(128)] public class Obj472 { }

        [FixedLength(56)]
        public class Obj480
        {
            [PrimitiveValue(36)]
            public Vector3 Position { get; set; }
        }
        
        [FixedLength(64)]
        public class Obj488
        {
            [PrimitiveValue(36)]
            public Vector3 Position { get; set; }
        }
        

        [FixedLength(152)] public class Obj536 { }
        [FixedLength(2)] public class Obj560 { }
        [FixedLength(20)] public class Obj568 
        {
            [PrimitiveValue(0)]
            public ushort Index { get; set; }
        }
        [FixedLength(100)] public class Obj592 { }
        [FixedLength(72)] public class Obj600 { }
        [FixedLength(192)] public class Obj656 { }
        [FixedLength(816)] public class Obj792 { }

        [FixedLength(76)]
        public class BlocInstance
        {
            [PrimitiveValue(0)]
            public ushort BlocDefinitionIndex { get; set; }

            [PrimitiveValue(8)]
            public Vector3 Position { get; set; }

            [PrimitiveValue(20)]
            public Vector3 Orientation { get; set; }

        }

        [FixedLength(40)]
        public class BlocDefinition
        {
            [PrimitiveValue(4)]
            public uint BlocId { get; set; }
        }

        [FixedLength(16)] public class Obj840 { }

        [FixedLength(16)] public class Obj904 { }

        [FixedLength(3196)] public class Obj920 { }

        [FixedLength(4)] public class Obj984 { }

    }
}