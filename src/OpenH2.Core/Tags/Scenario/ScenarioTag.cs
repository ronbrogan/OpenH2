using OpenH2.Core.Representations;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;
using System.Numerics;
using System.Text.Json.Serialization;

namespace OpenH2.Core.Tags.Scenario
{
    [TagLabel(TagName.scnr)]
    public partial class ScenarioTag : BaseTag
    {
        public override string Name { get; set; }
        public ScenarioTag(uint id) : base(id)
        {
        }

        
        [ReferenceArray(96)] public Obj96[] Obj96s { get; set; }
        [ReferenceArray(296)] public Obj296[] Obj296s { get; set; }
        [ReferenceArray(304)] public Obj304[] Obj304s { get; set; }
        [ReferenceArray(360)] public Obj360_String[] Obj360s_Locations { get; set; }
        [ReferenceArray(368)] public Obj368[] Obj368s { get; set; }
        [ReferenceArray(536)] public Obj536[] Obj536s { get; set; }
        [ReferenceArray(560)] public Obj560[] Obj560s { get; set; }        
        [ReferenceArray(592)] public Obj592[] Obj592s { get; set; }
        [ReferenceArray(600)] public Obj600[] Obj600s { get; set; }
        [ReferenceArray(656)] public Obj656[] Obj656s { get; set; }
        [ReferenceArray(792)] public Obj792[] Obj792s { get; set; }
        [ReferenceArray(832)] public Obj832[] Obj832s { get; set; }
        [ReferenceArray(896)] public Obj896[] Obj896s { get; set; }
        [ReferenceArray(904)] public Obj904[] Obj904s { get; set; }
        [ReferenceArray(920)] public Obj920[] Obj920s { get; set; }




        [ReferenceArray(8)] public SkyboxInstance[] SkyboxInstances { get; set; }
        [ReferenceArray(72)] public WellKnownItem[] WellKnownItems { get; set; }
        [ReferenceArray(80)] public SceneryInstance[] SceneryInstances { get; set; }
        [ReferenceArray(88)] public SceneryDefinition[] SceneryDefinitions { get; set; }
        [ReferenceArray(104)] public BipedDefinition[] BipedDefinitions { get; set; }
        [ReferenceArray(112)] public VehicleInstance[] VehicleInstances { get; set; }
        [ReferenceArray(120)] public VehicleDefinition[] VehicleDefinitions { get; set; }
        [ReferenceArray(128)] public EquipmentPlacement[] EquipmentPlacements { get; set; }
        [ReferenceArray(136)] public EquipmentDefinition[] EquipmentDefinitions { get; set; }
        [ReferenceArray(144)] public WeaponPlacement[] WeaponPlacements { get; set; }
        [ReferenceArray(152)] public WeaponDefinition[] WeaponDefinitions { get; set; }
        [ReferenceArray(160)] public DeviceGroupDefinition[] DeviceGroupDefinitions { get; set; }
        [ReferenceArray(168)] public MachineryInstance[] MachineryInstances { get; set; }
        [ReferenceArray(176)] public MachineryDefinition[] MachineryDefinitions { get; set; }
        [ReferenceArray(184)] public ControllerInstance[] ControllerInstances { get; set; }
        [ReferenceArray(192)] public ControllerDefinition[] ControllerDefinitions { get; set; }
        [ReferenceArray(216)] public SoundSceneryInstance[] SoundSceneryInstances { get; set; }
        [ReferenceArray(224)] public SoundSceneryDefinition[] SoundSceneryDefinitions { get; set; }
        [ReferenceArray(232)] public LightInstance[] LightInstances { get; set; }
        [ReferenceArray(240)] public LightDefinition[] LightDefinitions { get; set; }
        [ReferenceArray(248)] public StartingProfileDefinition[] StartingProfileDefinitions { get; set; }
        [ReferenceArray(256)] public PlayerSpawnMarker[] PlayerSpawnMarkers { get; set; }
        [ReferenceArray(264)] public TriggerVolume[] TriggerVolumes { get; set; }
        [ReferenceArray(280)] public GameModeMarker[] GameModeMarkers { get; set; }
        [ReferenceArray(288)] public ItemCollectionPlacement[] ItemCollectionPlacements { get; set; }
        [ReferenceArray(312)] public DecalInstance[] DecalInstances { get; set; }
        [ReferenceArray(320)] public DecalDefinition[] DecalDefinitions { get; set; }
        [ReferenceArray(336)] public StyleDefinition[] StyleDefinitions { get; set; }
        [ReferenceArray(344)] public AiSquadGroupDefinition[] AiSquadGroupDefinitions { get; set; }
        [ReferenceArray(352)] public AiSquadDefinition[] AiSquadDefinitions { get; set; }
        [ReferenceArray(376)] public CharacterDefinition[] CharacterDefinitions { get; set; }
        
        [ReferenceArray(432), JsonIgnore] public byte[] ScriptStrings { get; set; }
        [ReferenceArray(440)] public ScriptMethodDefinition[] ScriptMethods { get; set; }
        [ReferenceArray(448)] public ScriptVariableDefinition[] ScriptVariables { get; set; }
        [ReferenceArray(456)] public SoundDefinition[] SoundDefinitions { get; set; }

        [ReferenceArray(472)] public SpatialPointStuff[] SpatialPointStuffs { get; set; }
        [ReferenceArray(480)] public LocationFlagDefinition[] LocationFlagDefinitions { get; set; }
        [ReferenceArray(488)] public CameraPathTarget[] CameraPathTargets { get; set; }
        [ReferenceArray(496)] public CinematicTitleDefinition[] CinematicTitleDefinitions { get; set; }
        [ReferenceArray(528)] public Terrain[] Terrains { get; set; }
        [ReferenceArray(552)] public VehicleReference[] VehicleReferences { get; set; }
        [ReferenceArray(568), JsonIgnore] public ScriptSyntaxNode[] ScriptSyntaxNodes { get; set; }
        [ReferenceArray(576)] public AiOrderDefinition[] AiOrderDefinitions { get; set; }
        [ReferenceArray(584)] public AiNamedTrigger[] AiNamedTriggers { get; set; }
        [ReferenceArray(808)] public BlocInstance[] BlocInstances { get; set; }
        [ReferenceArray(816)] public BlocDefinition[] BlocDefinitions { get; set; }
        [ReferenceArray(840)] public FogDefinition[] FogDefinitions { get; set; }
        [ReferenceArray(888)] public DecrDefinition[] DecrDefinitions { get; set; }
        [ReferenceArray(944)] public MdlgDefinition[] MdlgDefinitions { get; set; }
        //[ReferenceArray(984)] public uint[] FreeSpace { get; set; }


        [FixedLength(36)]
        public class WellKnownItem
        {
            [StringValue(0, 32)]
            public string Description { get; set; }

            [PrimitiveValue(32)]
            public WellKnownVarType ItemType { get; set; }

            [PrimitiveValue(34)]
            public ushort Index { get; set; }
        }

        public enum WellKnownVarType : ushort
        {
            Unit = 0,
            Vehicle = 1,
            Weapon = 2,

            Scenery = 6,
            Machinery = 7, // As Device
            Controller = 8 // As Device
        }

        // TODO: placement test
        [FixedLength(84)]
        public class VehicleInstance
        {
            [PrimitiveValue(0)]
            public ushort Index { get; set; }

            [PrimitiveValue(8)]
            public Vector3 Position { get; set; }

            [PrimitiveValue(20)]
            public Vector3 Orientation { get; set; }
        }

        // TODO: placement test
        [FixedLength(56)]
        public class EquipmentPlacement
        {
            [PrimitiveValue(0)]
            public ushort Index { get; set; }

            [PrimitiveValue(8)]
            public Vector3 Position { get; set; }

            [PrimitiveValue(20)]
            public Vector3 Orientation { get; set; }
        }

        // TODO: placement test
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
        public class DeviceGroupDefinition
        {
            [StringValue(0, 32)]
            public string Description { get; set; }

            [PrimitiveValue(32)]
            public float Value { get; set; }
        }

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

        

        [FixedLength(68)]
        public class TriggerVolume
        {
            [InternedString(0)]
            public string Description { get; set; }

            [PrimitiveValue(4)]
            public uint Unknown { get; set; }

            [InternedString(8)]
            public string String2 { get; set; }

            [PrimitiveValue(16)]
            public Vector3 Orientation { get; set; }

            [PrimitiveValue(28)]
            public Vector2 Something { get; set; }

            [PrimitiveValue(36)]
            public Vector3 Position { get; set; }

            [PrimitiveValue(48)]
            public Vector3 Size { get; set; }

            [PrimitiveValue(60)]
            public float Param { get; set; }

            [PrimitiveValue(64)]
            public uint Unknown2 { get; set; }
        }

        [FixedLength(32)]
        public class GameModeMarker
        {
            [PrimitiveValue(0)]
            public Vector3 Position { get; set; }
        }

        // only in MP?
        [FixedLength(156)] public class Obj296 { }

        [FixedLength(14)]
        public class Obj304
        {
            [PrimitiveValue(0)]
            public ushort Index1 { get; set; }

            [PrimitiveValue(2)]
            public ushort Index2 { get; set; }

            [PrimitiveValue(4)]
            public ushort Index3 { get; set; }
        }

        [FixedLength(8)]
        public class StyleDefinition
        {
            [PrimitiveValue(4)]
            public TagRef StyleReference { get; set; }
        }

        [FixedLength(84)] 
        public class Obj368 
        {
            [PrimitiveValue(0)]
            public short Val1 { get; set; }

            [PrimitiveValue(2)]
            public short Val2 { get; set; }

            //[ReferenceArray(16)]
            public Obj16[] Obj16s { get; set; }

            [PrimitiveValue(24)]
            public short Val3 { get; set; }

            [PrimitiveValue(26)]
            public short Val4 { get; set; }

            //[ReferenceArray(32)]
            public Obj32[] Obj32s { get; set; }

            //[ReferenceArray(40)]
            public Obj40[] Obj40s { get; set; }

            // More here

            [FixedLength(24)]
            public class Obj16
            {
                
            }

            [FixedLength(20)]
            public class Obj32
            {
                
            }

            [FixedLength(20)]
            public class Obj40
            {
                
            }
        }


        [FixedLength(8)]
        public class CharacterDefinition
        {
            [PrimitiveValue(4)]
            public TagRef CharacterReference { get; set; }
        }

        [FixedLength(8)]
        public class SoundDefinition
        {
            [PrimitiveValue(36)]
            public TagRef<SoundTag> Sound { get; set; }
        }

        [FixedLength(128)]
        public class SpatialPointStuff
        {
            [ReferenceArray(0)]
            public SpatialPointCollection[] SpatialPointCollections { get; set; }

            [FixedLength(48)]
            public class SpatialPointCollection
            {
                [StringValue(0, 32)]
                public string Description { get; set; }

                [ReferenceArray(32)]
                public SpatialPointDefinition[] SpatialPoints { get; set; }


                [FixedLength(60)]
                public class SpatialPointDefinition
                {
                    [StringValue(0, 32)]
                    public string Description { get; set; }

                    [PrimitiveValue(32)]
                    public Vector3 Position { get; set; }

                    [PrimitiveValue(48)]
                    public short Value { get; set; }

                    [PrimitiveValue(52)]
                    public Vector2 Unknown { get; set; }
                }
            }
        }

        [FixedLength(56)]
        public class LocationFlagDefinition
        {
            [PrimitiveValue(0)]
            public uint Value { get; set; }

            [StringValue(4, 32)]
            public string Description { get; set; }

            [PrimitiveValue(36)]
            public Vector3 Position { get; set; }

            [PrimitiveValue(48)]
            public float Param { get; set; }
        }

        [FixedLength(64)]
        public class CameraPathTarget
        {
            [PrimitiveValue(0)]
            public uint Value { get; set; }

            [StringValue(4, 32)]
            public string Description { get; set; }

            [PrimitiveValue(36)]
            public Vector3 Position { get; set; }

            [PrimitiveValue(48)]
            public Vector3 Orientation { get; set; }
        }


        [FixedLength(36)]
        public class CinematicTitleDefinition
        {
            [InternedString(0)]
            public string Title { get; set; }

            [PrimitiveValue(4)]
            public ushort OneHundred { get; set; }

            [PrimitiveValue(6)]
            public ushort Ten24 { get; set; }

            [PrimitiveValue(8)]
            public ushort ValueA { get; set; }

            [PrimitiveValue(10)]
            public ushort ValueB { get; set; }

            [PrimitiveValue(12)]
            public ushort ValueC { get; set; }

            [PrimitiveValue(14)]
            public ushort ValueD { get; set; }

            [PrimitiveValue(24)]
            public Vector3 Floats { get; set; }
        }


        [FixedLength(24)]
        public class Obj536
        {
            [ReferenceArray(0)]
            public WildcardTagReference[] Obj0s { get; set; }

            [ReferenceArray(8)]
            public WildcardTagReference[] Obj8s { get; set; }

            [ReferenceArray(16)]
            public WildcardTagReference[] Obj16s { get; set; }

            [FixedLength(8)]
            public class WildcardTagReference
            {
                [StringValue(0, 4)]
                public string TagType { get; set; }

                [PrimitiveValue(4)]
                public TagRef Tag { get; set; }
            }
        }

        [FixedLength(8)]
        public class VehicleReference
        {
            [PrimitiveValue(0)]
            public TagRef Tag { get; set; }

            [PrimitiveValue(4)]
            public uint Value { get; set; }
        }

        [FixedLength(2)]
        public class Obj560
        {
            [PrimitiveValue(0)]
            public ushort Index { get; set; }
        }

        

        [FixedLength(124)]
        public class AiOrderDefinition
        {
            [StringValue(0, 32)]
            public string Description { get; set; }

            [PrimitiveValue(32)]
            public uint ValueA { get; set; }

            [PrimitiveValue(40)]
            public uint ValueB { get; set; }

            [ReferenceArray(84)]
            public Obj84[] Obj84s { get; set; }

            [ReferenceArray(116)]
            public Obj116[] Obj116s { get; set; }


            [FixedLength(60)]
            public class Obj84
            {

            }

            [FixedLength(36)]
            public class Obj116
            {

            }
        }

        [FixedLength(48)]
        public class AiNamedTrigger
        {
            [StringValue(0, 32)]
            public string Description { get; set; }

            [ReferenceArray(40)]
            public Obj40[] Obj40s { get; set; }


            [FixedLength(56)]
            public class Obj40
            {
                [PrimitiveValue(0)]
                public ushort Index1 { get; set; }

                [PrimitiveValue(6)]
                public ushort Index2 { get; set; }
            }
        }


        [FixedLength(100)]
        public class Obj592
        {
            [StringValue(0, 32)]
            public string Description { get; set; }

            [PrimitiveValue(36)]
            public TagRef<LoopingSoundTag> LoopingSound1 { get; set; }

            [PrimitiveValue(44)]
            public TagRef<LoopingSoundTag> LoopingSound2 { get; set; }

            [PrimitiveValue(68)]
            public float Param1 { get; set; }

            [PrimitiveValue(88)]
            public float Param2 { get; set; }
        }

        [FixedLength(72)]
        public class Obj600
        {
            [StringValue(0, 32)]
            public string Description { get; set; }

            [PrimitiveValue(36)]
            public TagRef SndeReference { get; set; }

            [PrimitiveValue(40)]
            public float Param1 { get; set; }

            [PrimitiveValue(44)]
            public float Param2 { get; set; }
        }

        [FixedLength(52)]
        public class Obj656
        {
            [PrimitiveValue(4)]
            public TagRef<BspTag> Bsp { get; set; }

            [ReferenceArray(8)]
            public uint[] ValueAs { get; set; }

            [ReferenceArray(16)]
            public uint[] ValueBs { get; set; }

            [PrimitiveValue(24)]
            public ushort Param1 { get; set; }

            [PrimitiveValue(26)]
            public ushort Param2 { get; set; }

            [ReferenceArray(28)]
            public uint[] ValueCs { get; set; }

            [ReferenceArray(36)]
            public uint[] ValueDs { get; set; }

            [ReferenceArray(44)]
            public uint[] ValueEs { get; set; }
        }

        // MP only?
        [FixedLength(96)]
        public class Obj792
        {
            [PrimitiveValue(0)]
            public uint Zero { get; set; }
        }

        [FixedLength(244)]
        public class Obj832
        {
            [PrimitiveValue(0)]
            public ushort ValueA { get; set; }
            [PrimitiveValue(2)]
            public ushort ValueB { get; set; }

            [PrimitiveArray(4, 9)]
            public float[] FloatsA { get; set; }

            [PrimitiveArray(136, 10)]
            public float[] FloatsB { get; set; }

            [PrimitiveValue(212)]
            public TagRef FpchRef { get; set; }
        }

        

        [FixedLength(8)]
        public class DecrDefinition
        {
            [PrimitiveValue(4)]
            public TagRef DecrRef { get; set; }
        }

        [FixedLength(8)]
        public class Obj896
        {
            [PrimitiveValue(0)]
            public ushort ValueA { get; set; }

            [PrimitiveValue(2)]
            public ushort ValueB { get; set; }

            [PrimitiveValue(4)]
            public uint ValueC { get; set; }
        }

        [FixedLength(16)]
        public class Obj904
        {
            [PrimitiveValue(4)]
            public TagRef<BspTag> BspRef { get; set; }

            // TODO: using Vector4[] for ReferenceArray isn't supported
            //[ReferenceArray(8)]
            public Vector4[] Params { get; set; }
        }

        [FixedLength(24)]
        // TODO: the localized map name and description are in here, after the ref array
        public class Obj920
        {
            [PrimitiveValue(4)]
            public TagRef UnicRef { get; set; }

            [ReferenceArray(8)]
            public Obj8[] Obj8s { get; set; }

            [FixedLength(16)]
            public class Obj8
            {
                [PrimitiveValue(12)]
                public TagRef<BitmapTag> BitmapRef { get; set; }
            }
        }

        [FixedLength(8)]
        public class MdlgDefinition
        {
            [PrimitiveValue(4)]
            public TagRef MdlgRef { get; set; }
        }
    }
}