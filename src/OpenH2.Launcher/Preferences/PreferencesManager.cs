using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace OpenH2.Launcher.Preferences
{
    public static class PreferencesManager
    {
        private static object mutex = new object();

        private static string prefRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "OpenH2", 
            "OpenH2.Launcher");
        private static string appPreferencesPath => Path.Combine(prefRoot, "launcher.prefs");

        static PreferencesManager()
        {
            Directory.CreateDirectory(prefRoot);
        }

        public static AppPreferences LoadAppPreferences()
        {
            lock(mutex)
            {
                if (File.Exists(appPreferencesPath))
                {
                    var contents = File.ReadAllText(appPreferencesPath);

                    return JsonSerializer.Deserialize<AppPreferences>(contents);
                }

                return new AppPreferences();
            }
        }

        public static void StoreAppPreferences(AppPreferences prefs)
        {
            lock (mutex)
            {
                try
                {
                    using var fs = new FileStream(appPreferencesPath, FileMode.Create, FileAccess.Write);

                    var text = JsonSerializer.Serialize(prefs);

                    fs.Write(Encoding.UTF8.GetBytes(text));
                }
                catch (Exception e)
                {
                    // log
                }
            }
        }
    }
}
