using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PatchGenerator.Views
{
    public partial class PatchGenerationView : UserControl
    {
        public PatchGenerationView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
