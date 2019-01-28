using System;
using System.Collections.Generic;
using System.Text;
using PropertyChanged;

namespace OpenH2.ScenarioExplorer.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class CaoViewModel
    {
        public int Offset { get; set; }

        public int Count { get; set; }
    }
}
