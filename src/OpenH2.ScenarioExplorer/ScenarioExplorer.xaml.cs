using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.ScenarioExplorer.Preferences;
using OpenH2.ScenarioExplorer.ViewModels;
using PropertyChanged;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenH2.ScenarioExplorer
{
    [DoNotNotify]
    public class ScenarioExplorer : Window
    {
        private ScenarioExplorerViewModel DataCtx;
        private PreferencesManager prefManager;
        private AppPreferences prefs;
        private List<Window> childWindows = new List<Window>();

        private string loadedMap = null;

        public ScenarioExplorer()
        {
            prefManager = new PreferencesManager();
            prefs = prefManager.LoadAppPreferences();
            DataCtx = new ScenarioExplorerViewModel();
            CreateMenu(prefs.RecentFiles, DataCtx);
            this.DataContext = DataCtx;

            this.InitializeComponent();

            if(App.StartupArgs.Length > 0)
            {
                LoadScenario(App.StartupArgs[0]);
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.Get<TextBox>("gotoTagBox").KeyDown += this.GotoTagBoxKeyDown;

            var flipVertically = Matrix.CreateScale(1, -1);
            this.Get<Image>("bitmPreviewImage").RenderTransform = new MatrixTransform(flipVertically);
        }

        public void CopyData(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            _ = App.Current.Clipboard.SetTextAsync(button.Content.ToString());
        }

        public void GotoAddress(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var offset = int.Parse(button.Content.ToString());

            this.DataCtx.SelectedOffset = offset;
        }

        private void GotoTagBoxKeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            var box = (TextBox)sender;

            if (e.Key == Avalonia.Input.Key.Enter)
            {
                if (uint.TryParse(box.Text, out var id))
                {
                    var roots = DataCtx.LoadedScenario.TreeRoots;
                    
                    foreach(var root in roots)
                    {
                        if(root.Id == id)
                        {
                            DataCtx.SelectedEntry = root;
                            break;
                        }
                        else
                        {
                            var found = TryFindChild(root, id, out var item);

                            if (found)
                            {
                                DataCtx.SelectedEntry = item;
                                break;
                            }
                        }
                    }
                }
            }

            bool TryFindChild(TagTreeEntryViewModel model, uint id, out TagTreeEntryViewModel result)
            {
                if(model.Children == null)
                {
                    result = null;
                    return false;
                }

                foreach(var child in model.Children)
                {
                    if (child.Id == id)
                    {
                        result = child;
                        return true;
                    }
                    else
                    {
                        if(TryFindChild(child, id, out result))
                        {
                            return true;
                        }
                    }
                }

                result = null;
                return false;
            }
        }

        private async Task FileOpenClick()
        {
            var open = new OpenFileDialog
            {
                AllowMultiple = false,
                Directory = prefs.LastBrowseLocation,
                Title = "Open .map"
            };

            var result = await open.ShowAsync(this);

            if (result.Any() && result[0].IsSignificant() && File.Exists(result[0]))
            {
                try
                {
                    LoadScenario(result[0]);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public void LoadScenario(string path)
        {
            foreach(var c in childWindows)
            {
                c.Close();
            }

            childWindows.Clear();

            if (string.IsNullOrEmpty(path))
            {
                prefManager.StoreAppPreferences(prefs);
                return;
            }

            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var rawData = file.ToMemory();
                file.Seek(0, SeekOrigin.Begin);

                var factory = new MapFactory(Path.GetDirectoryName(path), new MaterialFactory(Environment.CurrentDirectory + "/Configs"));
                var scene = factory.FromFile(file);
                var vm = new ScenarioViewModel(scene, rawData, prefs.DiscoveryMode);

                DataCtx.LoadedScenario = vm;
                DataCtx.SelectedEntry = vm.TreeRoots[0];
            }

            loadedMap = path;
            this.Title = $"OpenH2 Scenario Explorer - {Path.GetFileName(path)} {(prefs.DiscoveryMode ? "[Discovery Mode]" : "")}";

            prefs.LastBrowseLocation = Path.GetDirectoryName(path);

            var list = prefs.RecentFiles.ToList();
            list.Insert(0, path);
            prefs.RecentFiles = list.Take(5).Distinct().ToArray();

            prefManager.StoreAppPreferences(prefs);
        }

        public void ToggleDisoveryMode()
        {
            prefs.DiscoveryMode = !prefs.DiscoveryMode;
            LoadScenario(loadedMap);
        }

        public void ShowInternedStringsView()
        {
            var interned = new InternedStrings();
            interned.Bind(this.DataCtx.LoadedScenario);
            interned.Show();

            childWindows.Add(interned);
        }


        private void CreateMenu(string[] recents, ScenarioExplorerViewModel vm)
        {
            /*
             *<MenuItem Header="File">
               <MenuItem Header="Open" Click="FileOpenClick"></MenuItem>
               <MenuItem Header="Exit"></MenuItem>
               </MenuItem>
               <MenuItem Header="Help">
               <MenuItem Header="About"></MenuItem>
               </MenuItem>
             *
             *
             */

            var fileItems = new List<Control>{
                new MenuItem
                {
                    Header = "Open",
                    Command = ReactiveCommand.CreateFromTask(FileOpenClick)
                },
                new Separator()
            };

            if(recents.Any())
            {
                foreach (var recent in recents)
                {
                    var item = new MenuItem()
                    {
                        Header = Path.GetFileName(recent),
                        Command = ReactiveCommand.Create(() => LoadScenario(recent))
                    };

                    fileItems.Add(item);
                }

                fileItems.Add(new Separator());
            }
            
            fileItems.Add(new MenuItem
            {
                Header = "Exit",
                Command = ReactiveCommand.Create(this.Close)
            });

            var file = new MenuItem
            {
                Header = "File",
                Items = fileItems
            };

            var view = new MenuItem
            {
                Header = "View",
                Items = new List<Control>
                {
                    new MenuItem()
                    {
                        Header = "Interned Strings",
                        Command = ReactiveCommand.Create(ShowInternedStringsView)
                    }
                }
            };

            var edit = new MenuItem
            {
                Header = "Edit",
                Items = new List<Control>
                {
                    new MenuItem()
                    {
                        Header = "Toggle Mode",
                        Command = ReactiveCommand.Create(ToggleDisoveryMode)
                    }
                }
            };
            
            vm.MenuItems = new Control[] { file, view, edit };
        }
    }
}
