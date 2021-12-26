using Avalonia;
using PatchGenerator.Models;
using ReactiveUI;
using Splat;
using System.Windows.Input;

namespace PatchGenerator.ViewModels
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
        public MainWindowViewModel(GenStartupArgs genArgs = null)
        {
            Locator.CurrentMutable.RegisterConstant(navigator, typeof(ViewNavigator));

            if (genArgs != null && genArgs.ReadyToRun)
            {
                PatchGenInfo genInfo = new PatchGenInfo();

                genInfo.TargetFolderPath = genArgs.TargetFolderPath;
                genInfo.SourceFolderPath = genArgs.SourceFolderPath;
                genInfo.PatchName = genArgs.OutputFolderName;
                genInfo.AutoZip = genArgs.AutoZip;

                navigator.SelectedViewModel = new PatchGenerationViewModel(genInfo);
                return;
            }

            navigator.SelectedViewModel = new OptionsViewModel();
        }
    }
}
