using Avalonia.Media;
using OpenH2.AvaloniaControls.HexViewer;
using OpenH2.Core.Extensions;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using OpenH2.Core.Tags;
using Newtonsoft.Json.Serialization;
using OpenH2.Foundation;
using OpenH2.Core.Types;
using OpenH2.Core.Tags.Common;

namespace OpenH2.ScenarioExplorer.ViewModels
{
    public class FriendlyContractResolver : DefaultContractResolver
    {
        private HashSet<Type> bannedTypes = new HashSet<Type>()
        {
            typeof(VertexFormat[]),
            typeof(Vertex[]),
            typeof(MeshCollection)
        };

        public override JsonContract ResolveContract(Type type)
        {
            var contract = base.CreateContract(type);

            if(bannedTypes.Contains(type))
            {
                contract.Converter = null;
            }

            return contract;
        }
    }

    [AddINotifyPropertyChangedInterface]
    public class TagViewModel
    {
        private static IContractResolver resolver = new FriendlyContractResolver();

        private static JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = resolver
        };

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
                    var json = JsonConvert.SerializeObject(_originalTag, Formatting.Indented, serializerSettings);

                    _tagJson = json;
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

        public int InternalOffsetStart { get; set; }

        public int InternalOffsetEnd { get; set; }

        public List<CaoViewModel> Caos { get; set; } = new List<CaoViewModel>();
        public int RawOffset { get; internal set; }

        public void GeneratePointsOfInterest()
        {
            // go over all data, create entries for:
            //  - internal offset and count entries
            //  - tag references
            //  - ? 

            var span = this.Data.Span;

            for (var i = 0; i < this.Data.Length; i += 4)
            {
                var val = span.ReadInt32At(i);

                if(val > this.InternalOffsetStart && val < this.InternalOffsetEnd)
                {
                    var cao = new CaoViewModel(i-4)
                    {
                        Offset = val - InternalOffsetStart,
                        Count = span.ReadInt32At(i - 4)
                    };

                    this.Caos.Add(cao);
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
                Console.WriteLine($"{cao.Origin}\t{cao.Count}\t{cao.Offset}");

                this.Features.Add(new HexViewerFeature(cao.Origin, 8, Brushes.Goldenrod));
                var chunkFeature = new HexViewerFeature(cao.Offset, cao.Count * cao.ItemSize, Brushes.OliveDrab);

                this.Features.Add(chunkFeature);
            }
        }
    }
}
