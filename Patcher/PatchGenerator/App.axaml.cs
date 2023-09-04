using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PatchGenerator.Models;
using PatchGenerator.ViewModels;
using PatchGenerator.Views;
using ReactiveUI;
using System.Reactive;
using System;
using PatcherUtils.Model;

namespace PatchGenerator
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
                desktop.Startup += Desktop_Startup;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void Desktop_Startup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
        {
            if (sender is IClassicDesktopStyleApplicationLifetime desktop)
            {
                GenStartupArgs genArgs = GenStartupArgs.Parse(e.Args);

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(genArgs)
                };
            }
        }
    }
}
