using System;
using System.Text.Json;

namespace OpenH2.Core.Patching
{
    public class TagPatch
    {
        public string Name { get; set; }

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
        public byte[] Data { get; set; }
    }
}
