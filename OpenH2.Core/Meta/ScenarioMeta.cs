namespace OpenH2.Core.Meta
{
    public class ScenarioMeta : BaseMeta
    {
        public override string Name { get; set; }

        public byte[] RawMeta { get; set; }

        // TODO implement sky tag
        public uint[] SkyboxIds { get; set; }
        public BaseMeta[] Skybox { get; set; }

        public Terrain[] Terrains { get; set; }

        public class Terrain
        {
            public uint BspId { get; set; }
            public BspMeta Bsp { get; set; }

            // TODO implement lightmap tag
            public uint LightmapId { get; set; }
            public BaseMeta Lightmap { get; set; }
        }
    }
}
