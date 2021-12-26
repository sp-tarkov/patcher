using ReactiveUI;

namespace PatchGenerator.Models
{
    public class ViewNavigator : ReactiveObject
    {
        private object _SelectedViewModel;
        public object SelectedViewModel
        {
            get => _SelectedViewModel;
            set => this.RaiseAndSetIfChanged(ref _SelectedViewModel, value);
        }
    }
}
