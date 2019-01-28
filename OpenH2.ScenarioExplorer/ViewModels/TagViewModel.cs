using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Media;
using OpenH2.Avalonia;
using OpenH2.Core.Extensions;
using PropertyChanged;

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

        public ObservableCollection<HexViewerSpan> FormattedSpans { get; set; } = new ObservableCollection<HexViewerSpan>();

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
                    var cao = new CaoViewModel()
                    {
                        Offset = val - InternalOffsetStart,
                        Count = span.ReadInt32At(i - 4)
                    };

                    this.Caos.Add(cao);
                    this.FormattedSpans.Add(new HexViewerSpan(i - 4, 8, Brushes.Goldenrod));
                    this.FormattedSpans.Add(new HexViewerSpan(cao.Offset, cao.Count, Brushes.OliveDrab));
                }

            }


        }
    }
}
