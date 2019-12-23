using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
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

            BuildAvaloniaApp().Start(AppMain, args);
        }

        /// <summary>
        /// This method is needed for IDE previewer infrastructure
        /// </summary>
        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().LogToDebug().UsePlatformDetect().UseReactiveUI();

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // Application entry point. Avalonia is completely initialized.
        static void AppMain(Application app, string[] args)
        {
            // Start the main loop
            app.Run(new ScenarioExplorer());
        }
    }
}
