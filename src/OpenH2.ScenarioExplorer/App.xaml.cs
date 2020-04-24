using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging.Serilog;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using OpenH2.AvaloniaControls;
using PropertyChanged;

namespace OpenH2.ScenarioExplorer
{
    [DoNotNotifyAttribute]
    class App : Application
    {
        public static string[] StartupArgs;


        [STAThread]
        static void Main(string[] args)
        {
            var hexViewer = typeof(HexViewer);
            StartupArgs = args;

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        /// <summary>
        /// This method is needed for IDE previewer infrastructure
        /// </summary>
        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().LogToDebug().UsePlatformDetect().UseReactiveUI();

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.MainWindow = new ScenarioExplorer();

            base.OnFrameworkInitializationCompleted();
        }
    }
}
