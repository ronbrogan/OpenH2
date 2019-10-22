using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenH2.Core.Enums.Texture;
using OpenH2.Core.Offsets;
using OpenH2.Core.Parsing;
using OpenH2.Core.Tags.Layout;
using System;
using System.IO;
using System.IO.Compression;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.bitm)]
    public class BitmapTag : BaseTag
    {
        public override string Name { get; set; }

        public BitmapTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(0)]
        [JsonConverter(typeof(StringEnumConverter))]
        public TextureType TextureType { get; set; }

        [PrimitiveValue(2)]
        [JsonConverter(typeof(StringEnumConverter))]
        public TextureCompressionFormat TextureFormat { get; set; }

        [PrimitiveValue(4)]
        [JsonConverter(typeof(StringEnumConverter))]
        public TextureUsage TextureUsage { get; set; }

        [PrimitiveValue(52)]
        public short MipMapCount { get; set; }

        [StringValue(80, 4)]
        public string Tag { get; set; }

        [PrimitiveValue(84)]
        public short Width { get; set; }

        [PrimitiveValue(86)]
        public short Height { get; set; }

        [PrimitiveValue(88)]
        public short Depth { get; set; }

        [PrimitiveValue(90)]
        public short Type { get; set; }

        [PrimitiveValue(92)]
        public TextureFormat Format { get; set; }

        [PrimitiveValue(94)]
        public TextureProperties Properties { get; set; }

        [PrimitiveValue(96)]
        public short RegX { get; set; }

        [PrimitiveValue(98)]
        public short RegY { get; set; }

        [PrimitiveValue(100)]
        public short MipMapCount2 { get; set; }

        [PrimitiveValue(102)]
        public short PixelOffset { get; set; }
        
        [PrimitiveArray(108, 6)]
        public uint[] LodOffsets { get; set; }

        [PrimitiveArray(132, 6)]
        public uint[] LodSizes { get; set; }

        public BitmapLevelOfDetail[] LevelsOfDetail { get; set; }

        [PrimitiveValue(156)]
        public uint ID { get; set; }

        

        public class BitmapLevelOfDetail
        {
            public NormalOffset Offset { get; set; }

            public uint Size { get; set; }

            public Memory<byte> Data { get; set; } = Memory<byte>.Empty;
        }

        public override void PopulateExternalData(H2vReader sceneReader)
        {
            LevelsOfDetail = new BitmapLevelOfDetail[6];

            for (int i = 0; i < 6; i++)
            {
                var lod = new BitmapLevelOfDetail
                {
                    Offset = new NormalOffset((int)this.LodOffsets[i]),
                    Size = this.LodSizes[i]
                };

                if (lod.Offset.Value != 0 && lod.Offset.Value != int.MaxValue && lod.Size != 0)
                {
                    var inputStream = sceneReader.GetReader(lod.Offset).Data;
                    inputStream.Position = lod.Offset.Value + 2;

                    using (var decompress = new DeflateStream(inputStream, CompressionMode.Decompress, true))
                    using (var outputStream = new MemoryStream())
                    {
                        var copied = 0;
                        var buffer = new byte[81920];
                        var read = -1;

                        while (read != 0 && inputStream.Position < lod.Offset.Value + lod.Size)
                        {
                            read = decompress.Read(buffer, 0, 81920);
                            outputStream.Write(buffer, 0, read);

                            copied += read;
                        }

                        lod.Data = new Memory<byte>(outputStream.GetBuffer(), 0, (int)outputStream.Length);
                    }
                }

                LevelsOfDetail[i] = lod;
            }
        }
    }
}
