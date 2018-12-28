namespace OpenH2.Core.Tags
{
    public class Scenario : BaseTag
    {
        public Scenario(uint id) : base(id)
        {
        }

        public override string Name { get; set; }

        public byte[] RawMeta { get; set; }

        // TODO implement sky tag
        public uint[] SkyboxIds { get; set; }
        public BaseTag[] Skybox { get; set; }

        public Terrain[] Terrains { get; set; }

        public class Terrain
        {
            public uint BspId { get; set; }
            public Bsp Bsp { get; set; }

            // TODO implement lightmap tag
            public uint LightmapId { get; set; }
            public BaseTag Lightmap { get; set; }
        }
    }
}
