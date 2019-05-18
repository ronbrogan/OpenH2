using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using Avalonia.Markup.Xaml;
using OpenH2.AvaloniaControls.HexViewer;

namespace OpenH2.ScenarioExplorer
{
    class App : Application
    {
        [STAThread]
        static void Main(string[] args)
        {
            var hexViewer = typeof(HexViewer);

            BuildAvaloniaApp().Start<ScenarioExplorer>();
        }

        /// <summary>
        /// This method is needed for IDE previewer infrastructure
        /// </summary>
        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().LogToDebug().UsePlatformDetect().UseReactiveUI();

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
