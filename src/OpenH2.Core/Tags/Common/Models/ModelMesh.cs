using OpenH2.Core.Maps;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenH2.Core.Tags.Common.Models
{
    public class ModelMesh : IEquatable<ModelMesh>
    {
        [JsonIgnore]
        public int[] Indices { get; set; }
        [JsonIgnore]
        public VertexFormat[] Verticies { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MeshElementType ElementType { get; set; }

        public TagRef<ShaderTag> Shader { get; set; }
        public bool Compressed { get; set; }

        [JsonIgnore]
        public byte[] RawData { get; set; }

        public string Note { get; set; }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as ModelMesh);
        }

        public bool Equals(ModelMesh other)
        {
            return other != null &&
                   EqualityComparer<int[]>.Default.Equals(this.Indices, other.Indices) &&
                   EqualityComparer<VertexFormat[]>.Default.Equals(this.Verticies, other.Verticies) &&
                   this.ElementType == other.ElementType &&
                   EqualityComparer<TagRef<ShaderTag>>.Default.Equals(this.Shader, other.Shader) &&
                   this.Compressed == other.Compressed &&
                   EqualityComparer<byte[]>.Default.Equals(this.RawData, other.RawData) &&
                   this.Note == other.Note;
        }

        public override int GetHashCode()
        {
            var hashCode = 2053650439;
            hashCode = hashCode * -1521134295 + EqualityComparer<int[]>.Default.GetHashCode(this.Indices);
            hashCode = hashCode * -1521134295 + EqualityComparer<VertexFormat[]>.Default.GetHashCode(this.Verticies);
            hashCode = hashCode * -1521134295 + this.ElementType.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Shader.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Compressed.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode(this.RawData);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Note);
            return hashCode;
        }
    }
}
