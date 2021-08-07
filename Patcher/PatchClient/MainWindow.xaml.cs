using PatchClient.Extensions;
using PatcherUtils;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PatchClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo() { ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal };
        }

        private void RunPatcher()
        {
            Task.Run(() =>
            {
                FilePatcher bp = new FilePatcher()
                {
                    TargetBase = Environment.CurrentDirectory,
                    PatchBase = LazyOperations.PatchFolder.FromCwd()
                };

                bp.ProgressChanged += Bp_ProgressChanged;

                try
                {
                    if (bp.Run())
                    {
                        MessageBox.Show("Patch completed without issues", "Patching Successful");
                    }
                    else
                    {
                        MessageBox.Show("Failed to patch client.", "Patching Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Application.Current.Shutdown(0);
                    });
                }
            });
        }

        private void Bp_ProgressChanged(object Sender, int Progress, int Total, int Percent, string Message = "", params LineItem[] AdditionalLineItems)
        {
            string additionalInfo = "";
            foreach (LineItem item in AdditionalLineItems)
            {
                additionalInfo += $"{item.ItemText}: {item.ItemValue}\n";
            }


            PatchProgressBar.DispatcherSetValue(Percent);

            if (!string.IsNullOrWhiteSpace(Message))
            {
                PatchMessageLabel.DispaatcherSetContent(Message);
            }

            PatchProgressInfoLabel.DispaatcherSetContent($"[{Progress}/{Total}]");

            AdditionalInfoBlock.DispatcherSetText(additionalInfo);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RunPatcher();
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        private void label_topbar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
