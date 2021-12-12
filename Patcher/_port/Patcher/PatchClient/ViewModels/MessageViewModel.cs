using Avalonia;
using ReactiveUI;

namespace PatchClient.ViewModels
{
    public class MessageViewModel : ViewModelBase
    {
        private string _InfoText;
        public string InfoText
        {
            get => _InfoText;
            set => this.RaiseAndSetIfChanged(ref _InfoText, value);
        }

        public MessageViewModel(string Message)
        {
            InfoText = Message;
        }
    }
}
