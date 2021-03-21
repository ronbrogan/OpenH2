using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OpenH2.Launcher.ViewModels;
using PropertyChanged;

namespace OpenH2.Launcher
{
    [DoNotNotify]
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            this.DataContext = new MainWindowViewModel(this);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
