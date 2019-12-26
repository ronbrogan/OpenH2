using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace OpenH2.ScenarioExplorer.Preferences
{
    public class PreferencesManager
    {
        private string prefRoot;

        private string appPreferencesPath => Path.Combine(prefRoot, "app.prefs");


        public PreferencesManager()
        {
            var nsRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenH2");
            this.prefRoot = Path.Combine(nsRoot, "ScenarioExplorer");

            Directory.CreateDirectory(prefRoot);
        }

        public AppPreferences LoadAppPreferences()
        {
            if(File.Exists(appPreferencesPath))
            {
                var contents = File.ReadAllText(appPreferencesPath);

                return JsonSerializer.Deserialize<AppPreferences>(contents);
            }

            return new AppPreferences();
        }

        public void StoreAppPreferences(AppPreferences prefs)
        {
            try
            {
                using var fs = new FileStream(appPreferencesPath, FileMode.Create, FileAccess.Write);

                var text = JsonSerializer.Serialize(prefs);

                fs.Write(Encoding.UTF8.GetBytes(text));
            }
            catch(Exception e)
            {
                // log
            }
        }
    }
}
