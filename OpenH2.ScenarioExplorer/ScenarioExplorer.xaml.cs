using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
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
        ScenarioExplorerViewModel DataCtx;

        public ScenarioExplorer()
        {
            DataCtx = new ScenarioExplorerViewModel();
            CreateMenu(DataCtx);
            this.DataContext = DataCtx;

            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

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
                InitialDirectory = "D:\\",
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
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var rawData = file.ToMemory();
                file.Seek(0, SeekOrigin.Begin);

                var factory = new MapFactory(Path.GetDirectoryName(path));
                var scene = factory.FromFile(file);
                var vm = new ScenarioViewModel(scene, rawData, discoveryMode: false);

                DataCtx.LoadedScenario = vm;
            }
        }

        private void CreateMenu(ScenarioExplorerViewModel vm)
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

            foreach(var recent in vm.RecentFiles)
            {
                var item = new MenuItem()
                {
                    Header = Path.GetFileName(recent),
                    Command = ReactiveCommand.Create(() => LoadScenario(recent))
                };

                fileItems.Add(item);
            }

            fileItems.Add(new Separator());
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
            
            vm.MenuItems = new Control[] { file };
        }
    }
}
