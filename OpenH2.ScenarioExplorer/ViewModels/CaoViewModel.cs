using System;
using System.Collections.Generic;
using System.Text;
using PropertyChanged;

namespace OpenH2.ScenarioExplorer.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class CaoViewModel
    {
        public CaoViewModel(int origin)
        {
            this.Origin = origin;
        }

        public int Origin { get; set; }

        public int Offset { get; set; }

        public int Count { get; set; }

        public int ItemSize { get; set; } = 1;
    }
}
