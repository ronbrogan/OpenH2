using OpenH2.Core.Tags;
using OpenH2.Foundation;
using System.Text.Json.Serialization;

namespace OpenH2.Core.Configuration
{
    public class MaterialMapping
    {
        public string Name { get; set; }

        public int? DiffuseMapIndex { get; set; } = null;
        public int[] DiffuseColor { get; set; } = new int[0];

        public int? NormalMapIndex { get; set; } = null;
        public int? NormalScaleIndex { get; set; } = null;

        public int? Detail1MapIndex { get; set; } = null;
        public int? Detail1ScaleIndex { get; set; } = null;

        public int? Detail2MapIndex { get; set; } = null;
        public int? Detail2ScaleIndex { get; set; } = null;

        public int? EmissiveMapIndex { get; set; } = null;
        public int? EmissiveArgumentsIndex { get; set; } = null;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EmissiveType EmissiveType { get; set; } = EmissiveType.DiffuseBlended;

        public int? AlphaMapIndex { get; set; } = null;
        public bool AlphaFromRed { get; set; }

        public int? ColorChangeMaskMapIndex { get; set; } = null;

        // TODO: implement animation map
        public int? AnimationMapIndex { get; set; } = null;
        public int? SpecularMapIndex { get; set; } = null;
    }
}