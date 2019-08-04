using OpenH2.Core.Tags.Layout;

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
        [InternalReferenceValue(104)] public Obj104[] Obj104s { get; set; }
        [InternalReferenceValue(144)] public Obj144[] AmmoPickupInstances { get; set; }
        [InternalReferenceValue(152)] public Obj152[] Obj152s { get; set; }
        [InternalReferenceValue(160)] public Obj160[] Obj160s { get; set; }
        [InternalReferenceValue(168)] public Obj168[] Obj168s { get; set; }
        [InternalReferenceValue(184)] public Obj184[] Obj184s { get; set; }
        [InternalReferenceValue(192)] public Obj192[] Obj192s { get; set; }

        [InternalReferenceValue(176)] public Obj176[] Obj176s { get; set; }

        [InternalReferenceValue(216)] public SoundSceneryInstance[] SoundSceneryInstances { get; set; }

        [InternalReferenceValue(224)] public Obj224[] Obj224s { get; set; }

        [InternalReferenceValue(232)] public LightInstance[] LightInstances { get; set; }

        [InternalReferenceValue(240)] public Obj240[] Obj240s { get; set; }

        [InternalReferenceValue(256)] public PlayerSpawnMarker[] PlayerSpawnMarkers { get; set; }

        [InternalReferenceValue(264)] public Obj264[] Obj264s { get; set; }

        [InternalReferenceValue(280)] public GameModeMarker[] GameModeMarkers { get; set; }

        [InternalReferenceValue(288)] public ItemCollectionPlacement[] ItemCollectionPlacements { get; set; }

        [InternalReferenceValue(296)] public Obj296[] Obj296s { get; set; }

        [InternalReferenceValue(312)] public Obj312[] Obj312s { get; set; }



        [InternalReferenceValue(320)] public Obj320[] Obj320s { get; set; }

        [InternalReferenceValue(432)] public Obj432[] Obj432s { get; set; }

        [InternalReferenceValue(472)] public Obj472[] Obj472s { get; set; }


        [InternalReferenceValue(528)]
        public Terrain[] Terrains { get; set; }

        [InternalReferenceValue(536)] public Obj536[] Obj536s { get; set; }

        [InternalReferenceValue(560)] public Obj560[] Obj560s { get; set; }

        [InternalReferenceValue(568)] public Obj568[] Obj568s { get; set; }

        [InternalReferenceValue(592)] public Obj592[] Obj592s { get; set; }

        [InternalReferenceValue(600)] public Obj600[] Obj600s { get; set; }

        [InternalReferenceValue(656)] public Obj656[] Obj656s { get; set; }

        [InternalReferenceValue(792)] public Obj792[] Obj792s { get; set; }

        [InternalReferenceValue(808)] public BlocInstance[] BlocInstances { get; set; }

        [InternalReferenceValue(816)] public BlocDefinition[] BlocDefinitions { get; set; }

        [InternalReferenceValue(840)] public Obj840[] Obj840s { get; set; }

        [InternalReferenceValue(904)] public Obj904[] Obj904s { get; set; }

        [InternalReferenceValue(920)] public Obj920[] Obj920s { get; set; }

        [InternalReferenceValue(984)] public Obj984[] Obj984s { get; set; }


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
            public float X { get; set; }

            [PrimitiveValue(12)]
            public float Y { get; set; }

            [PrimitiveValue(16)]
            public float Z { get; set; }

            [PrimitiveValue(20)]
            public float Yaw { get; set; }

            [PrimitiveValue(24)]
            public float Pitch { get; set; }

            [PrimitiveValue(28)]
            public float Roll { get; set; }
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
            [PrimitiveValue(8)]
            public float X { get; set; }

            [PrimitiveValue(12)]
            public float Y { get; set; }

            [PrimitiveValue(16)]
            public float Z { get; set; }
        }
        [FixedLength(40)] public class Obj104 { }
        [FixedLength(84)]
        public class Obj144
        {
            [PrimitiveValue(8)]
            public float X { get; set; }

            [PrimitiveValue(12)]
            public float Y { get; set; }

            [PrimitiveValue(16)]
            public float Z { get; set; }
        }
        [FixedLength(40)] public class Obj160 { }
        [FixedLength(68)]
        public class Obj184
        {
            [PrimitiveValue(8)]
            public float X { get; set; }

            [PrimitiveValue(12)]
            public float Y { get; set; }

            [PrimitiveValue(16)]
            public float Z { get; set; }
        }
        [FixedLength(40)] public class Obj192 { }


        [FixedLength(40)] public class Obj152 { }

        [FixedLength(72)]
        public class Obj168
        {
            [PrimitiveValue(8)]
            public float X { get; set; }

            [PrimitiveValue(12)]
            public float Y { get; set; }

            [PrimitiveValue(16)]
            public float Z { get; set; }

            [PrimitiveValue(20)]
            public float I { get; set; }

            [PrimitiveValue(24)]
            public float J { get; set; }

            [PrimitiveValue(28)]
            public float K { get; set; }

        }
        [FixedLength(40)]
        public class Obj176
        {

        }

        [FixedLength(80)]
        public class SoundSceneryInstance
        {
            [PrimitiveValue(8)]
            public float X { get; set; }

            [PrimitiveValue(12)]
            public float Y { get; set; }

            [PrimitiveValue(16)]
            public float Z { get; set; }

            [PrimitiveValue(20)]
            public float I { get; set; }

            [PrimitiveValue(24)]
            public float J { get; set; }

            [PrimitiveValue(28)]
            public float K { get; set; }
        }

        [FixedLength(40)] public class Obj224 { }

        [FixedLength(108)]
        public class LightInstance
        {
            [PrimitiveValue(8)]
            public float X { get; set; }

            [PrimitiveValue(12)]
            public float Y { get; set; }

            [PrimitiveValue(16)]
            public float Z { get; set; }

            [PrimitiveValue(20)]
            public float I { get; set; }

            [PrimitiveValue(24)]
            public float J { get; set; }

            [PrimitiveValue(28)]
            public float K { get; set; }
        }

        [FixedLength(40)] public class Obj240 { }

        [FixedLength(52)]
        public class PlayerSpawnMarker
        {
            [PrimitiveValue(0)]
            public float X { get; set; }

            [PrimitiveValue(4)]
            public float Y { get; set; }

            [PrimitiveValue(8)]
            public float Z { get; set; }
        }

        [FixedLength(68)]
        public class Obj264
        {
            [PrimitiveValue(12)]
            public float X { get; set; }

            [PrimitiveValue(16)]
            public float Y { get; set; }

            [PrimitiveValue(20)]
            public float Z { get; set; }
        }

        [FixedLength(32)]
        public class GameModeMarker
        {
            [PrimitiveValue(0)]
            public float X { get; set; }

            [PrimitiveValue(4)]
            public float Y { get; set; }

            [PrimitiveValue(8)]
            public float Z { get; set; }
        }

        [FixedLength(144)]
        public class ItemCollectionPlacement
        {
            [PrimitiveValue(64)]
            public float X { get; set; }

            [PrimitiveValue(68)]
            public float Y { get; set; }

            [PrimitiveValue(72)]
            public float Z { get; set; }

            [PrimitiveValue(76)]
            public float Yaw { get; set; }

            [PrimitiveValue(80)]
            public float Pitch { get; set; }

            [PrimitiveValue(84)]
            public float Roll { get; set; }

            [PrimitiveValue(92)]
            public uint ItemCollectionId { get; set; }
        }

        [FixedLength(156)] public class Obj296 { }
        [FixedLength(16)]
        public class Obj312
        {
            [PrimitiveValue(0)]
            public ushort Index { get; set; }

            [PrimitiveValue(4)]
            public float X { get; set; }

            [PrimitiveValue(8)]
            public float Y { get; set; }

            [PrimitiveValue(12)]
            public float Z { get; set; }
        }
        [FixedLength(8)] public class Obj320 { }
        [FixedLength(1)] public class Obj432 { }
        [FixedLength(128)] public class Obj472 { }
        [FixedLength(152)] public class Obj536 { }
        [FixedLength(2)] public class Obj560 { }
        [FixedLength(20)] public class Obj568 { }
        [FixedLength(100)] public class Obj592 { }
        [FixedLength(72)] public class Obj600 { }
        [FixedLength(192)] public class Obj656 { }
        [FixedLength(816)] public class Obj792 { }
        [FixedLength(76)]
        public class BlocInstance
        {
            [PrimitiveValue(0)]
            public ushort SceneryDefinitionIndex { get; set; }

            [PrimitiveValue(8)]
            public float X { get; set; }

            [PrimitiveValue(12)]
            public float Y { get; set; }

            [PrimitiveValue(16)]
            public float Z { get; set; }

            [PrimitiveValue(20)]
            public float Yaw { get; set; }

            [PrimitiveValue(24)]
            public float Pitch { get; set; }

            [PrimitiveValue(28)]
            public float Roll { get; set; }
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