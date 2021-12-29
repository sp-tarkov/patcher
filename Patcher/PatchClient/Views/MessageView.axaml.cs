using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PatchClient.ViewModels;
using ReactiveUI;

namespace PatchClient.Views
{
    public partial class MessageView : ReactiveUserControl<MessageViewModel>
    {
        public MessageView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
