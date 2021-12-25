using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PatchGenerator.AttachedProperties;

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

        public void scrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                bool autoScroll = scrollViewer.GetValue(RandomBoolAttProp.RandomBoolProperty);

                if (autoScroll)
                {
                    scrollViewer.ScrollToEnd();
                }
            }
        }
    }
}
