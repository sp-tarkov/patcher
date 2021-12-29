using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PatchGenerator.AttachedProperties;
using PatchGenerator.ViewModels;
using ReactiveUI;

namespace PatchGenerator.Views
{
    public partial class PatchGenerationView : ReactiveUserControl<PatchGenerationViewModel>
    {
        public PatchGenerationView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables => { });
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
