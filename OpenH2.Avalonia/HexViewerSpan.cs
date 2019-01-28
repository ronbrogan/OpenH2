using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Media;

namespace OpenH2.Avalonia
{
    public class HexViewerSpan
    {
        public int start;
        public int length;
        public IBrush brush;

        public HexViewerSpan(int start, int length, IBrush brush = null)
        {
            this.start = start;
            this.length = length;
            this.brush = brush ?? Brushes.Black;
        }
    }
}
