using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PatchClient.ViewModels;
using PatchClient.Views;
using ReactiveUI;
using System.Reactive;
using System;
using System.Linq;
using PatcherUtils.Model;

namespace PatchClient
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            RxApp.DefaultExceptionHandler = Observer.Create<Exception>((exception) =>
            {
                PatchLogger.LogException(exception);
            });
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                bool autoClose = false;
                bool debugOutput = false;

                if (desktop.Args != null && desktop.Args.Length >= 1)
                {
                    autoClose = desktop.Args.Any(x => x.ToLower() == "autoclose");
                    debugOutput = desktop.Args.Any(x => x.ToLower() == "debug");
                }

                if (debugOutput)
                {
                    PatchLogger.LogInfo("Running in debug mode");
                }

                if (autoClose)
                {
                    PatchLogger.LogInfo("Running with autoclose");
                }

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(autoClose, debugOutput),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
