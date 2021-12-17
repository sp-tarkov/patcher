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
        }

        private void RunPatcher()
        {
            Task.Run(() =>
            {
                PatchHelper patcher = new PatchHelper(Environment.CurrentDirectory, null, LazyOperations.PatchFolder.FromCwd());

                patcher.ProgressChanged += patcher_ProgressChanged;

                try
                {
                    string message = patcher.ApplyPatches();

                    MessageBox.Show(message, "Patcher");
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

        private void patcher_ProgressChanged(object Sender, int Progress, int Total, int Percent, string Message = "", params LineItem[] AdditionalLineItems)
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
