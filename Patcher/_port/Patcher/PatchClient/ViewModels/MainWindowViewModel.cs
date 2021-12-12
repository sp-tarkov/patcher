using PatchClient.Models;
using Splat;

namespace PatchClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ViewNavigator navigator { get; set; } = new ViewNavigator();
        public MainWindowViewModel()
        {
            navigator.SelectedViewModel = new PatcherViewModel();

            Locator.CurrentMutable.RegisterConstant(navigator, typeof(ViewNavigator));
        }
    }
}
