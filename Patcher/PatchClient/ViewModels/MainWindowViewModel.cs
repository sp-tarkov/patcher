using Avalonia;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows.Input;

namespace PatchClient.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IActivatableViewModel, IScreen
    {
        public ViewModelActivator Activator { get; } = new ViewModelActivator();
        public RoutingState Router { get; } = new RoutingState();

        public ICommand CloseCommand => ReactiveCommand.Create(() =>
        {
            if (Application.Current.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktopApp)
            {
                desktopApp.MainWindow.Close();
            }
        });

        public MainWindowViewModel(bool autoClose)
        {
            this.WhenActivated((CompositeDisposable disposable) =>
            {
                Router.Navigate.Execute(new PatcherViewModel(this, autoClose));
            });
        }
    }
}
