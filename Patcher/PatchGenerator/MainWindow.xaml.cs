using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using PatcherUtils;

namespace PatchGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string compareFolder = "";
        private string targetFolder = "";
        private readonly string patchFolder = "Aki_Data/Patcher/".FromCwd();
        private Stopwatch stopwatch = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();
        }

        private string GetStopWatchTime()
        {
            return $"Hours: {stopwatch.Elapsed.Hours} - Mins: {stopwatch.Elapsed.Minutes} - Secs: {stopwatch.Elapsed.Seconds} - MilliSecs: {stopwatch.Elapsed.Milliseconds}";
        }

        private static bool FileDropCheck(DragEventArgs args, ref string str)
        {
            if (!args.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return false;
            }

            string[] paths = (string[])args.Data.GetData(DataFormats.FileDrop);

            if (paths.Length != 1) return false;

            if (!Directory.Exists(paths[0]))
            {
                return false;
            }

            str = paths[0];

            return true;
        }

        private void CompareLabel_Drop(object sender, DragEventArgs e)
        {
            if (FileDropCheck(e, ref compareFolder))
            { 
                CompareLabel.Content = $"Compare Folder:\n{compareFolder}";
                CompareLabel.BorderBrush = Brushes.DarkCyan;
            }
            else
            {
                MessageBox.Show("Dropped File/s could not be used. Make sure you only drop one folder.");
            }
        }

        private void TargetLabel_Drop(object sender, DragEventArgs e)
        {
            if(FileDropCheck(e, ref targetFolder))
            {
                TargetLabel.Content = $"Target Folder:\n{targetFolder}";
                TargetLabel.BorderBrush = Brushes.DarkCyan;
            }
            else 
            {
                MessageBox.Show("Dropped File/s could not be used. Make sure you only drop one folder.");
            }
        }

        private void GeneratePatches()
        {
            //create temp data
            Application.Current.Dispatcher.Invoke(() =>
            {
                GenProgressBar.IsIndeterminate = true;
                GenProgressMessageLabel.Content = "Extracting temp data ...";
            });

            LazyOperations.PrepTempDir();

            Application.Current.Dispatcher.Invoke(() =>
            {
                GenProgressBar.IsIndeterminate = false;
            });


            //generate patches
            
            FileCompare bc = new FileCompare(targetFolder, compareFolder, patchFolder);

            bc.ProgressChanged += Bc_ProgressChanged;

            if (!bc.CompareAll())
            {
                MessageBox.Show("Failed to generate diffs.", ":(", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //TODO - Build patcher

            //TODO - compress to file (should add a name textbox or something)

            //Cleanup temp data
            Application.Current.Dispatcher.Invoke(() =>
            {
                GenProgressBar.Value = 100;
                GenProgressMessageLabel.Content = $"Done";
            });

            if (!LazyOperations.CleanupTempDir()) 
            {
                MessageBox.Show($"Looks like some temp files could not be removed. You can safely delete this folder:\n\n{LazyOperations.TempDir}");
            }
        }

        private void Bc_ProgressChanged(object Sender, int Progress, int Total, int Percent, string Message = "", params LineItem[] AdditionalLineItems)
        {

            string additionalInfo = "";
            foreach(LineItem item in AdditionalLineItems)
            {
                additionalInfo += $"{item.ItemText}: {item.ItemValue}\n";
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                GenProgressBar.Value = Percent;

                if (!string.IsNullOrWhiteSpace(Message))
                {
                    GenProgressMessageLabel.Content = Message;
                }

                GenProgressInfoLabel.Content = $"[{Progress}/{Total}]";

                AdditionalInfoBlock.Text = additionalInfo;
            });
        }

        private void GenButton_Click(object sender, RoutedEventArgs e)
        {
            GenButton.IsEnabled = false;

            Task.Run(() =>
            {
                stopwatch.Reset();
                stopwatch.Start();

                try
                {
                    GeneratePatches();
                }
                finally
                {
                    stopwatch.Stop();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        GenButton.IsEnabled = true;
                        GenProgressMessageLabel.Content = "";
                        GenProgressInfoLabel.Content = $"Patches Generated in: {GetStopWatchTime()}";
                    });
                }
            });
        }
    }
}
