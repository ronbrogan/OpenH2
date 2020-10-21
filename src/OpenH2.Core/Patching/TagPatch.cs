using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenH2.Core.Patching
{
    public class TagPatch
    {
        public string TagName { get; set; }

        public TagPropertyPatch[] PropertyPatches { get; set; } = Array.Empty<TagPropertyPatch>();
        public TagBinaryPatch[] BinaryPatches { get; set; } = Array.Empty<TagBinaryPatch>();
    }

    public class TagPropertyPatch
    {
        public string PropertySelector { get; set; }

        public JsonElement Value { get; set; }
    }

    public class TagBinaryPatch
    {
        /// <summary>
        /// The offset of the data to modify, relative to the start of the tag data
        /// </summary>
        public uint RelativeOffset { get; set; }

        /// <summary>
        /// The data to replace with
        /// </summary>
        [JsonConverter(typeof(HexBytesConverter))]
        public byte[] Data { get; set; }

        private class HexBytesConverter : JsonConverter<byte[]>
        {
            public override bool CanConvert(Type typeToConvert)
            {
                return typeToConvert == typeof(byte[]);
            }

            public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var hex = reader.GetString();

                if (hex.Length % 2 == 1)
                    throw new Exception("Hex data must have an even number of characters");

                var arr = new byte[hex.Length >> 1];

                for (int i = 0; i < hex.Length >> 1; ++i)
                {
                    arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
                }

                return arr;

                int GetHexVal(char hex)
                {
                    int val = (int)hex;
                    //For uppercase A-F letters:
                    //return val - (val < 58 ? 48 : 55);
                    //For lowercase a-f letters:
                    //return val - (val < 58 ? 48 : 87);
                    //Or the two combined, but a bit slower:
                    return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
                }
            }

            public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }

    }
}
