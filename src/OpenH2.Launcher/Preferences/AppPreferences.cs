using System;

namespace OpenH2.Launcher.Preferences
{
    public class AppPreferences
    {
        static AppPreferences()
        {
            Current = PreferencesManager.LoadAppPreferences();
        }

        public static void StoreCurrent()
        {
            PreferencesManager.StoreAppPreferences(AppPreferences.Current);
        }

        public static AppPreferences Current { get; private set; }

        public string? ChosenMapFolder { get; set; }
    }
}
