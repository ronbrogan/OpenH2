using Avalonia.Media;
using OpenH2.AvaloniaControls.HexViewerImpl;
using OpenH2.Core.Extensions;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Common;
using OpenH2.Core.Types;
using OpenH2.Foundation;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenH2.ScenarioExplorer.ViewModels
{

    [AddINotifyPropertyChangedInterface]
    public class TagViewModel
    {
        private static JsonSerializerOptions serializerOptions = new JsonSerializerOptions() { WriteIndented = true };

        static TagViewModel()
        {
            serializerOptions.Converters.Add(new Vector2Converter());
            serializerOptions.Converters.Add(new Vector3Converter());
            serializerOptions.Converters.Add(new Vector4Converter());
            serializerOptions.Converters.Add(new NopConverter());
            serializerOptions.Converters.Add(new InternedStringConverter());
        }


        public TagViewModel(uint id, string tag, string name)
        {
            this.Id = id;
            this.Name = tag + (name != null ? " - " + name : string.Empty);
        }

        public uint Id { get; set; }

        public string Name { get; set; }

        public Memory<byte> Data { get; set; }

        private string _tagJson;
        public string OriginalTagJson { get
            {
                if(_tagJson == null)
                {
                    try
                    {
                        var json = JsonSerializer.Serialize(_originalTag, _originalTag.GetType(), serializerOptions);
                        _tagJson = json;
                    }
                    catch (Exception e) { Console.WriteLine(e.ToString()); }
                    
                    return _tagJson;
                }
                else
                {
                    return _tagJson;
                }
            }
        }

        private BaseTag _originalTag;

        [DoNotCheckEquality]
        public BaseTag OriginalTag
        {
            get => _originalTag;
            set
            {
                _originalTag = value;
                this.TagPreview = TagPreviewFactory.GetPreview(_originalTag);

                if(value != null)
                {
                    _tagJson = null;
                }
            }
        }

        public TagPreviewViewModel TagPreview { get; set; }

        public ObservableCollection<HexViewerFeature> Features { get; set; } = new ObservableCollection<HexViewerFeature>();

        public bool IsPostProcessed => Data.Length == 0 || (Features?.Any() ?? false);

        public int InternalOffsetStart { get; set; }

        public int InternalOffsetEnd { get; set; }

        public List<CaoViewModel> Caos { get; set; } = new List<CaoViewModel>();
        public int RawOffset { get; internal set; }

        public void GeneratePointsOfInterest(H2vMap scene)
        {
            // go over all data, create entries for:
            //  - internal offset and count entries
            //  - tag references
            //  - ? 

            if(IsPostProcessed)
            {
                return;
            }

            var internedStringRefs = new List<int>();

            var span = this.Data.Span;

            for (var i = 0; i < this.Data.Length; i += 4)
            {
                var val = span.ReadUInt32At(i);

                if(val > this.InternalOffsetStart && val < this.InternalOffsetEnd)
                {
                    var cao = new CaoViewModel(i-4)
                    {
                        Offset = (int)val - InternalOffsetStart,
                        Count = span.ReadInt32At(i - 4)
                    };

                    this.Caos.Add(cao);
                }

                var internedStringIndex = val & 0xFFFFFF;
                var internedStringLength = (byte)(val >> 24);

                if(internedStringIndex > 0
                    && scene.InternedStrings.TryGetValue((int)internedStringIndex, out var str) 
                    && str.Length == internedStringLength)
                {
                    internedStringRefs.Add(i);
                }
            }

            if(this.Caos.Count == 0)
            {
                return;
            }

            var firstOffset = this.Caos.Select(c => c.Offset).Min();
            var headerCaos = this.Caos.Where(c => c.Origin < firstOffset).OrderBy(c => c.Offset).ToList();

            for(var i = 0; i < headerCaos.Count(); i++)
            {
                var currentCao = headerCaos[i];
                var nextCaoStart = i+1 == headerCaos.Count() ? this.Data.Length : headerCaos[i + 1].Offset;

                var gap = nextCaoStart - currentCao.Offset;

                currentCao.ItemSize = gap / currentCao.ItemSize;
            }


            foreach(var cao in this.Caos)
            {
                this.Features.Add(new HexViewerFeature(cao.Origin, 8, Brushes.Goldenrod));
                var chunkFeature = new HexViewerFeature(cao.Offset, cao.Count * cao.ItemSize, Brushes.OliveDrab);

                this.Features.Add(chunkFeature);
            }

            foreach(var str in internedStringRefs)
            {
                this.Features.Add(new HexViewerFeature(str, 4, Brushes.Red));
            }
        }

        public class NopConverter : JsonConverter<object>
        {
            public NopConverter()
            { }

            public override bool CanConvert(Type type)
            {
                return type == typeof(VertexFormat[])
                || type == typeof(Vertex[])
                || type == typeof(byte[]);
            }

            public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return null;
            }

            public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
            {
                writer.WriteNullValue();
            }
        }

        public class Vector2Converter : JsonConverter<Vector2>
        {

            public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Vector2);

            public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

            public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(string.Format("({0}, {1})", value.X, value.Y));
            }
        }

        public class Vector3Converter : JsonConverter<Vector3>
        {

            public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Vector3);

            public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

            public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(string.Format("({0}, {1}, {2})", value.X, value.Y, value.Z));
            }
        }

        public class Vector4Converter : JsonConverter<Vector4>
        {

            public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Vector4);

            public override Vector4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

            public override void Write(Utf8JsonWriter writer, Vector4 value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(string.Format("({0}, {1}, {2}, {3})", value.X, value.Y, value.Z, value.W));
            }
        }

        public class InternedStringConverter : JsonConverter<InternedString>
        {
            public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(InternedString);

            public override InternedString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

            public override void Write(Utf8JsonWriter writer, InternedString value, JsonSerializerOptions options)
            {
                var strValue = value.Value ?? (value.Id != ushort.MaxValue ? value.Id.ToString() : null);

                writer.WriteStringValue(strValue);
            }
        }
    }
}
