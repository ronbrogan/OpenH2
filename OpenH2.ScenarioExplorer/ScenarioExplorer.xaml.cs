﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.ScenarioExplorer.ViewModels;
using PropertyChanged;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;

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
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async Task FileOpenClick()
        {
            var open = new OpenFileDialog
            {
                AllowMultiple = false,
                InitialDirectory = "D:\\",
                Title = "Open .map"
            };

            var result = await open.ShowAsync();

            if (result.Any() && result[0].IsSignificant() && File.Exists(result[0]))
            {
                LoadScenario(result[0]);
            }
        }

        private void LoadScenario(string path)
        {
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var factory = new SceneFactory();
                var scene = factory.FromFile(file);
                var vm = new ScenarioViewModel(scene);

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