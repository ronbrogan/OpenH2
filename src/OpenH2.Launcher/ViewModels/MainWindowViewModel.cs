using Avalonia.Controls;
using OpenH2.Launcher.Preferences;
using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace OpenH2.Launcher.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class MainWindowViewModel
    {
        private readonly Window window;

        public ObservableCollection<MapEntry> AvailableMaps { get; set; } = new();

        public MapEntry SelectedMap { get; set; }

        public MainWindowViewModel(Window window)
        {
            this.window = window;
            
            if(Directory.Exists(AppPreferences.Current.ChosenMapFolder))
            {
                LoadMaps(AppPreferences.Current.ChosenMapFolder);
            }
        }


        public async Task ChooseMapFolder()
        {
            var dialog = new OpenFolderDialog();
            dialog.Directory = 
                AppPreferences.Current.ChosenMapFolder ??
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft Games", "Halo 2", "maps");
            
            var chosenFolder = await dialog.ShowAsync(this.window);

            AppPreferences.Current.ChosenMapFolder = chosenFolder;
            AppPreferences.StoreCurrent();

            if (string.IsNullOrWhiteSpace(chosenFolder))
            { 
                return;
            }

            LoadMaps(chosenFolder);
        }

        private void LoadMaps(string folder)
        {
            this.AvailableMaps.Clear();

            var maps = Directory.GetFiles(folder, "*.map");

            foreach (var map in maps)
            {
                this.AvailableMaps.Add(new MapEntry(map));
            }
        }

        public void Launch()
        {
            if (this.SelectedMap == null) return;

            EngineConnector.Start(this.SelectedMap.FullPath);
        }

        public void Exit()
        {
            this.Exit();
        }
    }
}
