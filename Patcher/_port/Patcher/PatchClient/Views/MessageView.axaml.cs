using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PatchClient.Views
{
    public partial class MessageView : UserControl
    {
        public MessageView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
