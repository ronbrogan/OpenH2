using System;
using OpenH2.Core.Enums.Texture;
using OpenH2.Core.Offsets;

namespace OpenH2.Core.Tags
{
    public class Bitmap : BaseTag
    {
        public Bitmap(uint id) : base(id)
        {
        }

        public TextureType TextureType { get; set; }

        public TextureFormat TextureFormat { get; set; }

        public TextureUsage TextureUsage { get; set; }

        public short MipMapCount { get; set; }

        public string Tag { get; set; }

        public short Width { get; set; }

        public short Height { get; set; }

        public short Depth { get; set; }

        public short Type { get; set; }

        public short Format { get; set; }

        public TextureProperties Properties { get; set; }

        public short RegX { get; set; }

        public short RegY { get; set; }

        public short MipMapCount2 { get; set; }

        public short PixelOffset { get; set; }

        public BitmapLevelOfDetail[] LevelsOfDetail { get; set; }

        public uint ID { get; set; }

        public override string Name { get; set; }

        public class BitmapLevelOfDetail
        {
            public NormalOffset Offset { get; set; }

            public uint Size { get; set; }

            public Memory<byte> Data { get; set; } = Memory<byte>.Empty;
        }
    }
}
