using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OpenH2.ScenarioExplorer.ViewModels;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace OpenH2.ScenarioExplorer
{
    [DoNotNotify]
    public class InternedStrings : Window
    {
        public InternedStrings()
        {
            this.InitializeComponent();
        }

        public void Bind(ScenarioViewModel vm)
        {
            this.DataContext = new InternedStringsViewModel(vm);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public class InternedStringsViewModel
        {


            public InternedStringsViewModel(ScenarioViewModel vm)
            {
                this.Scenario = vm;

                this.FilteredStrings = new ObservableCollection<FilteredStringVm>();

                foreach(var kv in vm.InternedStrings)
                {
                    this.FilteredStrings.Add(new FilteredStringVm(kv.Key, kv.Value));
                }
            }

            private string searchValue = "";
            public string SearchValue
            {
                get { return searchValue; }
                set { FilterStrings(value); }
            }

            public ObservableCollection<FilteredStringVm> FilteredStrings { get; set; }

            public ScenarioViewModel Scenario { get; set; }

            public void FilterStrings(string value)
            {
                this.searchValue = value;
                if (string.IsNullOrWhiteSpace(value))
                {
                    foreach (var kv in this.Scenario.InternedStrings)
                    {
                        this.FilteredStrings.Add(new FilteredStringVm(kv.Key, kv.Value));
                    }
                    return;
                }

                this.FilteredStrings.Clear();

                var regex = new Regex(value, RegexOptions.Compiled);

                foreach (var kv in this.Scenario.InternedStrings)
                {
                    var match = regex.IsMatch(kv.Value);
                    if(match)
                    {
                        this.FilteredStrings.Add(new FilteredStringVm(kv.Key, kv.Value));
                    }
                }
            }

            public class FilteredStringVm
            {
                public FilteredStringVm(int index, string value)
                {
                    this.Index = index;
                    this.Value = value;
                }

                public int Index { get; set; }
                public string Value { get; set; }
            }
        }
    }
}
