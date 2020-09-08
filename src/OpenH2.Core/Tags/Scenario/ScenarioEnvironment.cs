using OpenH2.Core.Representations;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;
using System.Numerics;

namespace OpenH2.Core.Tags.Scenario
{
    public partial class ScenarioTag
    {
        [FixedLength(8)]
        public class SkyboxInstance
        {
            [PrimitiveValue(4)]
            public TagRef<SkyboxTag> Skybox { get; set; }
        }

        [FixedLength(68)]
        public class Terrain
        {
            [PrimitiveValue(20)]
            public TagRef<BspTag> Bsp { get; set; }

            // TODO implement lightmap tag
            [PrimitiveValue(28)]
            public uint LightmapId { get; set; }
        }

        [FixedLength(92)]
        public class SceneryInstance : Entity
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
            public TagRef<SceneryTag> Scenery { get; set; }
        }

        [FixedLength(16)]
        public class DecalInstance
        {
            [PrimitiveValue(0)]
            public ushort Index { get; set; }

            [PrimitiveValue(4)]
            public Vector3 Position { get; set; }
        }

        [FixedLength(8)]
        public class DecalDefinition
        {
            [PrimitiveValue(4)]
            public TagRef DecalReference { get; set; }
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
            public TagRef<MachineryTag> Machinery { get; set; }
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

        [FixedLength(40)]
        public class SoundSceneryDefinition
        {
            [PrimitiveValue(4)]
            public TagRef<SoundSceneryTag> SoundScenery { get; set; }
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

        [FixedLength(40)]
        public class LightDefinition
        {
            [PrimitiveValue(4)]
            public TagRef LightTag { get; set; }
        }

        [FixedLength(16)]
        public class FogDefinition
        {
            [PrimitiveValue(0)]
            public byte ValueA { get; set; }

            [PrimitiveValue(1)]
            public byte ValueB { get; set; }

            [PrimitiveValue(2)]
            public short ValueC { get; set; }

            [PrimitiveValue(8)]
            public TagRef FogRef { get; set; }

            [PrimitiveValue(12)]
            public uint ValueD { get; set; }
        }
    }
}
