using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PatchGenerator.ViewModels;
using ReactiveUI;

namespace PatchGenerator.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
