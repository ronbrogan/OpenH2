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

namespace OpenH2.ScenarioExplorer.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class TagViewModel
    {
        public TagViewModel(uint id, string tag)
        {
            this.Id = id;
            this.Name = tag;
        }

        public uint Id { get; set; }

        public string Name { get; set; }

        public Memory<byte> Data { get; set; }

        public string OriginalTagJson { get; private set; }

        private BaseTag _originalTag;
        public BaseTag OriginalTag
        {
            get => _originalTag;
            set
            {
                _originalTag = value;

                if(value != null)
                {
                    OriginalTagJson = JsonConvert.SerializeObject(value, Formatting.Indented);
                }
            }
        }

        public ObservableCollection<HexViewerFeature> Features { get; set; } = new ObservableCollection<HexViewerFeature>();

        public int InternalOffsetStart { get; set; }

        public int InternalOffsetEnd { get; set; }

        public List<CaoViewModel> Caos { get; set; } = new List<CaoViewModel>();

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
                this.Features.Add(new HexViewerFeature(cao.Origin, 8, Brushes.Goldenrod));
                var chunkFeature = new HexViewerFeature(cao.Offset, cao.Count * cao.ItemSize, Brushes.OliveDrab);

                this.Features.Add(chunkFeature);
            }
        }
    }
}
