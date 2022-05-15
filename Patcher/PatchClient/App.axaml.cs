using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PatchClient.ViewModels;
using PatchClient.Views;

namespace PatchClient
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                bool autoClose = false;

                if(desktop.Args != null && desktop.Args[0].ToLower() == "autoclose")
                    autoClose = true;

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(autoClose),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
