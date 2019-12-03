using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.ScenarioExplorer.Preferences
{
    public class AppPreferences
    {
        public string[] RecentFiles { get; set; } = new string[0];
        public bool DiscoveryMode = false;
        public string LastBrowseLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }
}
