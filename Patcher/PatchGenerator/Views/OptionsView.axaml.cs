using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PatchGenerator.ViewModels;
using ReactiveUI;

namespace PatchGenerator.Views
{
    public partial class OptionsView : ReactiveUserControl<OptionsViewModel>
    {
        public OptionsView()
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
