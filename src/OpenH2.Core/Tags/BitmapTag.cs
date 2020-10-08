using OpenH2.Core.Enums.Texture;
using OpenH2.Core.Offsets;
using OpenH2.Core.Parsing;
using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json.Serialization;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.bitm)]
    public class BitmapTag : BaseTag, IEquatable<BitmapTag>
    {
        public override string Name { get; set; }

        public BitmapTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(0)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TextureType TextureType { get; set; }

        [PrimitiveValue(2)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TextureCompressionFormat TextureFormat { get; set; }

        [PrimitiveValue(4)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TextureUsage TextureUsage { get; set; }

        [PrimitiveValue(52)]
        public short MipMapCount { get; set; }

        [ReferenceArray(60)]
        public AtlasInfo[] AtlasInfos { get; set; }

        [ReferenceArray(68)]
        public TextureInfo[] TextureInfos { get; set; }

        [FixedLength(60)]
        public class AtlasInfo
        {
            [ReferenceArray(52)]
            public SubImageInfo[] SubImages { get; set; }

            [FixedLength(32)]
            public class SubImageInfo
            {
                [PrimitiveValue(0)]
                public float A { get; set; }

                [PrimitiveValue(4)]
                public float B { get; set; }

                [PrimitiveValue(8)]
                public float C { get; set; }

                [PrimitiveValue(12)]
                public float D { get; set; }

                [PrimitiveValue(16)]
                public float E { get; set; }

                [PrimitiveValue(20)]
                public float F { get; set; }

                [PrimitiveValue(24)]
                public float G { get; set; }

                [PrimitiveValue(28)]
                public float H { get; set; }
            }
        }

        [FixedLength(116)]
        public class TextureInfo
        {
            [StringValue(0, 4)]
            public string Tag { get; set; }

            [PrimitiveValue(4)]
            public short Width { get; set; }

            [PrimitiveValue(6)]
            public short Height { get; set; }

            [PrimitiveValue(8)]
            public short Depth { get; set; }

            [PrimitiveValue(10)]
            public short Type { get; set; }

            [PrimitiveValue(12)]
            public TextureFormat Format { get; set; }

            [PrimitiveValue(14)]
            public TextureProperties Properties { get; set; }

            [PrimitiveValue(16)]
            public short RegX { get; set; }

            [PrimitiveValue(18)]
            public short RegY { get; set; }

            [PrimitiveValue(20)]
            public short MipMapCount2 { get; set; }

            [PrimitiveValue(22)]
            public short PixelOffset { get; set; }

            [PrimitiveArray(28, 6)]
            public uint[] LodOffsets { get; set; }

            [PrimitiveArray(52, 6)]
            public uint[] LodSizes { get; set; }

            public BitmapLevelOfDetail[] LevelsOfDetail { get; set; }

            [PrimitiveValue(76)]
            public uint ID { get; set; }
        }

        public class BitmapLevelOfDetail
        {
            public NormalOffset Offset { get; set; }

            public uint Size { get; set; }

            [JsonIgnore]
            public Memory<byte> Data { get; set; } = Memory<byte>.Empty;
        }

        public override void PopulateExternalData(H2MapReader sceneReader)
        {
            foreach(var info in this.TextureInfos)
            {
                info.LevelsOfDetail = new BitmapLevelOfDetail[6];

                for (int i = 0; i < 6; i++)
                {
                    var lod = new BitmapLevelOfDetail
                    {
                        Offset = new NormalOffset((int)info.LodOffsets[i]),
                        Size = info.LodSizes[i]
                    };

                    if (lod.Offset.Value != 0 && lod.Offset.Value != int.MaxValue && lod.Size != 0)
                    {
                        var reader = sceneReader.GetReader(lod.Offset);

                        // Can be null if non-local readers aren't setup
                        if(reader != null)
                        {
                            var inputStream = reader.Data;
                            inputStream.Position = lod.Offset.Value + 2;

                            using (var decompress = new DeflateStream(inputStream, CompressionMode.Decompress, true))
                            using (var outputStream = new MemoryStream())
                            {
                                var buffer = new byte[81920];
                                var read = -1;

                                var endOfInput = lod.Offset.Value + lod.Size;

                                while (read != 0)
                                {
                                    read = decompress.Read(buffer, 0, 81920);
                                    outputStream.Write(buffer, 0, read);
                                }

                                lod.Data = new Memory<byte>(outputStream.GetBuffer(), 0, (int)outputStream.Length);
                            }
                        }
                    }

                    info.LevelsOfDetail[i] = lod;
                }
            }
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as BitmapTag);
        }

        public bool Equals(BitmapTag other)
        {
            return other != null &&
                   this.Name == other.Name &&
                   this.TextureType == other.TextureType &&
                   this.TextureFormat == other.TextureFormat &&
                   this.TextureUsage == other.TextureUsage &&
                   this.MipMapCount == other.MipMapCount &&
                   this.TextureInfos[0].Tag == other.TextureInfos[0].Tag &&
                   this.TextureInfos[0].Width == other.TextureInfos[0].Width &&
                   this.TextureInfos[0].Height == other.TextureInfos[0].Height &&
                   this.TextureInfos[0].Depth == other.TextureInfos[0].Depth &&
                   this.TextureInfos[0].Type == other.TextureInfos[0].Type &&
                   this.TextureInfos[0].Format == other.TextureInfos[0].Format &&
                   this.TextureInfos[0].Properties == other.TextureInfos[0].Properties &&
                   this.TextureInfos[0].RegX == other.TextureInfos[0].RegX &&
                   this.TextureInfos[0].RegY == other.TextureInfos[0].RegY &&
                   this.TextureInfos[0].MipMapCount2 == other.TextureInfos[0].MipMapCount2 &&
                   this.TextureInfos[0].PixelOffset == other.TextureInfos[0].PixelOffset &&
                   this.TextureInfos[0].ID == other.TextureInfos[0].ID;
        }

        public override int GetHashCode()
        {
            var hashCode = 1071719634;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Name);
            hashCode = hashCode * -1521134295 + this.TextureType.GetHashCode();
            hashCode = hashCode * -1521134295 + this.TextureFormat.GetHashCode();
            hashCode = hashCode * -1521134295 + this.TextureUsage.GetHashCode();
            hashCode = hashCode * -1521134295 + this.MipMapCount.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.TextureInfos[0].Tag);
            hashCode = hashCode * -1521134295 + this.TextureInfos[0].Width.GetHashCode();
            hashCode = hashCode * -1521134295 + this.TextureInfos[0].Height.GetHashCode();
            hashCode = hashCode * -1521134295 + this.TextureInfos[0].Depth.GetHashCode();
            hashCode = hashCode * -1521134295 + this.TextureInfos[0].Type.GetHashCode();
            hashCode = hashCode * -1521134295 + this.TextureInfos[0].Format.GetHashCode();
            hashCode = hashCode * -1521134295 + this.TextureInfos[0].Properties.GetHashCode();
            hashCode = hashCode * -1521134295 + this.TextureInfos[0].RegX.GetHashCode();
            hashCode = hashCode * -1521134295 + this.TextureInfos[0].RegY.GetHashCode();
            hashCode = hashCode * -1521134295 + this.TextureInfos[0].MipMapCount2.GetHashCode();
            hashCode = hashCode * -1521134295 + this.TextureInfos[0].PixelOffset.GetHashCode();
            hashCode = hashCode * -1521134295 + this.TextureInfos[0].ID.GetHashCode();
            return hashCode;
        }
    }
}
