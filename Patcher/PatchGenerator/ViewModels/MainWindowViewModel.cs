using Avalonia;
using PatchGenerator.Models;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;

namespace PatchGenerator.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IActivatableViewModel, IScreen
    {
        public RoutingState Router { get; } = new RoutingState();
        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        public ReactiveCommand<Unit, Unit> CloseCommand => ReactiveCommand.Create(() =>
        {
            if (Application.Current.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktopApp)
            {
                desktopApp.MainWindow.Close();
            }
        });

        public MainWindowViewModel(GenStartupArgs genArgs = null)
        {
            this.WhenActivated((CompositeDisposable disposables) =>
            {
                if (genArgs != null && genArgs.ReadyToRun)
                {
                    PatchGenInfo genInfo = new PatchGenInfo();

                    genInfo.TargetFolderPath = genArgs.TargetFolderPath;
                    genInfo.SourceFolderPath = genArgs.SourceFolderPath;
                    genInfo.PatchName = genArgs.OutputFolderName;
                    // issues with auto zip, but it's not really used anymore so just disabling for now
                    genInfo.AutoZip = false;
                    genInfo.AutoClose = genArgs.AutoClose;

                    Router.Navigate.Execute(new PatchGenerationViewModel(this, genInfo));
                    return;
                }

                Router.Navigate.Execute(new OptionsViewModel(this));
            });
        }
    }
}
