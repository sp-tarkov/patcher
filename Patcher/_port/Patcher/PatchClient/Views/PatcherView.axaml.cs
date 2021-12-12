using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PatchClient.Views
{
    public partial class PatcherView : UserControl
    {
        public PatcherView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
