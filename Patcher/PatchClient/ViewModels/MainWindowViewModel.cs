using Avalonia;
using PatchClient.Models;
using ReactiveUI;
using Splat;
using System.Windows.Input;

namespace PatchClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ICommand CloseCommand => ReactiveCommand.Create(() =>
        {
            if (Application.Current.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktopApp)
            {
                desktopApp.MainWindow.Close();
            }
        });

        public ViewNavigator navigator { get; set; } = new ViewNavigator();
        public MainWindowViewModel()
        {
            navigator.SelectedViewModel = new PatcherViewModel();

            Locator.CurrentMutable.RegisterConstant(navigator, typeof(ViewNavigator));
        }
    }
}
