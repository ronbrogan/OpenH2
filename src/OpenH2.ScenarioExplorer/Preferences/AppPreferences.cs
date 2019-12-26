using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.ScenarioExplorer.Preferences
{
    public class AppPreferences
    {
        public string[] RecentFiles { get; set; } = new string[0];
        public bool DiscoveryMode { get; set; } = false;
        public string LastBrowseLocation { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }
}
