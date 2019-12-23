using Avalonia.Media;
using PropertyChanged;
using System;

namespace OpenH2.AvaloniaControls.HexViewerImpl
{
    [AddINotifyPropertyChangedInterface]
    public class HexViewerFeature
    {
        public int Start { get; set; }
        public int Length { get; set; }
        public IBrush Brush { get; set; }
        public IBrush ActiveBrush { get; set; }

        public Action<HexViewerFeature> OnFocus { get; set; }

        public HexViewerFeature(int start, int length, IBrush brush = null, IBrush activeBrush = null)
        {
            this.Start = start;
            this.Length = length;
            this.Brush = brush ?? Brushes.Black;
            this.ActiveBrush = activeBrush ?? Brushes.Blue;
        }

        
    }
}
