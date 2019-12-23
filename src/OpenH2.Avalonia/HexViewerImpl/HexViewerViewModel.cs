using Avalonia.Media;
using PropertyChanged;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenH2.AvaloniaControls.HexViewerImpl
{
    [AddINotifyPropertyChangedInterface]
    public class HexViewerViewModel
    {
        public HexViewerViewModel()
        {
            this.HighlightColor = Colors.First(c => c.Name == "HotPink");
            this.HighlightSize = 32;
        }

        public int HighlightSize { get; private set; }

        public List<int> HighlightSizes { get; set; } = new List<int>() { 8, 16, 32 };

        public List<BrushDetails> Colors { get; set; } = typeof(Brushes)
            .GetProperties()
            .Select(p => new BrushDetails(p))
            .ToList();

        public BrushDetails HighlightColor { get; set; } 
    }

    [AddINotifyPropertyChangedInterface]
    public class BrushDetails
    {
        public BrushDetails(PropertyInfo pi)
        {
            this.Name = pi.Name;
            this.Brush = pi.GetValue(null) as IBrush;
        }

        public string Name { get; set; }

        public IBrush Brush { get; set; }
    }
}
